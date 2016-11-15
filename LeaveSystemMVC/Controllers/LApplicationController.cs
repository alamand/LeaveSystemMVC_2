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


            DateTime d1 = model.startDate;
            string stdate = d1.ToString("yyyy-MM-dd");

            DateTime d2 = model.endDate;
            string endate = d2.ToString("yyyy-MM-dd");

            int leaveId = 0;
            if (model.leaveType.Equals("Annual"))
            {
                leaveId = 1;
                System.Diagnostics.Debug.WriteLine("annual leave selected id is: " + leaveId);
            }
            if (model.leaveType.Equals("Sick"))
            {
                leaveId = 2;
                System.Diagnostics.Debug.WriteLine("annual leave selected id is: " + leaveId);
            }
            if (model.leaveType.Equals("Compassionate"))
            {
                leaveId = 3;
                System.Diagnostics.Debug.WriteLine("annual leave selected id is: " + leaveId);
            }
            if (model.leaveType.Equals("Maternity"))
            {
                leaveId = 4;
                System.Diagnostics.Debug.WriteLine("annual leave selected id is: " + leaveId);
            }
            if (model.leaveType.Equals("Short"))
            {
                leaveId = 5;
                System.Diagnostics.Debug.WriteLine("annual leave selected id is: " + leaveId);
            }
            if (model.leaveType.Equals("Unpaid"))
            {
                leaveId = 6;
                System.Diagnostics.Debug.WriteLine("annual leave selected id is: " + leaveId);
            }

            string leaveid = leaveId.ToString();

            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            ViewBag.claim = c;
            string a = c.ToString();
            a = a.Substring(a.Length - 8);
            System.Diagnostics.Debug.WriteLine("id is:"+a + ".");


            string abc = model.comments;

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            string queryString = "Insert INTO Leave (Employee_ID,Start_Date,End_Date,Reporting_Back_Date,Leave_ID,Comment,Total_Leave_Days,Status) VALUES ('" + a +"','"+ stdate +"','"+ endate +"','" + endate + "','" + leaveid + "','"+ model.comments +"','2','1');";
            //string queryString = "Select * from dbo.Leave";

            var connection = new SqlConnection(connectionString);

                connection.Open();
                System.Diagnostics.Debug.WriteLine("connection opened");
                var command = new SqlCommand(queryString, connection);

                command.ExecuteNonQuery();
            System.Diagnostics.Debug.WriteLine("connection executed");
            connection.Close();
            
            return Create();
        }
    }
}