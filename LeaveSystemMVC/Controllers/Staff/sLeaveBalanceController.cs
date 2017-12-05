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
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);   
            ViewBag.claim = c;
            string a = c.ToString();
            a = a.Substring(a.Length - 5);

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;          
            string queryString = "Select Gender, Balance,Leave_Name FROM dbo.Employee, dbo.Leave_Balance, dbo.Leave_Type where Leave_Balance.Employee_ID = Employee.Employee_ID AND Leave_Balance.Employee_ID = '" + a+"' AND Leave_Balance.Leave_ID = Leave_Type.Leave_ID";

            using ( var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();

                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {

                        ViewBag.gender = Convert.ToChar(reader["Gender"]);            
                        string leave = (string)reader["Leave_Name"];
                        System.Diagnostics.Debug.WriteLine("Gender:"+ Convert.ToChar(reader["Gender"]));

                        if (leave.Equals("Annual"))
                            ViewBag.annual = (decimal)reader["Balance"];

                        if (leave.Equals("Sick"))
                            ViewBag.sick = (decimal)reader["Balance"];

                        if (leave.Equals("Compassionate"))
                            ViewBag.compassionate = (decimal)reader["Balance"];

                        if (leave.Equals("Maternity"))
                            ViewBag.maternity = (decimal)reader["Balance"];

                        if (leave.Equals("Short_Hours"))
                            ViewBag.shortHrs = (decimal)reader["Balance"];

                        if (leave.Equals("Unpaid"))
                            ViewBag.unpaid = (decimal)reader["Balance"];

                        if (leave.Equals("DIL"))
                            ViewBag.DIL = (decimal)reader["Balance"];  
                    }
                }
                connection.Close();
            }

            string queryString1 = "SELECT DATEDIFF(day,Emp_Start_Date,GETDATE()) AS DiffDate from dbo.Employment_Period where Employee_ID = '" + a + "';";
            using (var connection1 = new SqlConnection(connectionString))
            {
                var command1 = new SqlCommand(queryString1, connection1);
                connection1.Open();
                using (var reader1 = command1.ExecuteReader())
                {
                    while (reader1.Read())
                    {
                        int dif = (int)reader1["DiffDate"];
                        if (dif < 183) {
                            ViewBag.annual = 0;
                        }
                    }

                }
                connection1.Close();
            }

            return View();
        }
    }
}