using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class sLeaveBalanceController : Controller
    {
        // GET: sLeaveBalance
        public ActionResult Index()
        {

            var model = new List<Models.sleaveBalanceModel>();
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);   
            ViewBag.claim = c;
            string a = c.ToString();
            a = a.Substring(a.Length - 5);
            //System.Diagnostics.Debug.WriteLine("id is:"+a + ".");

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            string queryString = "Select * FROM dbo.Leave_Balance where Employee_ID = '"+a+"'";

           using( var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                int annual = 0;
                int sick = 0;
                int compassionate = 0;
                int maternity = 0;
                int shortHrs = 0;
                int unpaid = 0;

                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var balance = new Models.sleaveBalanceModel();

                        //string leave = "";

                        int id = (int)reader["Leave_ID"];
                        if (id == 1)
                            annual =  (int)reader["Balance"];
                        ViewBag.annual = annual;    

                        if (id == 2)
                            sick = (int)reader["balance"];
                        ViewBag.sick = sick;

                        if (id == 3)
                            compassionate = (int)reader["balance"];
                        ViewBag.compassionate = compassionate;

                        if (id == 4)
                           maternity = (int)reader["balance"];
                        ViewBag.maternity = maternity;

                        if (id == 5)
                           shortHrs = (int)reader["balance"];
                        ViewBag.shortHrs = shortHrs;

                        if (id == 6)
                            unpaid = (int)reader["balance"];
                        ViewBag.unpaid = unpaid;

                        model.Add(balance);
                    }
                }
                connection.Close();
            }


            return View(model);
        }
    }
}