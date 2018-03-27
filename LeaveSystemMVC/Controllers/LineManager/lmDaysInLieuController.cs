using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class lmDaysInLieuController : ControllerBase
    {
        // GET: lmDaysInLieu
        public ActionResult Index(int selectedEmployee = 0)
        {
            Dictionary<int, string> lmSubordinates = new Dictionary<int, string>();
            lmSubordinates.Add(0, "- Select Employee -");
            var allEmployees = GetEmployeeModel();
            int loggedInID = GetLoggedInID();

            foreach (var emp in allEmployees)
            {
                if (emp.reportsToLineManagerID == loggedInID)
                    lmSubordinates.Add((int)emp.staffID, emp.firstName + " " + emp.lastName);
            }

            foreach (var reporting in GetReportingList(loggedInID))
            {
                if (reporting.toID == loggedInID)
                    lmSubordinates.Add(reporting.employeeID, reporting.employeeName);
            }

            ViewData["EmployeeList"] = lmSubordinates;
            SetMessageViewBags();

            if (selectedEmployee != 0)
            {
                hrDaysInLieu daysInLieu = new hrDaysInLieu();
                daysInLieu.employeeID = selectedEmployee;
                ViewData["selectedEmployee"] = selectedEmployee;

                return View(daysInLieu);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(hrDaysInLieu dil)
        {
            string queryString = "INSERT INTO dbo.Days_In_Lieu VALUES ('" + dil.employeeID + "' , '" + dil.date.ToString("yyyy-MM-dd") + "' , '" + dil.numOfDays + "' , '" + dil.comment + "')";
            DBExecuteQuery(queryString);

            int dilID = DBLeaveTypeList().FirstOrDefault(obj => obj.Value == "DIL").Key;

            //Check if DIL leave type exists for this employee.
            Boolean isExists = IsLeaveBalanceExists(dil.employeeID, dilID);

            // if it exists, then get the leave balance ID, and update the audit trail (modified_by) and leave balance
            // else, insert a new record to the leave balance, and insert a new (created_by) in the audit trail
            if (isExists)
            {
                Tuple<int, decimal> balTuple = DBGetAuditDILLeaveBalance(dil.employeeID, dilID);
                DBUpdateDILAudit(balTuple.Item1, balTuple.Item2, (balTuple.Item2 + dil.numOfDays), dil.comment);
                queryString = "UPDATE dbo.Leave_Balance SET Balance = Balance + '" + dil.numOfDays + "' WHERE Employee_ID = '" + dil.employeeID + "' AND Leave_Type_ID = " + dilID;
                DBExecuteQuery(queryString);
            }
            else
            {
                queryString = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_Type_ID, Balance) VALUES ('" + dil.employeeID + "' , '" + dilID + "' , '" + dil.numOfDays + "')";
                DBExecuteQuery(queryString);
                Tuple<int, decimal> balTuple = DBGetAuditDILLeaveBalance(dil.employeeID, dilID);
                DBInsertDILAudit(balTuple.Item1, dil.numOfDays, dil.comment);
            }

            sEmployeeModel emp = GetEmployeeModel(dil.employeeID);
            TempData["SuccessMessage"] = "<b>" + emp.firstName + " " + emp.lastName + "</b> has been credited successfully.";

            return RedirectToAction("Index");           
        }

        [HttpPost]
        public ActionResult Select(FormCollection form)
        {
            int id = Convert.ToInt32(form["selectedEmployee"]);
            return RedirectToAction("Index", new { selectedEmployee = id });
        }

        private void DBInsertDILAudit(int leaveBalanceID, decimal valueAfter, string comment)
        {
            string queryString = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_After, Created_By, Created_On, Comment) " +
                  "VALUES('" + leaveBalanceID + "', 'Balance' ,'" + valueAfter + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "','" + comment + "')";
            DBExecuteQuery(queryString);
        }

        private void DBUpdateDILAudit(int leaveBalanceID, decimal valueBefore, decimal valueAfter, string comment)
        {
            string queryString = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                  "VALUES('" + leaveBalanceID + "', 'Balance' ,'" + valueBefore + "','" + valueAfter + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "','" + comment + "')";
            DBExecuteQuery(queryString);
        }

        private Tuple<int, decimal> DBGetAuditDILLeaveBalance(int empID, int leaveType)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT Leave_Balance_ID, Balance FROM dbo.Leave_Balance WHERE Employee_ID = @0 AND Leave_Type_ID = @1";
            Tuple<int, decimal> balTuple = null;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@0", empID);
                command.Parameters.AddWithValue("@1", leaveType);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int balID = (int)reader["Leave_Balance_ID"];
                        decimal balance = (decimal)reader["Balance"];
                        balTuple = new Tuple<int, decimal>(balID, balance);
                    }
                }
                connection.Close();
            }

            return balTuple;
        }
    }
}