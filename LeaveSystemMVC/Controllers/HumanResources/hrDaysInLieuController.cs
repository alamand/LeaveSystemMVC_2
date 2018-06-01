using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrDaysInLieuController : BaseController
    {
        // GET: hrDaysInLieu
        public ActionResult Index(int selectedEmployee = 0)
        {
            Dictionary dic = new Dictionary();

            ViewData["EmployeeList"] = dic.AddDefaultToDictionary(dic.GetStaff(1), 0, "- Select Employee -");
            SetMessageViewBags();

            if (selectedEmployee != 0)
            {
                DaysInLieu daysInLieu = new DaysInLieu();
                daysInLieu.employeeID = selectedEmployee;
                ViewData["selectedEmployee"] = selectedEmployee;

                return View(daysInLieu);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(DaysInLieu dil)
        {
            if (dil.date.Year != DateTime.Now.Year)
            {
                TempData["ErrorMessage"] = "You must select a date from the present year.";
                return Index(dil.employeeID);
            }
            else
            {
                bool success = true;
                Dictionary dic = new Dictionary();

                success = (success) ? InsertDIL(dil.employeeID, dil.date, dil.numOfDays, dil.comment) : success;
                
                int dilID = dic.GetLeaveType().FirstOrDefault(obj => obj.Value == "DIL").Key;

                //Check if DIL leave type exists for this employee.
                bool isExists = IsLeaveBalanceExist(dil.employeeID, dilID);

                // if it exists, then get the leave balance ID, and update the audit trail (modified_by) and leave balance
                // else, insert a new record to the leave balance, and insert a new (created_by) in the audit trail
                if (isExists)
                {
                    Tuple<int, decimal> balTuple = GetAuditDILLeaveBalance(dil.employeeID, dilID);
                    success = (success) ? UpdateDILLeaveBalance(dil.employeeID, dilID, dil.numOfDays) : success;
                    success = (success) ? InsertModifiedDILAudit(balTuple.Item1, balTuple.Item2, (balTuple.Item2 + dil.numOfDays), dil.comment) : success;
                }
                else
                {
                    success = (success) ? InsertDILLeaveBalance(dil.employeeID, dilID, dil.numOfDays) : success;
                    Tuple<int, decimal> balTuple = GetAuditDILLeaveBalance(dil.employeeID, dilID);
                    success = (success) ? InsertCreatedDILAudit(balTuple.Item1, dil.numOfDays, dil.comment) : success;
                }

                Employee emp = GetEmployeeModel(dil.employeeID);
                if (success)
                    TempData["SuccessMessage"] = "<b>" + emp.firstName + " " + emp.lastName + "</b> has been credited successfully.";
                else
                    TempData["ErrorMessage"] = "Something went wrong, please double check <b>" + emp.firstName + " " + emp.lastName + "</b>'s Days In Lieu and try again.";

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Select(FormCollection form)
        {
            int id = Convert.ToInt32(form["selectedEmployee"]);
            return RedirectToAction("Index", new { selectedEmployee = id });
        }

        private bool InsertDIL(int empID, DateTime date, Decimal numOfDays, string comment)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@date", SqlDbType.NChar).Value = date.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@numOfDays", SqlDbType.Decimal).Value = numOfDays;
            cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";
            cmd.CommandText = "INSERT INTO dbo.Days_In_Lieu VALUES (@empID, @date , @numOfDays , @comment)";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool InsertDILLeaveBalance(int empID, int balanceID, decimal numOfDays)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
            cmd.Parameters.Add("@numOfDays", SqlDbType.Decimal).Value = numOfDays;
            cmd.CommandText = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_Type_ID, Balance) VALUES (@empID, @balanceID, @numOfDays)";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool UpdateDILLeaveBalance(int empID, int balanceID, decimal numOfDays)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
            cmd.Parameters.Add("@numOfDays", SqlDbType.Decimal).Value = numOfDays;
            cmd.CommandText = "UPDATE dbo.Leave_Balance SET Balance = Balance + @numOfDays WHERE Employee_ID = @empID AND Leave_Type_ID = @balanceID";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool InsertCreatedDILAudit(int balanceID, decimal valueAfter, string comment)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = valueAfter;
            cmd.Parameters.Add("@createdBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@createdOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";
            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_After, Created_By, Created_On, Comment) " +
                "VALUES(@balanceID, 'Balance', @balance, @createdBy, @createdOn, @comment)";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool InsertModifiedDILAudit(int balanceID, decimal valueBefore, decimal valueAfter, string comment)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
            cmd.Parameters.Add("@valueBefore", SqlDbType.Decimal).Value = valueBefore;
            cmd.Parameters.Add("@valueAfter", SqlDbType.Decimal).Value = valueAfter;
            cmd.Parameters.Add("@createdBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@createdOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";
            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                "VALUES(@balanceID, 'Balance', @valueBefore, @valueAfter, @createdBy, @createdOn, @comment)";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private Tuple<int, decimal> GetAuditDILLeaveBalance(int empID, int leaveType)
        {
            Tuple<int, decimal> balTuple = null;
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@leaveType", SqlDbType.Int).Value = leaveType;
            cmd.CommandText = "SELECT Leave_Balance_ID, Balance FROM dbo.Leave_Balance WHERE Employee_ID = @empID AND Leave_Type_ID = @leaveType";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                int balID = (int)row["Leave_Balance_ID"];
                decimal balance = (decimal)row["Balance"];
                balTuple = new Tuple<int, decimal>(balID, balance);
            }
            
            return balTuple;
        }
    }
}