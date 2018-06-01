using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using Hangfire;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrEmployeeLeaveHistoryController : BaseController
    {
        // GET: hrEmployeeLeaveHistory
        public ActionResult Index(int filterDepartmentID = -1, int filterLeaveType = -1, int filterLeaveStatus = -1, string filterSearch = "", string filterOrderBy = "", string filterStartDate = "", string filterEndDate = "")
        {
            var model = GetFilteredBalances(filterDepartmentID, filterLeaveType, filterLeaveStatus, filterSearch, filterOrderBy, filterStartDate, filterEndDate);
            Dictionary dic = new Dictionary();

            ViewData["EnteredSearch"] = filterSearch;
            ViewData["DepartmentList"] = dic.AddDefaultToDictionary(dic.GetDepartment(), -1, "All Departments");        // @TODO: change dic.GetDepartment() to dic.GetDepartmentName()
            ViewData["SelectedDepartment"] = filterDepartmentID;
            ViewData["LeaveStatusList"] = LeaveStatusList();
            ViewData["SelectedLeaveStatus"] = filterLeaveStatus;
            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["LeaveTypeList"] = dic.AddDefaultToDictionary(dic.GetLeaveTypeName(), -1, "All Types");
            ViewData["SelectedLeaveType"] = filterLeaveType;
            ViewData["SelectedStartDate"] = filterStartDate;
            ViewData["SelectedEndDate"] = filterEndDate;

            return View(model);
        }

        [HttpGet]
        public ActionResult View(int appID)
        {
            SetMessageViewBags();
            return View(GetLeaveModel("Leave_Application_ID", appID)[0]);
        }

        [HttpPost]
        public ActionResult Filter(FormCollection form)
        {
            int deptID = Convert.ToInt32(form["selectedDepartment"]);
            int leaveStatID = Convert.ToInt32(form["selectedLeaveStatus"]);
            int leaveTypeID = Convert.ToInt32(form["selectedLeaveType"]);
            string search = form["enteredSearch"];
            string orderBy = form["selectedOrderBy"];
            string startDate = form["selectedStartDate"];
            string endDate = form["selectedEndDate"];
            return RedirectToAction("Index", new { filterDepartmentID = deptID, filterLeaveType = leaveTypeID, filterLeaveStatus = leaveStatID, filterSearch = search, filterOrderBy = orderBy, filterStartDate = startDate, filterEndDate = endDate });
        }

        private List<Leave> GetFilteredBalances(int deptID, int leaveType, int leaveStat, string search, string order, string sDate, string eDate)
        {

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, " +
                "Leave.Reporting_Back_Date, Leave.Leave_Type_ID, Leave_Name, Leave_Type.Display_Name as Leave_Type_Display, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, " +
                "Leave_Status.Display_Name as Leave_Status_Display, HR_Comment, LM_Comment, Leave.Personal_Email, Is_Half_Start_Date, Is_Half_Reporting_Back_Date " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_Type_ID = Leave_Type.Leave_Type_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND " +
                "Employee.Employee_ID = Reporting.Employee_ID AND Leave_Status.Status_Name != 'Pending_LM' AND Leave_Status.Status_Name != 'Pending_HR'";

            // adds a filter query if a department is selected from the dropdown, note that -1 represents All Departments
            if (deptID >= 0)
            {
                cmd.Parameters.Add("@deptID", SqlDbType.Int).Value = deptID;
                cmd.CommandText += " AND Department.Department_ID = @deptID";
            }

            // adds a filter query if a leave type is selected from the dropdown, note that -1 represents All Types
            if (leaveType >= 0)
            {
                cmd.Parameters.Add("@leaveType", SqlDbType.Int).Value = leaveType;
                cmd.CommandText += " AND Leave.Leave_Type_ID = @leaveType";
            }

            // adds a filter query if a leave status is selected from the dropdown, note that -1 represents all status
            if (leaveStat >= 0)
            {
                cmd.Parameters.Add("@leaveStat", SqlDbType.Int).Value = leaveStat;
                cmd.CommandText += " AND Leave.Leave_Status_ID = @leaveStat";
            }

            // adds a filter query if search box contains character(s), note that 0 length means the search box is empty
            if (search.Length > 0)
            {
                cmd.Parameters.Add("@search", SqlDbType.NChar).Value = search;
                cmd.CommandText += " AND (Employee.Employee_ID LIKE '%' + @search + '%' " +
                    "OR CONCAT(First_Name, ' ', Last_Name) LIKE '%' + @search + '%')";
            }

            if (sDate.Length > 0)
            {
                cmd.Parameters.Add("@sDate", SqlDbType.NChar).Value = sDate;
                cmd.CommandText += " AND Leave.Start_Date >= @sDate";
            }

            if (eDate.Length > 0)
            {
                cmd.Parameters.Add("@eDate", SqlDbType.NChar).Value = eDate;
                cmd.CommandText += " AND Leave.Start_Date <= @eDate";
            }

            if (order.Length > 0)
            {
                cmd.CommandText += " ORDER BY " + order;
            }
            else
            {
                cmd.CommandText += " ORDER BY Leave_Application_ID ASC";
            }

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);
            
            return GetLeaveModel(cmd);
        }

        private Dictionary<string, string> OrderByList()
        {
            var orderByList = new Dictionary<string, string>
            {
                { "Leave_Application_ID ASC", "App. ID | Ascending" },
                { "Leave_Application_ID DESC", "App. ID | Descending" },
                { "First_Name ASC", "First Name | Ascending" },
                { "First_Name DESC", "First Name | Descending" },
                { "Last_Name ASC", "Last Name | Ascending" },
                { "Last_Name DESC", "Last Name | Descending" },
                { "Leave_Name ASC", "Leave Type | Ascending" },
                { "Leave_Name DESC", "Leave Type | Descending" },
                { "Status_Name ASC", "Leave Status | Ascending" },
                { "Status_Name DESC", "Leave Status | Descending" }
            };
            return orderByList;
        }

        private Dictionary<int, string> LeaveStatusList()
        {
            Dictionary<int, string> list = new Dictionary<int, string>();
            Dictionary dic = new Dictionary();
            Dictionary<int, string> temp = dic.GetLeaveStatusName();

            list.Add(-1, "All Statuses");
            foreach (var status in dic.GetLeaveStatus())
            {
                if (!status.Value.Equals("Pending_LM") && !status.Value.Equals("Pending_HR"))
                    list.Add(status.Key, temp[status.Key]);
            }

            return list;
        }

        // @TODO: attempt to change applicationID to appID
        public ActionResult Cancel(int applicationID)
        {
            Leave leave = GetLeaveModel("Leave_Application_ID", applicationID)[0];

            UpdateLeaveStatus(applicationID);
            RefundLeaveBalance(applicationID);
            AuditLeaveApplication(applicationID);

            string message = "Your " + leave.leaveTypeName + " leave application from " + leave.startDate.ToShortDateString() +
               " to " + leave.returnDate.ToShortDateString() + " with ID " + applicationID + " has been cancelled by human resources.";

            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

            TempData["WarningMessage"] = "Leave application <b>" + applicationID + "</b> has been cancelled successfully.";
            return RedirectToAction("View", new { appID = applicationID });
        }

        private void UpdateLeaveStatus(int appID)
        {
            Dictionary dic = new Dictionary();
            int cancelledID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Cancelled_HR").Key;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
            cmd.Parameters.Add("@cancelledID", SqlDbType.Int).Value = cancelledID;
            cmd.CommandText = "UPDATE dbo.Leave SET Leave_Status_ID = @cancelledID WHERE Leave_Application_ID = @appID";
            DataBase db = new DataBase();
            db.Execute(cmd);
        }

        private void RefundLeaveBalance(int appID)
        {
            Dictionary<int, decimal> audit = GetAuditLeaveBalance(appID);

            foreach (KeyValuePair<int, decimal> pair in audit)
            {
                int balanceID = pair.Key;
                decimal consumedBal = pair.Value;
                decimal prevBalance = GetLeaveBalance(balanceID);
                decimal newBalance = prevBalance + consumedBal;

                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
                cmd.Parameters.Add("@newBalance", SqlDbType.Decimal).Value = newBalance;
                cmd.CommandText = "UPDATE dbo.Leave_Balance SET Balance = @newBalance WHERE Leave_Balance_ID = @balanceID";
                DataBase db = new DataBase();
                db.Execute(cmd);

                cmd.Parameters.Clear();
                cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
                cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
                cmd.Parameters.Add("@valueBefore", SqlDbType.Decimal).Value = prevBalance;
                cmd.Parameters.Add("@valueAfter", SqlDbType.Decimal).Value = newBalance;
                cmd.Parameters.Add("@modifiedBy", SqlDbType.Int).Value = GetLoggedInID();
                cmd.Parameters.Add("@modifiedOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
                cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = "Refund All Balances";
                cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                    "VALUES(@balanceID, @appID, 'Balance', @valueBefore, @valueAfter, @modifiedBy, @modifiedOn, @comment)";
            }

        }

        private Dictionary<int, decimal> GetAuditLeaveBalance(int appID)
        {
            Dictionary<int, decimal> auditLeave = new Dictionary<int, decimal>();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
            cmd.CommandText = "SELECT Leave_Balance_ID, Value_Before, Value_After FROM dbo.Audit_Leave_Balance WHERE Leave_Application_ID = @appID ORDER BY Modified_On";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                int balID = (int)row["Leave_Balance_ID"];
                string valBefore = (string)row["Value_Before"];
                string valAfter = (string)row["Value_After"];

                if (auditLeave.ContainsKey(balID))
                    auditLeave[balID] += decimal.Parse(valBefore) - decimal.Parse(valAfter);
                else
                    auditLeave.Add(balID, decimal.Parse(valBefore) - decimal.Parse(valAfter));
            }

            return auditLeave;
        }

        private void AuditLeaveApplication(int appID)
        {
            Dictionary dic = new Dictionary();
            Dictionary<int, String> leaveStatus = dic.GetLeaveStatus();

            int approvedID = leaveStatus.FirstOrDefault(obj => obj.Value == "Approved").Key;
            int cancelledID = leaveStatus.FirstOrDefault(obj => obj.Value == "Cancelled_HR").Key;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
            cmd.Parameters.Add("@approvedID", SqlDbType.Int).Value = approvedID;
            cmd.Parameters.Add("@cancelledID", SqlDbType.Int).Value = cancelledID;
            cmd.Parameters.Add("@modifiedBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@modifiedOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Application (Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On) " +
                 "VALUES(@appID, 'Leave_Status_ID', @approvedID, @cancelledID, @modifiedBy, @modifiedOn)";
            DataBase db = new DataBase();
            db.Execute(cmd);
        }

        private decimal GetLeaveBalance(int balID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@balID", SqlDbType.Int).Value = balID;
            cmd.CommandText = "SELECT Balance FROM dbo.Leave_Balance WHERE Leave_Balance_ID = @balID";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            decimal balance = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                balance = (decimal)row["Balance"];
            }

            return balance;
        }
    }
}