using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class hrCreditLeaveBalanceController : Controller
    {
        // GET: hrCreditLeaveBalance
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Credit(string value)
        {
            //2 steps
            //1: Query DB for all Leave types
            //2: Sort by Leave type and update the values
            //QUery DB for leave name and duration
            //Send in parameter leave name and duration
            //For Counting Rows SELECT COUNT(*) FROM dbo.Leave_Balance;
            if (ModelState.IsValid)
            {
              /*  var list = new List<Leave>();
                list.Add(new Leave(1,1));
                foreach (var item in list)
                {
                    CreditBalance(item.id, item.duration);
                }*/
                ModelState.AddModelError("Save", "Save Button Clicked");
                return RedirectToAction("Index");
            }
            return View("Index");
        }
        public struct Leave
        {
            public Leave(int a, int b) { id = a; duration = b; }
            public int id { get; set; }
            public int duration { get; set; }
        }
        public void CreditBalance(int leaveID, int balance)
        {
            var model = new List<Models.hrHolidaysCalender>();
            var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            string queryString = "Update dbo.Leave_Balance SET Balance='"+balance+"' WHERE Leave_ID='"+leaveID+"'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var calender = new Models.hrHolidaysCalender();
                        calender.holidayName = (string)reader["Name"];
                        calender.startDate = (DateTime)reader["Date"];
                        model.Add(calender);
                    }
                }

                connection.Close();
            }

        }

    }
}