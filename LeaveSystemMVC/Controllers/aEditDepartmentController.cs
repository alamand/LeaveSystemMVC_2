using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class aEditDepartmentController : Controller
    {
        // GET: aEditDepartment
        public ActionResult Index()
        {

            List<string> departmentNames = new List<string>();
            List<string> lm = new List<string>();

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Department_Name FROM dbo.Department ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        departmentNames.Add((string)reader[0]);
                    }
                }


                queryString = "Select First_Name,Last_Name FROM dbo.Employee Full Join dbo.Employee_Role On dbo.Employee_Role.Employee_ID = dbo.Employee.Employee_ID WHERE Employee_Role.Role_ID ='3' ";
                command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                {
                    //string lmName = First_Name + " " + Last_Name;

                    while (reader.Read())
                    {
                        lm.Add((string)reader[0]);
                    }
                }

                connection.Close();
                ViewBag.departmentNames = departmentNames;
                ViewBag.lm = lm;
            }

            return View();
        }
    }
}