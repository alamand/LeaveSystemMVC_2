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
        public ActionResult Index(int? value)
        {
            if (ModelState.IsValid)
            {
                var list = new List<Leave>();
                var driver = new List<Leave>();
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
                string queryString = "Select * from LeaveSystem.dbo.Leave_Type";
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(queryString, connection);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //if not driver
                            var id = (int)reader["Leave_ID"];
                            var duration = (int)reader["Duration"];
                            list.Add(new Leave(id,duration ));
                            //if driver CreditDriver()
                        }
                    }

                    connection.Close();
                }
                foreach (var item in list)
                  {
                      CreditBalance(item.id, item.duration);
                  }
            }
            return View();
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
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            string queryString = "Update dbo.Leave_Balance SET Balance='"+balance+"' WHERE Leave_ID='"+leaveID+"'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                }

                connection.Close();
            }

        }
        public bool isDriver(int id)
        {
            return false;
        }
        public Leave CreditDriver(int id, int duration)
        {
            return new Leave(id,duration);
        }


    }
}