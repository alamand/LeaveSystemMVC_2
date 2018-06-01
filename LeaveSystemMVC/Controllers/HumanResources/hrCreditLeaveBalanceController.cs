using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrCreditLeaveBalanceController : BaseController
    {
        // GET: hrCreditLeaveBalance
        public ActionResult Index()
        {
            List<Employee> allEmployees = GetEmployeeModel();
            Dictionary<int, string> employees = new Dictionary<int, string>();
            List<Balance> empBalance = new List<Balance>();

            foreach (var emp in allEmployees)
            {
                if (emp.accountStatus == true && emp.onProbation == false && emp.isStaff)
                {
                    empBalance.Add(GetLeaveBalanceModel((int)emp.staffID));
                    employees.Add((int)emp.staffID, emp.firstName + " " + emp.lastName);
                }
            }

            ViewBag.employeeList = employees;
            ViewBag.defaultBalance = GetLeaveBalanceModel();
            SetMessageViewBags();

            return View(empBalance);
        }

        [HttpPost]
        public ActionResult Index(List<Balance> model)
        {
            Balance defaultBal = GetLeaveBalanceModel();
            List<Employee> failedToUpdateEmployees = new List<Employee>();

            foreach (var empBal in model)
            {
                bool success = true;
                success = (success) ? UpdateLeaveQuotaPerAnnum(empBal.empId, empBal.daysInLieuID, empBal.daysInLieu) : success;
                success = (success) ? UpdateLeaveQuotaPerAnnum(empBal.empId, empBal.annualID, empBal.annual) : success;
                success = (success) ? UpdateLeaveQuotaPerAnnum(empBal.empId, defaultBal.sickID, defaultBal.sick) : success;

                if (GetEmployeeModel(empBal.empId).gender == 'F')
                    success = (success) ? UpdateLeaveQuotaPerAnnum(empBal.empId, defaultBal.maternityID, defaultBal.maternity) : success;

                if (!success)
                    failedToUpdateEmployees.Add(GetEmployeeModel(empBal.empId));
            }

            if (failedToUpdateEmployees.Count > 0)
            {
                TempData["WarningMessage"] = "Something went wrong, please double check balances on the following employee(s): <br/>";
                foreach (var emp in failedToUpdateEmployees)
                {
                    TempData["WarningMessage"] += "-" + emp.firstName + " " + emp.lastName + " [" + emp.staffID + "]<br/>";
                }
            }
            else
            {
                TempData["SuccessMessage"] = "Leave balances have been updated.";
            }

            return RedirectToAction("Index");
        }

        private bool UpdateLeaveQuotaPerAnnum(int empID, int typeID, decimal balance)
        {
            bool success = false;
            int balanceID;

            if (IsLeaveBalanceExist(empID, typeID))
            {
                balanceID = GetLeaveBalanceID(empID, typeID);
                decimal prevBalance = GetLeaveBalance(balanceID);
                
                if (UpdateLeaveBalance(empID, typeID, balance))
                    success = InsertModifiedLeaveBalanceAudit(balanceID, prevBalance, balance);
            }
            else
            {
                if (InsertLeaveBalance(empID, typeID, balance))
                {
                    balanceID = GetLeaveBalanceID(empID, typeID);
                    success = InsertCreatedLeaveBalanceAudit(balanceID, balance);
                }
            }

            return success;
        }

        private bool UpdateLeaveBalance(int? empID, int typeID, decimal balance)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;
            cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = balance;
            cmd.CommandText = "UPDATE dbo.Leave_Balance SET Balance = @balance, " +
                "Last_Edit_Comment = 'Leave quota per annum' WHERE Employee_ID = @empID AND Leave_Type_ID = @typeID";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool InsertLeaveBalance(int? empID, int typeID, decimal balance)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;
            cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = balance;
            cmd.CommandText = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_Type_ID, Balance, Last_Edit_Comment) " +
                "VALUES(@empID, @typeID, @balance, 'Leave quota per annum')";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool InsertModifiedLeaveBalanceAudit(int balanceID, decimal prevBalance, decimal balance)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
            cmd.Parameters.Add("@prevBalance", SqlDbType.Decimal).Value = prevBalance;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = balance;
            cmd.Parameters.Add("@modifiedBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@modifiedOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                "VALUES(@balanceID, 'Balance', @prevBalance, @balance, @modifiedBy, @modifiedOn, 'Leave quota per annum')";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool InsertCreatedLeaveBalanceAudit(int balanceID, decimal balance)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = balance;
            cmd.Parameters.Add("@createdBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@createdOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_After, Created_by, Created_On, Comment) " +
                "VALUES(@balanceID, 'Balance', @balance, @createdBy, @createdOn, 'Leave quota per annum')";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private decimal GetLeaveBalance(int balanceID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
            cmd.CommandText = "SELECT Balance FROM dbo.Leave_Balance WHERE Leave_Balance_ID = @balanceID";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            decimal balance = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                balance = (decimal)row["Balance"];
            }

            return balance;
        }

        private int GetLeaveBalanceID(int empID, int typeID)
        {
            int leaveBalanceID = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
            cmd.CommandText = "SELECT Leave_Balance_ID FROM dbo.Leave_Balance WHERE Employee_ID = @empID AND Leave_Type_ID = @typeID";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                leaveBalanceID = (int)row["Leave_Balance_ID"];
            }

            return leaveBalanceID;
        }
    }
}