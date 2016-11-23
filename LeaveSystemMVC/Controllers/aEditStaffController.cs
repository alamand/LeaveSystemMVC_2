/*
 * Author: M Hamza Rahimy
 */
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
    public class aEditStaffController : Controller
    {
        /*
         * Look into @ http://stackoverflow.com/questions/32949510/add-items-to-select-list-on-the-client-side-in-mvc-5-asp
         */
        // GET: aEditStaff
        [HttpGet]
        public ActionResult Index()
        {
            sEmployeeModel EmptyEmployee = new sEmployeeModel();
            /*Get the list of departments and create a list of 
             minDepartment objects. This object will be passed into the 
             _MinDepartmentList partial*/
            List<minDepartment> tempDeps = new List<minDepartment>();
            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            string queryString = "SELECT Department_ID, Department_Name " +
                "FROM dbo.Department " + 
                "WHERE dbo.Department.Department_Name IS NOT NULL";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            tempDeps.Add(new minDepartment
                            {
                                departmentID = (int)reader[0],
                                departmentName = (string)reader[1]
                            });
                        }
                    }
                }
                connection.Close();
            }
            ViewData["DepartmentList"] = tempDeps;
            return View(tempDeps);
        }

        [HttpPost]
        public ActionResult Index(sEmployeeModel SE)
        {
            return View(SE);
        }
    }
}