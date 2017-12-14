using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Security.Claims;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;

namespace LeaveSystemMVC.Controllers
{

    public class lmPendingApplicationsController : ControllerBase
    {
        [HttpGet]
        // GET: lmPendingApplications
        public ActionResult Index()
        {
            List<sLeaveModel> RetrievedApplications = new List<sLeaveModel>();

            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;

            //Check if this user has nominated a substitute.
            string queryString2 = "SELECT dbo.Department.Substitute_LM_ID FROM dbo.Department WHERE dbo.Department.Line_Manager_ID = '" + GetLoggedInID() + "'";
            Boolean substituteExists = false;
            int subsID = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["Substitute_LM_ID"] != DBNull.Value)
                            {
                                substituteExists = true;
                                subsID = (int)reader["Substitute_LM_ID"];
                            }
                        }
                    }
                }
            }

            //No substitute nominated, so add the leave applications of staff members who report to this user
            if (!substituteExists)
            {
                var leaveList = GetLeaveModel("Reporting.Reporting_ID", GetLoggedInID());

                foreach (var leave in leaveList)
                {
                    if (leave.leaveStatusName.Equals("Pending_LM"))
                        RetrievedApplications.Add(leave);
                }
            }

            //Check if this user is a substitute.
            queryString2 = "SELECT dbo.Department.Line_Manager_ID FROM dbo.Department WHERE dbo.Department.Substitute_LM_ID = '" + GetLoggedInID() + "'";
            Boolean isSubstitute = false;
            int LM_ID = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["Line_Manager_ID"] != DBNull.Value)
                            {
                                isSubstitute = true;
                                LM_ID = (int)reader["Line_Manager_ID"];
                            }
                        }
                    }
                }
            }
            //Adding the leave applications to the substitute instead
            if (isSubstitute)                        
            {
                var leaveList = GetLeaveModel("Reporting.Reporting_ID", LM_ID);

                foreach (var leave in leaveList)
                {
                    if (leave.leaveStatusName.Equals("Pending_LM"))
                        RetrievedApplications.Add(leave);
                }
            }

            /*Get the list of applications due for the line manager to approve*/
            TempData["RetrievedApplications"] = RetrievedApplications;
            return View(RetrievedApplications);
        }

        [HttpPost]
        public ActionResult Index(sLeaveModel SL)
        {
            return Index();
        }

        [HttpGet]
        public ActionResult Select(int Id)
        {
            List<sLeaveModel> passedApplications = TempData["RetrievedApplications"] as List<sLeaveModel>;
            sLeaveModel passingLeave = passedApplications.First(leave => leave.leaveAppID.Equals(Id));
            List<string> balanceStrings = new List<string>();
            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            string queryString = "SELECT * FROM dbo.Leave_Balance, dbo.Employee " +
                "WHERE dbo.Leave_Balance.Employee_ID = '" + passingLeave.employeeID + "' " +
                "AND dbo.Employee.Employee_ID = '" + passingLeave.employeeID + "' ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string balanceType = GetLeaveType((int)reader["Leave_ID"]);
                            decimal balance = (decimal)reader["Balance"];
                            string empGender = (string)reader["Gender"];
                            if (empGender.Equals("M") && balanceType.Equals("Maternity"))
                            {
                                continue;
                            }
                            string email = (string)reader["Email"];
                            TempData["EmployeeEmail"] = email;
                            string balanceString = balanceType + ": " + balance.ToString();
                            balanceStrings.Add(balanceString);
                        }
                    }
                }
                connection.Close();
            }

            var leaveHistory = new List<sLeaveModel>();
            string queryString2 = "SELECT Leave_Name, Leave_Status_ID, Start_Date, Reporting_Back_Date, Start_Hrs, End_Hrs, Total_Leave " +
                "FROM dbo.leave l,dbo.Leave_Type t, dbo.Employee e " +
                "WHERE e.Employee_ID = " + passingLeave.employeeID + " AND e.Employee_ID = l.Employee_ID AND l.Leave_ID = t.Leave_ID AND l.leave_Status_ID IN (2,3,4,5)" +
                "ORDER BY Start_Date DESC";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var leave = new sLeaveModel();

                        leave.leaveTypeName = (string)reader["Leave_Name"];
                        leave.startDate = (DateTime)reader["Start_Date"];
                        leave.endDate = (DateTime)reader["Reporting_Back_Date"];
                        leave.leaveDuration = (decimal)reader["Total_Leave"];
                        leave.shortStartTime = (!DBNull.Value.Equals(reader["Start_Hrs"])) ? (TimeSpan)reader["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0);
                        leave.shortEndTime = (!DBNull.Value.Equals(reader["End_Hrs"])) ? (TimeSpan)reader["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0);
                        leave.leaveStatusID = (int)reader["Leave_Status_ID"];

                        leaveHistory.Add(leave);
                    }
                }
                connection.Close();
            }

            ViewData["LeaveHistory"] = leaveHistory;
            ViewData["BalanceStrings"] = balanceStrings;
            return View(passingLeave);
        }

        [HttpPost]
        public ActionResult Select(sLeaveModel SL, string submit)
        {
            string queryString = "";

            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;

            queryString = "SELECT dbo.Leave.Leave_ID FROM dbo.Leave WHERE dbo.Leave.Leave_Application_ID = '" + SL.leaveAppID + "'";
            int leaveID = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["Leave_ID"] != DBNull.Value)
                                leaveID = (int)(reader["Leave_ID"]); // Leave ID
                        }
                    }
                }
                connection.Close();
            }

            queryString = "SELECT dbo.Leave_Type.Leave_Name FROM dbo.Leave_Type WHERE dbo.Leave_Type.Leave_ID = '" + leaveID + "'";

            string leaveName = "";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["Leave_Name"] != DBNull.Value)
                                leaveName = (string)(reader["Leave_Name"]); // Leave Name
                        }
                    }
                }
                connection.Close();
            }


            string text = "";
            switch (submit)
            {
                case "Approve":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '1', " +
                        "LM_Comment = '" + SL.lmComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + SL.leaveAppID + "' ";
                    text = "Your " + leaveName + " leave application " + "from " + SL.startDate + " to " + SL.returnDate + " has been approved by your line manager. It is now awaiting review by Human Resources.";
                    break;
                case "Reject":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '3', " +
                        "LM_Comment = '" + SL.lmComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + SL.leaveAppID + "' ";
                    text = "Your " + leaveName + " leave application " + "from " + SL.startDate + " to " + SL.returnDate + " has been rejected by your line manager.";
                    break;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }


            /*Construct an e-mail and send it.*/
            string temp_email = TempData["EmployeeEmail"] as string;

            MailMessage message = new MailMessage();
            message.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");



            message.To.Add(new MailAddress(temp_email));

            message.Subject = "Leave Application Update";
            string body = "";
            body = body + text;

            message.Body = body;
            SmtpClient client = new SmtpClient();

            client.EnableSsl = true;

            client.Credentials = new NetworkCredential("project_ict333@murdochdubai.ac.ae", "ict@333");
            try
            {
                client.Send(message);
            }
            catch (Exception e)
            {
                Response.Write("<script> alert('The email could not be sent due to a network error.');</script>");
            }


            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult CreateList()
        {
            return View();
        }

        private string GetLeaveStatus(int statusID)
        {
            string statusInString = "";
            switch (statusID)
            {
                case 0:
                    statusInString = "PendingLM";
                    break;
                case 1:
                    statusInString = "PendingHR";
                    break;
                case 2:
                    statusInString = "Approved";
                    break;
                case 3:
                    statusInString = "RejectedLM";
                    break;
                case 4:
                    statusInString = "RejectedHR";
                    break;
                case 5:
                    statusInString = "Cancelled";
                    break;
            }

            return statusInString;
        }

        private string GetLeaveType(int leaveID)
        {
            string typeInString = "";
            switch (leaveID)
            {
                case 1:
                    typeInString = "Annual";
                    break;
                case 2:
                    typeInString = "Maternity";
                    break;
                case 3:
                    typeInString = "Sick";
                    break;
                case 4:
                    typeInString = "Compassionate";
                    break;
                case 5:
                    typeInString = "DIL";
                    break;
                case 6:
                    typeInString = "Short_Hours";
                    break;
            }
            return typeInString;
        }

    }
}