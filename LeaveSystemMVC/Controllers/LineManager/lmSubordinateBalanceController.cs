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
        public ActionResult Index(string filterSearch = "", string filterOrderBy = "")
        {
            var model = new List<Tuple<sEmployeeModel, sleaveBalanceModel>>();

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = GetFilteredQuery(filterSearch, filterOrderBy); 

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

            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["EnteredSearch"] = filterSearch;
            ViewData["ReligionList"] = DBReligionList();

            return View(model);
        }

        [HttpPost]
        public ActionResult Filter(FormCollection form)
        {
            string search = form["enteredSearch"];
            string orderBy = form["selectedOrderBy"];
            return RedirectToAction("Index", new { filterSearch = search, filterOrderBy = orderBy });
        }

        private string GetFilteredQuery(string search, string order)
        {
            string queryString = "SELECT Employee.Employee_ID FROM dbo.Employee, dbo.Reporting WHERE Employee.Employee_ID = Reporting.Employee_ID AND Reporting_ID = " + GetLoggedInID() + " AND Start_Date <= SYSDATETIME() AND (End_Date > SYSDATETIME() OR End_Date IS NULL)";

            // adds a filter query if search box contains character(s), note that 0 length means the search box is empty
            if (search.Length > 0)
            {
                queryString += " AND (Employee.Employee_ID LIKE '%" + search + "%' " +
                    "OR First_Name LIKE '%" + search + "%' " +
                    "OR Last_Name LIKE '%" + search + "%')";
            }

            if (order.Length > 0)
            {
                queryString += " ORDER BY " + order;
            }

            return queryString;
        }

        private Dictionary<string, string> OrderByList()
        {
            var orderByList = new Dictionary<string, string>
            {
                { "Employee.First_Name ASC", "First Name | Ascending" },
                { "Employee.First_Name DESC", "First Name | Descending" },
                { "Employee.Last_Name ASC", "Last Name | Ascending" },
                { "Employee.Last_Name DESC", "Last Name | Descending" },
                { "Employee.Employee_ID ASC", "Employee ID | Ascending" },
                { "Employee.Employee_ID DESC", "Employee ID | Descending" }
            };
            return orderByList;
        }


    }
}