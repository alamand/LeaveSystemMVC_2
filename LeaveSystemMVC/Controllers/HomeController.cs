using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var identity = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            string loggedInID = identity.ToString();
            loggedInID = loggedInID.Substring(loggedInID.Length - 5);

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string query = "Select First_Name, Last_Name FROM dbo.Employee WHERE Employee_ID = " + loggedInID;
            string name = "";
            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        name = (string)reader["First_Name"] + " " + (string)reader["Last_Name"];
                    }
                    connection.Close();
                }
            }

            Session["UserName"] = name;
            return View();
        }
    }
}