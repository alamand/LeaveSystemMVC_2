using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrEditBalanceController : ControllerBase
    {
        // GET: hrEditBalance
        public ActionResult Index(int filterDepartmentID = 0, int filterAccStatus = -1, string filterSearch = "", string filterOrderBy = "")
        {
            var model = new List<Tuple<sEmployeeModel, sleaveBalanceModel>>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = GetFilteredQuery(filterDepartmentID, filterAccStatus, filterSearch, filterOrderBy);

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);  
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // iterate through all employees in the database and add them all to the list
                    while (reader.Read())
                    {
                        var employee = GetEmployeeModel((int)reader["Employee_ID"]);
                        var leaveBalance = GetLeaveBalanceModel((int)reader["Employee_ID"]);
                        model.Add(new Tuple<sEmployeeModel, sleaveBalanceModel>(employee, leaveBalance));
                    }
                }
                connection.Close();
            }

            ViewData["DepartmentList"] = AddDefaultToDictionary(DBDepartmentList(), 0, "All Departments");
            ViewData["SelectedDepartment"] = filterDepartmentID;
            ViewData["AccountStatusList"] = AccountStatusList();
            ViewData["SelectedAccStatus"] = filterAccStatus;
            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["EnteredSearch"] = filterSearch;
            ViewData["ReligionList"] = DBReligionList();

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

        private string GetFilteredQuery(int deptID, int accStat, string search, string order)
        {
            int staffRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "Staff").Key;
            string queryString = "SELECT Employee.Employee_ID FROM dbo.Employee, dbo.Employee_Role " +
                "WHERE Employee.Employee_ID = Employee_Role.Employee_ID AND Employee_Role.Role_ID = " + staffRoleID;

            // adds a filter query if a department is selected from the dropdown, note that 0 represents All Departments
            if (deptID > 0)
            {
                queryString += " AND Department_ID = " + deptID;
            }

            // adds a filter query if a account status is selected from the dropdown, note that -1 represents Active/InActive
            if (accStat >= 0)
            {
                queryString += " AND Account_Status = " + accStat;
            }

            // adds a filter query if search box contains character(s), note that 0 length means the search box is empty
            if (search.Length > 0)
            {
                queryString += " AND (Employee.Employee_ID LIKE '%" + search + "%' " +
                    "OR First_Name LIKE '%" + search + "%' " +
                    "OR Last_Name LIKE '%" + search + "%')";
            }

            if (order.Length > 0)
            {
                queryString += " ORDER BY " + order;
            }

            return queryString;
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


        public ActionResult Edit(int empID)
        {
            var emp = GetEmployeeModel(empID);
            ViewBag.name = emp.firstName + " " + emp.lastName;
            ViewBag.gender = emp.gender;
            ViewBag.religion = DBReligionList()[emp.religionID];

            var leaveBalance = GetLeaveBalanceModel(empID);

            return View(leaveBalance);
        }

        [HttpPost]
        public ActionResult Edit(sleaveBalanceModel lb)
        {
            if (ModelState.IsValid)
            {
                DBUpdateBalance(lb.empId, lb.annualID, lb.annual);
                DBUpdateComment(lb.empId, lb.annualID, lb.editCommentAnnual);
                DBUpdateBalance(lb.empId, lb.maternityID, lb.maternity);
                DBUpdateComment(lb.empId, lb.maternityID, lb.editCommentMaternity);
                DBUpdateBalance(lb.empId, lb.sickID, lb.sick);
                DBUpdateComment(lb.empId, lb.sickID, lb.editCommentSick);
                DBUpdateBalance(lb.empId, lb.compassionateID, lb.compassionate);
                DBUpdateComment(lb.empId, lb.compassionateID, lb.editCommentCompassionate);
                DBUpdateBalance(lb.empId, lb.daysInLieuID, lb.daysInLieu);
                DBUpdateComment(lb.empId, lb.daysInLieuID, lb.editCommentDIL);
                DBUpdateBalance(lb.empId, lb.shortHoursID, lb.shortHours);
                DBUpdateComment(lb.empId, lb.shortHoursID, lb.editCommentShortHours);
                DBUpdateBalance(lb.empId, lb.pilgrimageID, lb.pilgrimage);
                DBUpdateComment(lb.empId, lb.pilgrimageID, lb.editCommentPilgrimage);
                DBUpdateBalance(lb.empId, lb.unpaidID, lb.unpaid);
                DBUpdateComment(lb.empId, lb.unpaidID, lb.editCommentUnpaid);
                ViewBag.SuccessMessage = " The information has been updated successfully.";
            }
            else
            {
                ViewBag.ErrorMessage = " An error occured, please check your input and try again.";
            }

            return Edit(lb.empId);
        }

        private void DBUpdateBalance(int employeeID, int leaveID, decimal balance)
        {
            string insertQuery = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) VALUES('" + employeeID + "','" + leaveID + "','" + balance + "')";
            string updateQuery = "UPDATE dbo.Leave_Balance SET Balance = '" + balance + "' WHERE Leave_ID = '" + leaveID + "' AND Employee_ID = '" + employeeID + "'";
            string queryString = (!IsLeaveBalanceExists(employeeID, leaveID) && balance > 0) ? insertQuery : updateQuery;
            DBExecuteQuery(queryString);
        }

        private void DBUpdateComment(int employeeID, int leaveID, String comment)
        {
            if(comment != null)
            {
                string updateQuery = "UPDATE dbo.Leave_Balance SET Last_Edit_Comment = '" + comment + "' WHERE Leave_ID = '" + leaveID + "' AND Employee_ID = '" + employeeID + "'";
                DBExecuteQuery(updateQuery);
            }            
        }
    }
}