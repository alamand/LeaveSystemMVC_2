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
    public class hrPendingApplicationController : ControllerBase
    {
        [HttpGet]
        // GET: hrPendingApplication
        public ActionResult Index()
        {
            List<sLeaveModel> RetrievedApplications = new List<sLeaveModel>();

            foreach (var leave in GetLeaveModel())
            {
                if (leave.leaveStatusName.Equals("Pending_HR"))
                    RetrievedApplications.Add(leave);
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
            System.Diagnostics.Debug.WriteLine("ID: " + Id);

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
                            string email = (string)reader["Email"];
                            TempData["EmployeeEmail"] = email;
                            if (empGender.Equals("M") && balanceType.Equals("Maternity"))
                            {
                                continue;
                            }
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
                        var leave = new sLeaveModel
                        {
                            leaveTypeID = (int)reader["Leave_ID"],
                            startDate = (DateTime)reader["Start_Date"],
                            endDate = (DateTime)reader["Reporting_Back_Date"],
                            leaveDuration = (decimal)reader["Total_Leave"],
                            shortStartTime = (!DBNull.Value.Equals(reader["Start_Hrs"])) ? (TimeSpan)reader["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                            shortEndTime = (!DBNull.Value.Equals(reader["End_Hrs"])) ? (TimeSpan)reader["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                            leaveStatusID = (int)reader["Leave_Status_ID"]
                        };

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

            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            string queryString = "";

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
                        System.Diagnostics.Debug.WriteLine("Found leave ID");
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
                                leaveName = (string) (reader["Leave_Name"]); // Leave Name
                        }
                    }
                }
                connection.Close();
            }

            string text = "";
            switch (submit)
            {
                case "Approve":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '2', " +
                        "HR_Comment = '" + SL.hrComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + SL.leaveAppID + "' ";
                    text = "Your " + leaveName + " leave application " + "from " + SL.startDate + " to " + SL.returnDate + " has been fully approved.";
                    break;
                case "Reject":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '4', " +
                        "HR_Comment = '" + SL.hrComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + SL.leaveAppID + "' ";
                    text = "Your " + leaveName + " leave application " + "from " + SL.startDate + " to " + SL.returnDate + " has been rejected by Human Resources.";
                    break;
            }
            
            using (var connection2 = new SqlConnection(connectionString))
            {
                var command2 = new SqlCommand(queryString, connection2);
                connection2.Open();
                using (var reader2 = command2.ExecuteReader())
                    connection2.Close();
            }


            /*Construct an e-mail and send it.*/
            string temp_email = TempData["EmployeeEmail"] as string;

            MailMessage message = new MailMessage();
            message.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");



            message.To.Add(new MailAddress(temp_email));

            message.Subject = "Leave Application Update";
            string body = "";
            body = body + text + Environment.NewLine;

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
                case 6:
                    statusInString = "secondLMPending";
                    break;
                case 7:
                    statusInString = "secondLMRejected";
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