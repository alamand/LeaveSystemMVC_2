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
    public class lmSubordinateLeaveHistoryController : BaseController
    {
        // GET: lmSubordinateLeaveHistory
        public ActionResult Index(int filterLeaveType = -1, int filterLeaveStatus = -1, string filterSearch = "", string filterOrderBy = "", string filterStartDate = "", string filterEndDate = "")
        {
            var model = GetFilteredLeaveApplications(filterLeaveType, filterLeaveStatus, filterSearch, filterOrderBy, filterStartDate, filterEndDate);
            Dictionary dic = new Dictionary();

            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["EnteredSearch"] = filterSearch;
            ViewData["SelectedStartDate"] = filterStartDate;
            ViewData["SelectedEndDate"] = filterEndDate;
            ViewData["LeaveStatusList"] = LeaveStatusList();
            ViewData["SelectedLeaveStatus"] = filterLeaveStatus;
            ViewData["LeaveTypeList"] = dic.AddDefaultToDictionary(dic.GetLeaveTypeName(), -1, "All Types");
            ViewData["SelectedLeaveType"] = filterLeaveType;

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
            int leaveStatID = Convert.ToInt32(form["selectedLeaveStatus"]);
            int leaveTypeID = Convert.ToInt32(form["selectedLeaveType"]);
            string search = form["enteredSearch"];
            string orderBy = form["selectedOrderBy"];
            string startDate = form["selectedStartDate"];
            string endDate = form["selectedEndDate"];
            return RedirectToAction("Index", new { filterLeaveType = leaveTypeID, filterLeaveStatus = leaveStatID, filterSearch = search, filterOrderBy = orderBy, filterStartDate = startDate, filterEndDate = endDate });
        }

        private List<Leave> GetFilteredLeaveApplications(int leaveType, int leaveStat, string search, string order, string sDate, string eDate)
        {
            int loggedInID = GetLoggedInID();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@loggedInID", SqlDbType.Int).Value = loggedInID;
            cmd.CommandText = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, Leave.Reporting_Back_Date, Leave.Leave_Type_ID, Leave_Name, Leave_Type.Display_Name as Leave_Type_Display, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, Leave_Status.Display_Name as Leave_Status_Display, HR_Comment, LM_Comment, Leave.Personal_Email, Leave.Is_Half_Start_Date, Leave.Is_Half_Reporting_Back_Date " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_Type_ID = Leave_Type.Leave_Type_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND Employee.Employee_ID = Reporting.Employee_ID " +
                "AND Leave_Status.Status_Name != 'Pending_LM' AND Leave_Status.Status_Name != 'Pending_HR' AND (Reporting.Report_To_ID = @loggedInID";

            List<Reporting> reportingList = GetReportingList(loggedInID);
            string reportToList = "";
            foreach (var reporting in reportingList)
            {
                if (reporting.toID == loggedInID)
                    reportToList += " OR Report_To_ID = " + reporting.reportToID;
            }
            cmd.Parameters.Add("@reportToList", SqlDbType.NChar).Value = reportToList;
            cmd.CommandText += reportToList + ")";

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
                cmd.CommandText += " AND CONCAT(First_Name, ' ', Last_Name) LIKE '%' + @search + '%'";
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

        public ActionResult Cancel(int applicationID)
        {
            Dictionary dic = new Dictionary();

            Leave leave = GetLeaveModel("Leave_Application_ID", applicationID)[0];
            int cancelledID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Cancelled_LM").Key;
            int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = applicationID;
            cmd.Parameters.Add("@cancelledID", SqlDbType.Int).Value = cancelledID;
            cmd.Parameters.Add("@approvedID", SqlDbType.Int).Value = approvedID;
            cmd.Parameters.Add("@modifiedBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@modifiedOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");

            cmd.CommandText = "UPDATE dbo.Leave SET Leave_Status_ID = @cancelledID WHERE Leave_Application_ID = @appID";
            db.Execute(cmd);

            //DBRefundLeaveBalance(applicationID);      @TODO

            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Application (Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On) " +
                  "VALUES(@appID, 'Leave_Status_ID', @approvedID, @cancelledID, @modifiedBy, @modifiedOn)";
            db.Execute(cmd);

            string message = "Your " + leave.leaveTypeName + " leave application from " + leave.startDate.ToShortDateString() + 
                " to " + leave.returnDate.ToShortDateString() + " with ID " + applicationID + " has been cancelled by your line manager.";

            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

            TempData["WarningMessage"] = "Leave application <b>" + applicationID + "</b> has been cancelled successfully.";
            return RedirectToAction("View", new { appID = applicationID });
        }
    }
}