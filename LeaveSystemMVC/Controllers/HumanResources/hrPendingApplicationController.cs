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
    public class hrPendingApplicationController : Controller
    {
        [HttpGet]
        // GET: hrPendingApplication
        public ActionResult Index()
        {
            string userID = "";
            List<sLeaveModel> RetrievedApplications = new List<sLeaveModel>();

            //to get the id of the person logged in 
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (c != null)
                {
                    userID = c.Value;
                }
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            string queryString = "SELECT dbo.Leave.Leave_Application_ID, dbo.Leave.Employee_ID, " +
                "dbo.Leave.Start_Date, dbo.Leave.End_Date, dbo.Leave.Reporting_Back_Date, " +
                "dbo.Leave.Leave_ID, dbo.Leave.Contact_Outside_UAE, dbo.Leave.Comment, " +
                "dbo.Leave.Document, dbo.Leave.Flight_Ticket, dbo.Leave.Total_Leave, " +
                "dbo.Leave.Start_Hrs, dbo.Leave.End_Hrs, dbo.Leave.Leave_Status_ID, " +
                "dbo.Leave.LM_Comment, dbo.Leave.HR_Comment, dbo.Employee.First_Name, dbo.Employee.Last_Name " +
                "FROM dbo.Leave " +
                "FULL JOIN dbo.Employee " +
                "ON dbo.Leave.Employee_ID = dbo.Employee.Employee_ID " +
                /*
                "FULL JOIN dbo.Department " +
                "ON dbo.Employee.Department_ID = dbo.Department.Department_ID " + */
                "WHERE dbo.Leave.Leave_Status_ID = '1' " +
                "AND dbo.Leave.Leave_ID IS NOT NULL ";
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
                            var leave = new Models.sLeaveModel();

                            leave.comments = (string)reader["Comment"];
                            leave.bookAirTicket = (bool)reader["Flight_Ticket"];
                            leave.contactDetails = (string)reader["Contact_Outside_UAE"];

                            if (reader["Leave_ID"] != DBNull.Value)
                                leave.leaveType = GetLeaveType((int)reader["Leave_ID"]); // Leave Type ID
                            var lidint = (int)reader["Leave_Application_ID"]; //Leave Application ID
                            leave.leaveID = lidint;

                            leave.startDate = (DateTime)reader["Start_Date"];

                            string date1 = leave.startDate.ToString("yyyy-MM-dd");

                            //ViewBag.stDt = date1;

                            leave.endDate = (DateTime)reader["End_Date"];
                            string date2 = leave.endDate.ToString("yyyy-MM-dd");
                            //ViewBag.enDt = date2;

                            leave.returnDate = (DateTime)reader["Reporting_Back_Date"];

                            string date3 = leave.returnDate.ToString("yyyy-MM-dd");

                            leave.leaveDuration = (decimal)reader["Total_Leave"];
                            if (!reader.IsDBNull(11))
                            {
                                leave.shortStartTime = (TimeSpan)reader["Start_Hrs"];
                            }
                            else
                            {
                                leave.shortStartTime = new TimeSpan(0, 0, 0, 0, 0);
                            }
                            if (!reader.IsDBNull(12))
                            {
                                leave.shortEndTime = (TimeSpan)reader["End_Hrs"];
                            }
                            else
                            {
                                leave.shortEndTime = new TimeSpan(0, 0, 0, 0, 0);
                            }

                            leave.leaveStatus = (int)reader["Leave_Status_ID"];
                            if (!reader.IsDBNull(15))
                                leave.hrComment = (string)reader["HR_Comment"];
                            else
                                leave.hrComment = "";
                            if (!reader.IsDBNull(14))
                                leave.lmComment = (string)reader["LM_Comment"];
                            else
                                leave.hrComment = "";

                            string empFirstName = (string)reader["First_Name"];
                            string empLastName = (string)reader["Last_Name"];
                            leave.staffName = empFirstName + " " + empLastName;
                            int empID = (int)reader["Employee_ID"];
                            leave.employeeID = empID;
                            RetrievedApplications.Add(leave);
                        }
                    }
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
        public ActionResult Select(string Id)
        {
            List<sLeaveModel> passedApplications = TempData["RetrievedApplications"] as List<sLeaveModel>;
            sLeaveModel passingLeave = passedApplications.First(leave => leave.leaveID.Equals(Id));
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
                        var leave = new sLeaveModel();

                        leave.leaveType = (string)reader["Leave_Name"];
                        leave.startDate = (DateTime)reader["Start_Date"];
                        leave.endDate = (DateTime)reader["Reporting_Back_Date"];
                        leave.leaveDuration = (decimal)reader["Total_Leave"];
                        leave.shortStartTime = (!DBNull.Value.Equals(reader["Start_Hrs"])) ? (TimeSpan)reader["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0);
                        leave.shortEndTime = (!DBNull.Value.Equals(reader["End_Hrs"])) ? (TimeSpan)reader["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0);
                        leave.leaveStatus = (int)reader["Leave_Status_ID"];

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

            queryString = "SELECT dbo.Leave.Leave_ID FROM dbo.Leave WHERE dbo.Leave.Leave_Application_ID = '" + SL.leaveID + "'";
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
                        "WHERE dbo.Leave.Leave_Application_ID = '" + SL.leaveID + "' ";
                    text = "Your " + leaveName + " leave application " + "from " + SL.startDate + " to " + SL.returnDate + " has been fully approved.";
                    break;
                case "Reject":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '4', " +
                        "HR_Comment = '" + SL.hrComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + SL.leaveID + "' ";
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