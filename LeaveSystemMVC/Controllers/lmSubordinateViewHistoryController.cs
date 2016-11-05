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
        // GET: lmSubordinateViewHistory
        public ActionResult Index(subordinateListModel model)
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
            minEmployee tempEmp = new minEmployee();
            tempEmp.empID = 32160627;
            tempEmp.empName = "hamza";

            return View(/*model*/);
        }
        
    }
}