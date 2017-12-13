using LeaveSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Claims;
using System.IO;
using System.Net.Mail;
using System.Net;

namespace LeaveSystemMVC.Controllers
{
    public class LApplicationController : Controller
    {
        // GET: LApplication
        [HttpGet]
        public ActionResult Create()
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            ViewBag.claim = c;
            string a = c.ToString();
            a = a.Substring(a.Length - 5);

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Gender, Email FROM dbo.Employee where Employee_ID = '" + a + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                char gender = '\0';
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gender = Convert.ToChar(reader["Gender"]);
                        TempData["EmployeeEmail"] = (string)(reader["Email"]);
                    }
                }
                connection.Close();

                List<string> leaves = new List<string>();
                leaves.Add("Annual");
                leaves.Add("Sick");
                leaves.Add("Compassionate");
                if (gender == 'F')
                {
                    leaves.Add("Maternity");
                }
                leaves.Add("Short");
                leaves.Add("Unpaid");
                ViewBag.leave = leaves;
            }
           
            return View();
        }

        [HttpPost]
        public ActionResult Create(Models.sLeaveModel model, HttpPostedFileBase file) {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            ViewBag.claim = c;
            string a = c.ToString();
            a = a.Substring(a.Length - 5);
            string fileN = "";
            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                // extract only the filename
                var fileName = Path.GetFileName(file.FileName);
                 fileName = a+fileName;
                fileN = fileName;
                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/App_Data"),fileName);
                file.SaveAs(path);
            }
            DateTime d1 = model.startDate;
            string stdate = d1.ToString("yyyy-MM-dd");

            DateTime d2 = model.endDate;
            string endate = d2.ToString("yyyy-MM-dd");

            TimeSpan difference = d2 - d1;
            

            DateTime today = DateTime.Today;

            int result = DateTime.Compare(d2, d1);

            /* Can apply e.g. for sick leave in the past
            if (d1 < today) {
                ModelState.AddModelError("startDate", "The Start Date cannot be in the Past");
            }*/

            if (result < 0) {
                ModelState.AddModelError("endDate", "The reporting back date cannot be earlier than the start date.");                
            }

            if(result == 0)
            {
                ModelState.AddModelError("endDate", "The start and reporting back dates cannot be the same.");
            }

                int days = 0;
                if (result > 0 && !(model.leaveType.Equals("Maternity"))) {
                    TimeSpan diff = d2 - d1;
                    days = diff.Days;
                    int tempDays = days;
                    for (var i = 1; i <= tempDays; i++)
                    {
                        var testDate = d1.AddDays(i);
                        switch (testDate.DayOfWeek)
                        {
                            case DayOfWeek.Saturday: days--; break;
                            case DayOfWeek.Friday: days--; break;
                        }
                    }

                    var connectionString1 = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    string query1 = "Select * from dbo.Public_Holiday where Date between'" + stdate + "' AND '" + endate + "'";

                    using (var connection1 = new SqlConnection(connectionString1))
                    {
                        var command1 = new SqlCommand(query1, connection1);

                        connection1.Open();
                        using (var reader1 = command1.ExecuteReader())
                        {
                            while (reader1.Read())
                            {
                                DateTime day = (DateTime)reader1["Date"];
                                if (day.DayOfWeek.Equals(DayOfWeek.Saturday) || day.DayOfWeek.Equals(DayOfWeek.Friday))
                                { System.Diagnostics.Debug.WriteLine("Holiday on weekend"); }

                                else
                                {
                                    days--;
                                }
                            }
                        }
                        connection1.Close();
                     }
                }
                

                int leaveId = 0;
                if (model.leaveType.Equals("Annual"))
                {
                    leaveId = 1;
                    //30 days beforehand need not be enforced by the system
                    /*double difference = (d1 - today).TotalDays;
                    if(difference<30)
                    ModelState.AddModelError("startDate", "Leave must be applied 30 days in advance");*/
                }
                if (model.leaveType.Equals("Sick"))
                {
                    leaveId = 3;
                }
                if (model.leaveType.Equals("Compassionate"))
                {
                    leaveId = 4;
                }
                if (model.leaveType.Equals("Maternity"))
                {
                    leaveId = 2;

                //    ModelState.AddModelError("startDate", "Maternity is disabled;");

                    TimeSpan diff = d2 - d1;
                        days = diff.Days;
                        
                        var connectionString1 = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                        string query1 = "Select * from dbo.Public_Holiday where Date between'" + stdate + "' AND '" + endate + "'";

                        using (var connection1 = new SqlConnection(connectionString1))
                        {
                            var command1 = new SqlCommand(query1, connection1);

                            connection1.Open();
                            using (var reader1 = command1.ExecuteReader())
                            {
                                while (reader1.Read())
                                {
                                    days--;
                                }
                            }
                            connection1.Close();
                            System.Diagnostics.Debug.WriteLine("Maternity Days: After holiday Deduction" + days);
                        }
                    
                }
                if (model.leaveType.Equals("Short"))
                {
                    leaveId = 6;
                    if (model.startDate != model.endDate)
                    {
                        ModelState.AddModelError("endDate", "For short leave, the start date and return date should be same!");
                    }
                }

                if (model.leaveType.Equals("Unpaid"))
                {
                    leaveId = 6;
                }
                    
                string leaveid = leaveId.ToString();
            if (days == 0)
            {
                ModelState.AddModelError("endDate", "You cannot apply for zero days of leave.");
            }

            if (ModelState.IsValid)
            {
                bool ticket = model.bookAirTicket;
                
                string abc = model.comments;

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                string queryString = "Insert INTO Leave (Employee_ID,Document,Start_Date,End_Date,Reporting_Back_Date,Leave_ID,Contact_Outside_UAE,Comment,Flight_Ticket,Total_Leave_Days,Start_Hrs,End_Hrs,Leave_Status_ID) VALUES ('" + a + "','"+fileN+"','" + stdate + "','" + endate + "','" + endate + "','" + leaveid + "','" + model.contactDetails + "','" + model.comments + "','" + ticket + "','"+days+"','" + model.shortStartTime + "','" + model.shortEndTime + "','0');";

                var connection = new SqlConnection(connectionString);

                connection.Open();
                var command = new SqlCommand(queryString, connection);

                command.ExecuteNonQuery();

                /*Construct an e-mail and send it to the employee.*/

                queryString = "SELECT dbo.Leave.Leave_ID FROM dbo.Leave WHERE dbo.Leave.Leave_Application_ID = '" + model.leaveID + "'";
                int leaveID = 0;
                using (var connection2 = new SqlConnection(connectionString))
                {
                    var command2 = new SqlCommand(queryString, connection2);
                    connection2.Open();
                    using (var reader = command2.ExecuteReader())
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
                using (var connection2 = new SqlConnection(connectionString))
                {
                    var command2 = new SqlCommand(queryString, connection2);
                    connection2.Open();
                    using (var reader = command2.ExecuteReader())
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

                string temp_email = TempData["EmployeeEmail"] as string;
                MailMessage message = new MailMessage();
                message.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");

                message.To.Add(new MailAddress(temp_email));

                message.Subject = "Leave Application Update";
                string body = "";
                string text = "Your " + leaveName + " leave application " + "from " + model.startDate + " to " + model.returnDate + " has been sent to your line manager for approval.";
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
                Response.Write("<script> alert('Your leave application has been submitted.');location.href='Create'</script>");
                connection.Close();
            }
            return Create();
        }
    }
}