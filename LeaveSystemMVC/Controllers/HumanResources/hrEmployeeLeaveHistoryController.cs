using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;
using Hangfire;

namespace LeaveSystemMVC.Controllers
{
    public class hrEmployeeLeaveHistoryController : ControllerBase
    {
        // GET: hrEmployeeLeaveHistory
        public ActionResult Index(int filterDepartmentID = -1, int filterLeaveType = -1, int filterLeaveStatus = -1, string filterSearch = "", string filterOrderBy = "", string filterStartDate = "", string filterEndDate = "")
        {
            string queryString = GetFilteredQuery(filterDepartmentID, filterLeaveType, filterLeaveStatus, filterSearch, filterOrderBy, filterStartDate, filterEndDate);
            var model = GetLeaveModel(queryString);

            ViewData["EnteredSearch"] = filterSearch;
            ViewData["DepartmentList"] = AddDefaultToDictionary(DBDepartmentList(), -1, "All Departments");
            ViewData["SelectedDepartment"] = filterDepartmentID;
            ViewData["LeaveStatusList"] = LeaveStatusList();
            ViewData["SelectedLeaveStatus"] = filterLeaveStatus;
            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["LeaveTypeList"] = AddDefaultToDictionary(DBLeaveTypeList(), -1, "All Types");
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

        private string GetFilteredQuery(int deptID, int leaveType, int leaveStat, string search, string order, string sDate, string eDate)
        {
            var queryString = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, Leave.Reporting_Back_Date, Leave.Leave_Type_ID, Leave_Name, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, Leave_Status.Display_Name, HR_Comment, LM_Comment, Leave.Personal_Email, Leave.Is_Half_Start_Date, Leave.Is_Half_Reporting_Back_Date " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_Type_ID = Leave_Type.Leave_Type_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND Employee.Employee_ID = Reporting.Employee_ID " +
                "AND Leave_Status.Status_Name != 'Pending_LM' AND Leave_Status.Status_Name != 'Pending_HR'";

            // adds a filter query if a department is selected from the dropdown, note that -1 represents All Departments
            if (deptID >= 0)
            {
                queryString += " AND Department.Department_ID = " + deptID;
            }

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
            int cancelledID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Cancelled_HR").Key;
            string queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '" + cancelledID + "' WHERE Leave_Application_ID = '" + applicationID + "'";
            DBExecuteQuery(queryString);

            DBRefundLeaveBalance(applicationID);

            int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
            string quditString = "INSERT INTO dbo.Audit_Leave_Application (Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On) " +
                  "VALUES('" + applicationID + "', 'Leave_Status_ID', '" + approvedID + "','" + cancelledID + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
            DBExecuteQuery(quditString);

            string message = "";
            message = "Your " + leaveModel.leaveTypeName + " leave application from " + leaveModel.startDate.ToShortDateString() + " to " + leaveModel.returnDate.ToShortDateString() + " with ID " + applicationID + " has been cancelled by human resources.";
            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leaveModel.employeeID).email, message));

            TempData["WarningMessage"] = "Leave application <b>" + applicationID + "</b> has been cancelled successfully.";
            return RedirectToAction("View", new { appID = applicationID });
        }


    }
}