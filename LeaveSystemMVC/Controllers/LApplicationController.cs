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
            string queryString = "Select Gender FROM dbo.Employee where Employee_ID = '" + a + "'";
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
                    }
                }
                connection.Close();

                System.Diagnostics.Debug.WriteLine("Entered get");
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
            System.Diagnostics.Debug.WriteLine("Entered post");
            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                // extract only the filename
                var fileName = Path.GetFileName(file.FileName);
                 fileName = a+fileName;
                System.Diagnostics.Debug.WriteLine("filename  is:"+fileName);
                System.Diagnostics.Debug.WriteLine("file Uploaded");
                fileN = fileName;
                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/App_Data"),fileName);
                file.SaveAs(path);
            }
            DateTime d1 = model.startDate;
            string stdate = d1.ToString("yyyy-MM-dd");

            DateTime d2 = model.endDate;
            string endate = d2.ToString("yyyy-MM-dd");
            System.Diagnostics.Debug.WriteLine("Endate is: " +endate);

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
                    System.Diagnostics.Debug.WriteLine("Difference is" + days);
                    for (var i = 0; i <= days; i++)
                    {
                        var testDate = d1.AddDays(i);
                        switch (testDate.DayOfWeek)
                        {
                            case DayOfWeek.Saturday:
                            case DayOfWeek.Friday:
                                days--;
                                //Console.WriteLine(testDate.ToShortDateString());
                                break;
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
                                System.Diagnostics.Debug.WriteLine("Day of holiday is: " + day.DayOfWeek);
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
                        System.Diagnostics.Debug.WriteLine("Maternity Days: " + days);
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
                        ModelState.AddModelError("endDate", "For short leave, THe start date and return date should be same!");
                    }
                }

                if (model.leaveType.Equals("Unpaid"))
                {
                    leaveId = 6;
                }
                    
                string leaveid = leaveId.ToString();

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
                Response.Write("<script> alert('Your leave application has been submitted.');location.href='Create'</script>");
                connection.Close();
            }
            return Create();
        }
    }
}