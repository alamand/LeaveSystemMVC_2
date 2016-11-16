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

            DateTime d3 = model.shortStartTime;
            string stTime = d3.ToString("HH:mm:ss");

            DateTime d4 = model.shortEndTime;
            string endTime = d4.ToString("HH:mm:ss");

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
                if (result == 0)
                    System.Diagnostics.Debug.WriteLine("is the same time as");

                int leaveId = 0;
                if (model.leaveType.Equals("Annual"))
                {
                    leaveId = 1;
                }
                if (model.leaveType.Equals("Sick"))
                {
                    leaveId = 2;
                }
                if (model.leaveType.Equals("Compassionate"))
                {
                    leaveId = 3;
                }
                if (model.leaveType.Equals("Maternity"))
                {
                    leaveId = 4;
                }
                if (model.leaveType.Equals("Short"))
                {
                    leaveId = 5;
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
                a = a.Substring(a.Length - 8);
                //System.Diagnostics.Debug.WriteLine("id is:"+a + ".");

                bool ticket = model.bookAirTicket;

                //System.Diagnostics.Debug.WriteLine("ticket is:" +ticket);


                string abc = model.comments;

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                string queryString = "Insert INTO Leave (Employee_ID,Start_Date,End_Date,Reporting_Back_Date,Leave_ID,Contact_Outside_UAE,Comment,Flight_Ticket,Total_Leave_Days,Start_Hrs,End_Hrs,Status) VALUES ('" + a + "','" + stdate + "','" + endate + "','" + endate + "','" + leaveid + "','" + model.contactDetails + "','" + model.comments + "','" + ticket + "','2','" + stTime + "','" + endTime + "','1');";

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