using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Configuration;
using System.Data.SqlClient;
using Hangfire;

namespace LeaveSystemMVC.Controllers
{
    public class lmSubordinateLeaveHistoryController : ControllerBase
    {
        // GET: lmSubordinateLeaveHistory
        public ActionResult Index(int filterLeaveType = -1, int filterLeaveStatus = -1, string filterSearch = "", string filterOrderBy = "", string filterStartDate = "", string filterEndDate = "")
        {
            string queryString = GetFilteredQuery(filterLeaveType, filterLeaveStatus, filterSearch, filterOrderBy, filterStartDate, filterEndDate);
            var model = GetLeaveModel(queryString);

            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["EnteredSearch"] = filterSearch;
            ViewData["SelectedStartDate"] = filterStartDate;
            ViewData["SelectedEndDate"] = filterEndDate;
            ViewData["LeaveStatusList"] = LeaveStatusList();
            ViewData["SelectedLeaveStatus"] = filterLeaveStatus;
            ViewData["LeaveTypeList"] = AddDefaultToDictionary(DBLeaveTypeList(), -1, "All Types");
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

        private string GetFilteredQuery(int leaveType, int leaveStat, string search, string order, string sDate, string eDate)
        {
            int loggedInID = GetLoggedInID();
            string queryString = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, Leave.Reporting_Back_Date, Leave.Leave_Type_ID, Leave_Name, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, HR_Comment, LM_Comment, Leave.Personal_Email " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_Type_ID = Leave_Type.Leave_Type_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND Employee.Employee_ID = Reporting.Employee_ID " +
                "AND Leave_Status.Status_Name != 'Pending_LM' AND Leave_Status.Status_Name != 'Pending_HR' AND (Reporting.Report_To_ID = " + loggedInID;

            List<lmReporting> reportingList = GetReportingList(loggedInID);
            foreach (var reporting in reportingList)
            {
                if (reporting.toID == loggedInID)
                {
                    queryString += " OR Report_To_ID = " + reporting.reportToID;
                }
            }
            queryString += ")";

            // adds a filter query if a leave type is selected from the dropdown, note that -1 represents All Types
            if (leaveType >= 0)
            {
                queryString += " AND Leave.Leave_Type_ID = " + leaveType;
            }

            // adds a filter query if a leave status is selected from the dropdown, note that -1 represents all status
            if (leaveStat >= 0)
            {
                queryString += " AND Leave.Leave_Status_ID = " + leaveStat;
            }

            // adds a filter query if search box contains character(s), note that 0 length means the search box is empty
            if (search.Length > 0)
            {
                queryString += " AND (Employee.Employee_ID LIKE '%" + search + "%' " +
                    "OR CONCAT(First_Name, ' ', Last_Name) LIKE '%" + search + "%')";
            }

            if (sDate.Length > 0)
            {
                queryString += " AND Leave.Start_Date >= '" + sDate + "'";
            }

            if (eDate.Length > 0)
            {
                queryString += " AND Leave.Start_Date <= '" + eDate + "'";
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

        private Dictionary<int, string> LeaveStatusList()
        {
            Dictionary<int, string> leaveStatusList = new Dictionary<int, string>();

            leaveStatusList.Add(-1, "All Statuses");
            foreach (var status in DBLeaveStatusList())
            {
                if (!status.Value.Equals("Pending_LM") && !status.Value.Equals("Pending_HR"))
                    leaveStatusList.Add(status.Key, status.Value);
            }

            return leaveStatusList;
        }

        public ActionResult Cancel(int applicationID)
        {
            sLeaveModel leaveModel = GetLeaveModel("Leave_Application_ID", applicationID)[0];
            int cancelledID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Cancelled_LM").Key;
            string queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '" + cancelledID + "' WHERE Leave_Application_ID = '" + applicationID + "'";
            DBExecuteQuery(queryString);

            DBRefundLeaveBalance(applicationID);

            int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
            string quditString = "INSERT INTO dbo.Audit_Leave_Application (Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On) " +
                  "VALUES('" + applicationID + "', 'Leave_Status_ID', '" + approvedID + "','" + cancelledID + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
            DBExecuteQuery(quditString);

            string message = "";
            message = "Your " + leaveModel.leaveTypeName + " leave application from " + leaveModel.startDate.ToShortDateString() + " to " + leaveModel.returnDate.ToShortDateString() + " with ID " + applicationID + " has been cancelled by your line manager.";
            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leaveModel.employeeID).email, message));

            TempData["WarningMessage"] = "Leave application <b>" + applicationID + "</b> has been cancelled successfully.";
            return RedirectToAction("View", new { appID = applicationID });
        }
    }
}