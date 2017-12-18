using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;


namespace LeaveSystemMVC.Controllers
{
    public class lmSubordinateBalanceController : ControllerBase
    {
        // GET: lmSubordinateBalance
        public ActionResult Index()
        {
            var model = new List<Tuple<sEmployeeModel, sleaveBalanceModel>>();

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Employee_ID FROM dbo.Employee WHERE Reporting_ID = " + GetLoggedInID();

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);      // retrieve employee id, first and last name
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // iterate through all employees in the database and add them all to the list
                    while (reader.Read())
                    {
                        var employee = GetEmployeeModel((int)reader["Employee_ID"]);
                        var leaveBalance = GetLeaveBalanceModel((int)reader["Employee_ID"]);
                        model.Add(new Tuple<sEmployeeModel, sleaveBalanceModel>(employee, leaveBalance));
                    }
                }
                connection.Close();
            }

            ViewData["ReligionList"] = DBReligionList();

            return View(model);
        }
    }
}