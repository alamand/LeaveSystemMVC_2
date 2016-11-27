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

    public class lmPendingApplicationsController : Controller
    {
        [HttpGet]
        // GET: lmPendingApplications
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
                "dbo.Leave.Document, dbo.Leave.Flight_Ticket, dbo.Leave.Total_Leave_Days, " +
                "dbo.Leave.Start_Hrs, dbo.Leave.End_Hrs, dbo.Leave.Status, " + 
                "dbo.Leave.LM_Comment, dbo.Leave.HR_Comment, dbo.Employee.First_Name, dbo.Employee.Last_Name " +   
                "FROM dbo.Leave " +
                "FULL JOIN dbo.Employee " +
                "ON dbo.Leave.Employee_ID = dbo.Employee.Employee_ID " +
                "FULL JOIN dbo.Department " +
                "ON dbo.Employee.Department_ID = dbo.Department.Department_ID " +
                "WHERE dbo.Department.Line_Manager_ID = '" + userID + "' " +
                "AND dbo.Leave.Status = '0'" + 
                "AND dbo.Leave.Leave_ID IS NOT NULL ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            var leave = new Models.sLeaveModel();

                            if (reader["Leave_ID"] != DBNull.Value)
                                leave.leaveType = GetLeaveType((int)reader["Leave_ID"]); // Leave Type ID
                            var lidint = (int)reader["Leave_Application_ID"]; //Leave Application ID
                            leave.leaveID = lidint.ToString();

                            leave.startDate = (DateTime)reader["Start_Date"];

                            string date1 = leave.startDate.ToString("yyyy-MM-dd");

                            //ViewBag.stDt = date1;

                            leave.endDate = (DateTime)reader["End_Date"];
                            string date2 = leave.endDate.ToString("yyyy-MM-dd");
                            //ViewBag.enDt = date2;

                            leave.leaveDuration = (int)reader["Total_Leave_Days"];
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

                            leave.leaveStatus = (int)reader["Status"];
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
                            leave.employeeID = empID.ToString();
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
                "WHERE dbo.Leave_Balance.Employee_ID = '" + passingLeave.employeeID +"' " + 
                "AND dbo.Employee.Employee_ID = '" + passingLeave.employeeID + "' ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            string balanceType = GetLeaveType((int)reader["Leave_ID"]);
                            int balance = (int)reader["Balance"];
                            string empGender = (string)reader["Gender"];
                            if(empGender.Equals("M") && balanceType.Equals("Maternity"))
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

            ViewData["BalanceStrings"] = balanceStrings;
            return View(passingLeave); 
        }

        [HttpPost]
        public ActionResult Select(sLeaveModel SL, string submit)
        {
            string queryString = "";
            int lid;
            int.TryParse(SL.leaveID, out lid);
            switch (submit)
            {
                case "Approve":
                    queryString = "UPDATE dbo.Leave SET Status = '2', " +
                        "LM_Comment = '" + SL.lmComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + lid + "' ";
                    break;
                case "Reject":
                    queryString = "UPDATE dbo.Leave SET Status = '4', " +
                        "LM_Comment = '" + SL.lmComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + lid + "' ";
                    break;
            }
            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
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
            body = body + "You application has been fully approved.";

            message.Body = body;
            SmtpClient client = new SmtpClient();

            client.EnableSsl = true;

            client.Credentials = new NetworkCredential("project_ict333@murdochdubai.ac.ae", "ict@333");
            client.Send(message);


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