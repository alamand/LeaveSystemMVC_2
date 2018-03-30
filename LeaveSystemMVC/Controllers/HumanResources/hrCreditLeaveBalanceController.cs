using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;


namespace LeaveSystemMVC.Controllers
{
    public class hrCreditLeaveBalanceController : ControllerBase
    {
        // GET: hrCreditLeaveBalance
        public ActionResult Index()
        {
            var ActiveEmployees = DBStaffList(1);
            List<sleaveBalanceModel> empBalance = new List<sleaveBalanceModel>();
            foreach (int empID in ActiveEmployees.Keys)
            {
                empBalance.Add(GetLeaveBalanceModel(empID));
            }

            ViewBag.employeeList = ActiveEmployees;
            ViewBag.defaultBalance = GetLeaveBalanceModel();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(empBalance);
        }

        [HttpPost]
        public ActionResult Index(List<sleaveBalanceModel> model)
        {
            sleaveBalanceModel defaultBal = GetLeaveBalanceModel();

            foreach (var empBal in model)
            {
                UpdateLeaveBalance(empBal.empId, empBal.daysInLieuID, empBal.daysInLieu);
                UpdateLeaveBalance(empBal.empId, empBal.annualID, empBal.annual);
                UpdateLeaveBalance(empBal.empId, defaultBal.sickID, defaultBal.sick);

                if (GetEmployeeModel(empBal.empId).gender == 'F')
                    UpdateLeaveBalance(empBal.empId, defaultBal.maternityID, defaultBal.maternity);
            }

            TempData["SuccessMessage"] = "Leave balances have been updated.";

            return RedirectToAction("Index");
        }

        private void UpdateLeaveBalance(int empID, int leaveID, decimal balance)
        {
            string queryString = "";
            int balanceID = GetEmpBalanceID(empID, leaveID);
            decimal prevBalance = DBGetLeaveBalance(balanceID);
            string comment = "Leave quota per annum";

            if (IsLeaveBalanceExists(empID, leaveID))
                queryString = "UPDATE dbo.Leave_Balance SET Balance = " + balance + ", Last_Edit_Comment = 'Leave quota per annum' WHERE Employee_ID = " + empID + " AND Leave_Type_ID = " + leaveID;
            else
                queryString = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_Type_ID, Balance, Last_Edit_Comment) VALUES(" + empID + "," + leaveID + "," + balance + ",'Leave quota per annum')";
            DBExecuteQuery(queryString);

            queryString = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                                     "VALUES('" + balanceID + "', 'Balance' ,'" + prevBalance + "','" + balance + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "','" + comment + "')";
            DBExecuteQuery(queryString);
        }

        private int GetEmpBalanceID(int empID, int leaveTypeID)
        {
            string queryString = "SELECT Leave_Balance_ID FROM dbo.Leave_Balance WHERE Employee_ID = '" + empID + "' AND Leave_Type_ID = '" + leaveTypeID + "'";
            int leaveBalID = 0;

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        leaveBalID = (int)reader["Leave_Balance_ID"];
                    }
                }
                connection.Close();
            }

            return leaveBalID;
        }
    }
}