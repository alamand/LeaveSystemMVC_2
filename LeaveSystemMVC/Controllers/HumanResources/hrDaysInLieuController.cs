using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class hrDaysInLieuController : Controller
    {
        // GET: hrDaysInLieu
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Select()
        {
            minStaff model = new minStaff();
            Dictionary<int, string> EmployeeList = new Dictionary<int, string>();
            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            string queryString = "SELECT Employee_ID, First_Name, Last_Name " +
                "FROM dbo.Employee " +
                "WHERE dbo.Employee.First_Name IS NOT NULL ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string fullName = (string)reader[1] + " " + (string)reader[2];
                            EmployeeList.Add((int)reader[0], fullName);
                        }
                    }
                }
                connection.Close();
            }
            ViewData["EmployeeList"] = EmployeeList;
            return View();
        }

        [HttpPost]
        public ActionResult Index(hrDaysInLieu DL)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "INSERT into dbo.Days_In_Lieu VALUES ('" + TempData["EmpID"] + "' , '" + DL.Date.ToString("yyyy-MM-dd") + "' , '" + DL.NumDays + "' , '" + DL.Comment + "')";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }
            
            queryString = "UPDATE dbo.Leave_Balance SET Balance = Balance + '" +  DL.NumDays + "' WHERE Employee_ID = '" + TempData["EmpID"] + "' AND Leave_ID = 5";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }
            //@todo: Get popup alert working
            Response.Write("<script> alert('The days in lieu have been successfully credited.');</script>");
            return RedirectToAction("Select");
        }

        [HttpPost]
        public ActionResult Select(minStaff empMod)
        {
            if (empMod.empIDAsString != null)
            {
                TempData["EmpID"] = empMod.empIDAsString;
                return RedirectToAction("Index");
            }
            return Select();
        }
    }
}