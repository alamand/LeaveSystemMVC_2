
//Adding a new leave type by hr Controller 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrCreateLeaveTypeController : Controller
    {
        // GET: hrCreateLeaveType
        public ActionResult Index()
        {
            List<string> leaveTypeName = new List<string>() ;

            //get a list of existing leave types from the database 
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Leave_Name From dbo.Leave_Type";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        leaveTypeName.Add((string)reader[0]);
                    }
                }
                connection.Close();
                ViewBag.leaveTypeName = leaveTypeName;
            }
                return View();
        }

        [HttpPost]
        public ActionResult Index (Models.hrCreateLeave createNewLeave)
        {
            //checks if leave name already exists
            if (createNewLeave.newLeaveType == null)
            {
                ModelState.AddModelError("newLeaveType", "Leave Name cannot be empty");

            }

            //checks if leave name already exists 
            List<string> leaveNames = new List<string>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Leave_Name FROM dbo.Leave_Type ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        leaveNames.Add((string)reader[0]);
                    }
                }
                connection.Close();
            }
            foreach (var leave in leaveNames)
            {
                if (createNewLeave.newLeaveType.ToLower() == leave.ToLower())
                {
                    ModelState.AddModelError("newLeaveType", "Leave Type already exists");

                }
            }

            //checks if leave name has more than 30 charecters
            if (createNewLeave.newLeaveType.Length>30)
            {
                ModelState.AddModelError("newLeaveType", "Leave Type too long");
            }

            //checks if leave name has digits
            foreach (char c in createNewLeave.newLeaveType)
            {
                if (char.IsDigit(c))
                {
                    ModelState.AddModelError("newLeaveType", "Leave Type cannot contain digits");
                }
            }

            //check if duration longer than 60 days
            if (createNewLeave.holidayDuration > 60)
            {
                ModelState.AddModelError("holidayDuration", "Duration too long, Should not be longer than 60 days");
            }

            //add the new leave type into the database 
            if (ModelState.IsValid)
            {
                connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                string insertString = "Insert Into dbo.Leave_Type (Leave_Name,Duration) VALUES ('" + createNewLeave.newLeaveType + "','" + createNewLeave.holidayDuration + "')";
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(insertString, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    connection.Close();
                }
                Response.Write("<script> alert ('Successfully added a new department')</script>");
            }
            return Index();
        }
    }
}