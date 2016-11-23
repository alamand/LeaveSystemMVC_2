using LeaveSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Claims;



namespace LeaveSystemMVC.Controllers
{
    public class LApplicationController : Controller
    {
        // GET: LApplication
        [HttpGet]
        public ActionResult Create()
        {
            System.Diagnostics.Debug.WriteLine("Entered get");
            List<string> leaves = new List<string>() ;
            leaves.Add("Annual");
            leaves.Add("Sick");
            leaves.Add("Compassionate");
            leaves.Add("Maternity");
            leaves.Add("Short");
            leaves.Add("Unpaid");
            ViewBag.leave = leaves;
            return View();
        }

        [HttpPost]
        public ActionResult Create(Models.sLeaveModel model) {
            System.Diagnostics.Debug.WriteLine("Entered post");

//            DateTime d3 = model.shortStartTime;
  //          string stTime = d3.ToString("HH:mm:ss");

            //DateTime d4 = model.shortEndTime;
            //string endTime = d4.ToString("HH:mm:ss");

            DateTime d1 = model.startDate;
            string stdate = d1.ToString("yyyy-MM-dd");

            DateTime d2 = model.endDate;
            string endate = d2.ToString("yyyy-MM-dd");
            System.Diagnostics.Debug.WriteLine("Endate is: " +endate);

            int result = DateTime.Compare(d2, d1);

            if (result < 0) {
                ModelState.AddModelError("endDate", "The End Date cannot be earlier than the start date");
                Response.Write("<script> alert('End Date Cannot be earlier than the Start Date');location.href='Create'</script>");
                System.Diagnostics.Debug.WriteLine("is earlier than");
                //Redirect(Create.UrlReferrer.ToString());
            }
            if (ModelState.IsValid)
                
            {
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
                                System.Diagnostics.Debug.WriteLine("weekend reached");
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
                                    System.Diagnostics.Debug.WriteLine("Holiday Deducted");
                                }
                            }
                        }
                        connection1.Close();
                     }
                    System.Diagnostics.Debug.WriteLine("Not Maternity Days: " +days);

                }

               else {
                    TimeSpan diff = d2 - d1;
                    days = diff.Days;
                    System.Diagnostics.Debug.WriteLine("Maternity Days: " + days);
                    var connectionString1 = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    string query1 = "Select * from dbo.Public_Holiday where Date between'"+ stdate + "' AND '"+ endate +"'";

                    using (var connection1 = new SqlConnection(connectionString1)) {
                        var command1 = new SqlCommand(query1, connection1);

                        connection1.Open();
                        using (var reader1 = command1.ExecuteReader()) {
                            while (reader1.Read()) {
                                    days--;
                            }
                        }
                        connection1.Close();
                        System.Diagnostics.Debug.WriteLine("Maternity Days: After holiday Deduction" + days);
                    }
                }

                int leaveId = 0;
                if (model.leaveType.Equals("Annual"))
                {
                    leaveId = 1;
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
                }
                if (model.leaveType.Equals("Short"))
                {
                    leaveId = 6;
                }
                if (model.leaveType.Equals("Unpaid"))
                {
                    leaveId = 6;
                }
                    
                string leaveid = leaveId.ToString();

                var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
                var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                ViewBag.claim = c;
                string a = c.ToString();
                a = a.Substring(a.Length - 5);
                //System.Diagnostics.Debug.WriteLine("id is:"+a + ".");

                bool ticket = model.bookAirTicket;

                //System.Diagnostics.Debug.WriteLine("ticket is:" +ticket);
                

                string abc = model.comments;

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                string queryString = "Insert INTO Leave (Employee_ID,Start_Date,End_Date,Reporting_Back_Date,Leave_ID,Contact_Outside_UAE,Comment,Flight_Ticket,Total_Leave_Days,Start_Hrs,End_Hrs,Status) VALUES ('" + a + "','" + stdate + "','" + endate + "','" + endate + "','" + leaveid + "','" + model.contactDetails + "','" + model.comments + "','" + ticket + "','"+days+"','" + model.shortStartTime + "','" + model.shortEndTime + "','0');";

                var connection = new SqlConnection(connectionString);

                connection.Open();
                //    System.Diagnostics.Debug.WriteLine("connection opened");
                var command = new SqlCommand(queryString, connection);

                command.ExecuteNonQuery();
                //  System.Diagnostics.Debug.WriteLine("connection executed");
                Response.Write("<script> alert('Leave Application Submitted');location.href='Create'</script>");
                connection.Close();
            }
            return Create();
        }
    }
}