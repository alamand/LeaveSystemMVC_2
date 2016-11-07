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
        private readonly List<minStaff> model = new List<minStaff>
        {
            new minStaff {empID = 32160627, empName = "Hamza Rahimy"},
            new minStaff {empID = 32060627, empName = "Not Hamza Rahimy"}
        };

        // GET: lmSubordinateViewHistory
        public ActionResult Index()
        {
            
            /*
            subordinateListModel model = new subordinateListModel();
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
                        minEmployee tempEmp = new minEmployee();
                        tempEmp.empID = (int)reader[0];
                        tempEmp.empName = (string)reader[1] + " " + (string)reader[2];
                        model.employeeList.Add(tempEmp);
                        iter++;
                    }
                }
            }
            */

            return View(model);
        }
        
        public PartialViewResult SelectEmployee(int id)
        {
            var tempList = new List<miniLeaveListModel>();
            var temp = new miniLeaveListModel();
            if(id == 32160627)
                temp.displayText = "Hamza Rahimy";
            else
                temp.displayText = "NOT Hamza Rahimy";
            tempList.Add(temp);
            return PartialView("_MinLeaveList", tempList);
        }
        
    }
}