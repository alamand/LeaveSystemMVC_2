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
    public class lmSubordinateViewHistoryController : Controller
    {
        private List<minStaff> staffMembers = new List<minStaff>();

        private List<miniLeaveModel> applications = new List<miniLeaveModel>();

        // GET: lmSubordinateViewHistory
        public ActionResult Index()
        {
            
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Employee_ID, First_Name, Last_Name FROM dbo.Employee";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    int iter = 0;
                    while (reader.Read())
                    {
                        minStaff newEmployee = new minStaff();
                        newEmployee.empID = (int)reader[0];
                        newEmployee.empName = (string)reader[1] + " " + (string)reader[2];
                        staffMembers.Add(newEmployee);
                        iter++;
                    }
                }
            }

            return View(staffMembers);
        }
        
        /*Action will return the list of leave applications 
         corresponding with the selected employeeID to 
         a partial view that will then be displayed on
         the main page.*/
         //
        public PartialViewResult SelectEmployee(int id)
        {
            miniLeaveModel tempLeave = new miniLeaveModel
            {
                displayText = "Annual Leave: 28 Jun - 15 July"
            };
            applications.Add(tempLeave);
            return PartialView("_MinLeaveList", applications);
        }
        
        /*Action will return the details of the leave application
         with the corresponding leaveID provided in the parameter
         and pass it to the partial view to be displayed on the main
         page.*/
         //
        public ActionResult SelectLeave(int id)
        {
            return View(this);
        }
        
    }
}