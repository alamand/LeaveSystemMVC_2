using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrEditBalanceController : BaseController
    {
        // GET: hrEditBalance
        public ActionResult Index(int filterDepartmentID = 0, int filterAccStatus = -1, string filterSearch = "", string filterOrderBy = "")
        {
            var model = GetFilteredBalances(filterDepartmentID, filterAccStatus, filterSearch, filterOrderBy);
            Dictionary dic = new Dictionary();

            ViewData["DepartmentList"] = dic.AddDefaultToDictionary(dic.GetDepartment(), 0, "All Departments");     // @TODO: change dic.GetDepartment() to dic.GetDepartmentName()
            ViewData["SelectedDepartment"] = filterDepartmentID;
            ViewData["AccountStatusList"] = AccountStatusList();
            ViewData["SelectedAccStatus"] = filterAccStatus;
            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["EnteredSearch"] = filterSearch;
            ViewData["ReligionList"] = dic.GetReligionName();

            return View(model);
        }

        [HttpPost]
        public ActionResult Filter(FormCollection form)
        {
            int deptID = Convert.ToInt32(form["selectedDepartment"]);
            int accStatID = Convert.ToInt32(form["SelectedAccStatus"]);
            string search = form["enteredSearch"];
            string orderBy = form["selectedOrderBy"];
            return RedirectToAction("Index", new { filterDepartmentID = deptID, filterAccStatus = accStatID, filterSearch = search, filterOrderBy = orderBy });
        }

        private List<Tuple<Employee, Balance>> GetFilteredBalances(int deptID, int accStat, string search, string order)
        {
            List<Tuple<Employee, Balance>> model = new List<Tuple<Employee, Balance>>();
            Dictionary dic = new Dictionary();

            int staffRoleID = dic.GetRole().FirstOrDefault(obj => obj.Value == "Staff").Key;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@staffRoleID", SqlDbType.Int).Value = staffRoleID;
            cmd.CommandText = "SELECT Employee.Employee_ID FROM dbo.Employee, dbo.Employee_Role " +
                "WHERE Employee.Employee_ID = Employee_Role.Employee_ID AND Employee_Role.Role_ID = " + staffRoleID;

            // adds a filter query if a department is selected from the dropdown, note that 0 represents All Departments
            if (deptID > 0)
            {
                cmd.Parameters.Add("@deptID", SqlDbType.Int).Value = deptID;
                cmd.CommandText += " AND Department_ID = @deptID";
            }

            // adds a filter query if a account status is selected from the dropdown, note that -1 represents Active/InActive
            if (accStat >= 0)
            {
                cmd.Parameters.Add("@accStat", SqlDbType.Bit).Value = accStat;
                cmd.CommandText += " AND Account_Status = @accStat";
            }

            // adds a filter query if search box contains character(s), note that 0 length means the search box is empty
            if (search.Length > 0)
            {
                cmd.Parameters.Add("@search", SqlDbType.NChar).Value = search;
                cmd.CommandText += " AND (Employee.Employee_ID LIKE '%' + @search + '%' " +
                    "OR CONCAT(First_Name, ' ', Last_Name) LIKE '%' + @search + '%')";
            }

            if (order.Length > 0)
            {
                cmd.CommandText += " ORDER BY " + order;
            }

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                Employee employee = GetEmployeeModel((int)row["Employee_ID"]);
                Balance leaveBalance = GetLeaveBalanceModel((int)row["Employee_ID"]);
                model.Add(new Tuple<Employee, Balance>(employee, leaveBalance));
            }
            
            return model;
        }

        private Dictionary<string, string> OrderByList()
        {
            var orderByList = new Dictionary<string, string>
            {
                { "Employee.First_Name ASC", "First Name | Ascending" },
                { "Employee.First_Name DESC", "First Name | Descending" },
                { "Employee.Last_Name ASC", "Last Name | Ascending" },
                { "Employee.Last_Name DESC", "Last Name | Descending" },
                { "Employee.Employee_ID ASC", "Employee ID | Ascending" },
                { "Employee.Employee_ID DESC", "Employee ID | Descending" }
            };
            return orderByList;
        }

        public Dictionary<int, string> AccountStatusList()
        {
            Dictionary<int, string> accStatus = new Dictionary<int, string>
            {
                { -1, "Active/Inactive" },
                { 1, "Active Only" },
                { 0, "Inactive Only" }
            };
            return accStatus;
        }

        public ActionResult Edit(int empID)
        {
            Dictionary dic = new Dictionary();

            var emp = GetEmployeeModel(empID);
            ViewBag.name = emp.firstName + " " + emp.lastName;
            ViewBag.gender = emp.gender;
            ViewBag.religion = dic.GetReligionName()[emp.religionID];

            var leaveBalance = GetLeaveBalanceModel(empID);

            return View(leaveBalance);
        }

        [HttpPost]
        public ActionResult Edit(Balance lb)
        {
            if (ModelState.IsValid)
            {
                UpdateBalance(lb.empId, lb.annualID, lb.annual, lb.editCommentAnnual);
                UpdateBalance(lb.empId, lb.maternityID, lb.maternity, lb.editCommentMaternity);
                UpdateBalance(lb.empId, lb.sickID, lb.sick, lb.editCommentSick);
                UpdateBalance(lb.empId, lb.compassionateID, lb.compassionate, lb.editCommentCompassionate);
                UpdateBalance(lb.empId, lb.daysInLieuID, lb.daysInLieu, lb.editCommentDIL);
                UpdateBalance(lb.empId, lb.shortHoursID, lb.shortHours, lb.editCommentShortHours);
                UpdateBalance(lb.empId, lb.pilgrimageID, lb.pilgrimage, lb.editCommentPilgrimage);
                UpdateBalance(lb.empId, lb.unpaidID, lb.unpaid, lb.editCommentUnpaid);
                ViewBag.SuccessMessage = "The information has been updated successfully.";
            }
            else
            {
                ViewBag.ErrorMessage = "An error occured, please check your input and try again.";
            }

            return Edit(lb.empId);
        }

        private void UpdateBalance(int empID, int typeID, decimal balance, string comment)
        {
            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            bool balanceExists = IsLeaveBalanceExist(empID, typeID);
            Tuple<int, decimal> oldBalance = (balanceExists) ? GetBalanceBefore(empID, typeID) : null;

            if (!balanceExists && balance > 0)
            {
                cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
                cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
                cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = balance;
                cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";
                cmd.CommandText = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_Type_ID, Balance, Last_Edit_Comment) VALUES(@empID, @typeID, @balance, @comment)";
            }
            else
            {
                cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
                cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
                cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = balance;
                cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";
                cmd.CommandText = "UPDATE dbo.Leave_Balance SET Balance = @balance, Last_Edit_Comment = @comment WHERE Leave_Type_ID = @typeID AND Employee_ID = @empID";
            }

            db.Execute(cmd);
            cmd.Parameters.Clear();

            if (oldBalance == null)
            {
                if (balance > 0)
                {
                    Tuple<int, decimal> createdBalance = GetBalanceBefore(empID, typeID);
                    cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = createdBalance.Item1;
                    cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = createdBalance.Item2;
                    cmd.Parameters.Add("@createdBy", SqlDbType.Int).Value = GetLoggedInID();
                    cmd.Parameters.Add("@createdOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";
                    cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_After, Created_By, Created_On, Comment) " +
                        "VALUES(@balanceID, 'Balance', @balance, @createdBy, @createdOn, @comment)";
                    db.Execute(cmd);
                }
            }
            else
            {
                if (oldBalance.Item2 != balance)
                {
                    Tuple<int, decimal> createdBalance = GetBalanceBefore(empID, typeID);
                    cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = oldBalance.Item1;
                    cmd.Parameters.Add("@prevBalance", SqlDbType.Decimal).Value = oldBalance.Item2;
                    cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = createdBalance.Item2;
                    cmd.Parameters.Add("@modifiedBy", SqlDbType.Int).Value = GetLoggedInID();
                    cmd.Parameters.Add("@modifiedOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
                    cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";
                    cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                        "VALUES(@balanceID, 'Balance', @prevBalance, @balance, @modifiedBy, @modifiedOn, @comment)";
                    db.Execute(cmd);
                }
            }
        }

        private Tuple<int, decimal> GetBalanceBefore(int empID, int typeID)
        {
            Tuple<int, decimal> leaveBalance = null;
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
            cmd.CommandText = "SELECT Leave_Balance_ID, Balance FROM dbo.Leave_Balance WHERE Employee_ID = @empID AND Leave_Type_ID = @typeID";

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                int balanceID = (int)row["Leave_Balance_ID"];
                decimal balance = (decimal)row["Balance"];
                leaveBalance = new Tuple<int, decimal>(balanceID, balance);
            }
            
            return leaveBalance;
        }
    }
}