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
    public class hrEmployeeLeaveHistoryController : ControllerBase
    {
        // GET: hrEmployeeLeaveHistory
        public ActionResult Index(int filterDepartmentID = 0, int filterAccStatus = -1, string filterSearch = "", string filterOrderBy = "", string filterStartDate = "", string filterEndDate = "")
        {
            // @TODO: ADD FILTERS BY DEPARTMENT, EMPLOYEE and DATE (START-END).
            var model = new List<sLeaveModel>();
            System.Diagnostics.Debug.WriteLine("search: " + filterStartDate);

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = GetFilteredQuery(filterDepartmentID, filterAccStatus, filterSearch, filterOrderBy, filterStartDate, filterEndDate);

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var leave = new sLeaveModel
                        {
                            leaveAppID = (int)reader["Leave_Application_ID"],
                            employeeID = (int)reader["Employee_ID"],
                            employeeName = (string)reader["First_Name"] + " " + (string)reader["Last_Name"],
                            startDate = (!DBNull.Value.Equals(reader["Start_Date"])) ? (DateTime)reader["Start_Date"] : new DateTime(0, 0, 0),
                            endDate = (!DBNull.Value.Equals(reader["End_Date"])) ? (DateTime)reader["End_Date"] : new DateTime(0, 0, 0),
                            leaveTypeID = (int)reader["Leave_ID"],
                            leaveTypeName = (string)reader["Leave_Name"],
                            contactDetails = (!DBNull.Value.Equals(reader["Contact_Outside_UAE"])) ? (string)reader["Contact_Outside_UAE"] : "",
                            comments = (!DBNull.Value.Equals(reader["Comment"])) ? (string)reader["Comment"] : "",
                            documentation = (!DBNull.Value.Equals(reader["Documentation"])) ? (string)reader["Documentation"] : "",
                            bookAirTicket = (!DBNull.Value.Equals(reader["Flight_Ticket"])) ? (bool)reader["Flight_Ticket"] : false,
                            leaveDuration = (decimal)reader["Total_Leave"],
                            shortStartTime = (!DBNull.Value.Equals(reader["Start_Hrs"])) ? (TimeSpan)reader["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                            shortEndTime = (!DBNull.Value.Equals(reader["End_Hrs"])) ? (TimeSpan)reader["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                            leaveStatusID = (int)reader["Leave_Status_ID"],
                            leaveStatusName = (string)reader["Status_Name"],
                            hrComment = (!DBNull.Value.Equals(reader["HR_Comment"])) ? (string)reader["HR_Comment"] : "",
                            lmComment = (!DBNull.Value.Equals(reader["LM_Comment"])) ? (string)reader["LM_Comment"] : ""
                        };
                        model.Add(leave);
                        System.Diagnostics.Debug.WriteLine("db: " + reader["Start_Date"]);
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
            ViewData["SelectedStartDate"] = filterStartDate;
            ViewData["SelectedEndDate"] = filterEndDate;
            return View(model);
        }

        [HttpPost]
        public ActionResult Filter(FormCollection form)
        {
            int deptID = Convert.ToInt32(form["selectedDepartment"]);
            int accStatID = Convert.ToInt32(form["SelectedAccStatus"]);
            string search = form["enteredSearch"];
            string orderBy = form["selectedOrderBy"];
            string startDate = form["selectedStartDate"];
            string endDate = form["selectedEndDate"];
            return RedirectToAction("Index", new { filterDepartmentID = deptID, filterAccStatus = accStatID, filterSearch = search, filterOrderBy = orderBy, filterStartDate = startDate, filterEndDate = endDate });
        }

        private string GetFilteredQuery(int deptID, int accStat, string search, string order, string sDate, string eDate)
        {
            var queryString = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, Leave.End_Date, Leave.Leave_ID, Leave_Name, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, HR_Comment, LM_Comment " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_ID = Leave_Type.Leave_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND Employee.Employee_ID = Reporting.Employee_ID " +
                "AND Leave_Status.Status_Name != 'Pending_LM' AND Leave_Status.Status_Name != 'Pending_HR'";

            // adds a filter query if a department is selected from the dropdown, note that 0 represents All Departments
            if (deptID > 0)
            {
                queryString += " AND Department.Department_ID = " + deptID;
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
                    "OR Leave_Application_ID LIKE '%" + search + "%' " +
                    "OR First_Name LIKE '%" + search + "%' " +
                    "OR Last_Name LIKE '%" + search + "%')";
            }

            if (sDate.Length > 0)
            {
                queryString += " AND CONVERT(VARCHAR(10), Leave.Start_Date, 0) >= '" + sDate + "'"; 
            }

            if (sDate.Length > 0)
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
    }
}