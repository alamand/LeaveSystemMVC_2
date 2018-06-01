using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class lmSubordinateBalanceController : BaseController
    {
        // GET: lmSubordinateBalance
        public ActionResult Index(string filterSearch = "", string filterOrderBy = "")
        {
            var model = GetFilteredBalances(filterSearch, filterOrderBy);
            Dictionary dic = new Dictionary();

            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["EnteredSearch"] = filterSearch;
            ViewData["ReligionList"] = dic.GetReligionName();

            return View(model);
        }

        [HttpPost]
        public ActionResult Filter(FormCollection form)
        {
            string search = form["enteredSearch"];
            string orderBy = form["selectedOrderBy"];
            return RedirectToAction("Index", new { filterSearch = search, filterOrderBy = orderBy });
        }

        private List<Tuple<Employee, Balance>> GetFilteredBalances(string search, string order)
        {
            List<Tuple<Employee, Balance>> model = new List<Tuple<Employee, Balance>>();

            int loggedInID = GetLoggedInID();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@loggedInID", SqlDbType.Int).Value = loggedInID;
            cmd.CommandText = "SELECT Employee.Employee_ID FROM dbo.Employee, dbo.Reporting WHERE Employee.Employee_ID = Reporting.Employee_ID AND (Report_To_ID = @loggedInID";

            List<Reporting> reportingList = GetReportingList(loggedInID);
            string reportToList = "";
            foreach (var reporting in reportingList)
            {
                if (reporting.toID == loggedInID)
                    reportToList += " OR Report_To_ID = " + reporting.reportToID;
            }
            cmd.Parameters.Add("@reportToList", SqlDbType.NChar).Value = reportToList;
            cmd.CommandText += reportToList + ")";
            
            // adds a filter query if search box contains character(s), note that 0 length means the search box is empty
            if (search.Length > 0)
            {
                cmd.Parameters.Add("@search", SqlDbType.NChar).Value = search;
                cmd.CommandText += " AND (Employee.Employee_ID LIKE '%' + @search + '%' " +
                    "OR First_Name LIKE '%' + @search + '%' " +
                    "OR Last_Name LIKE '%' + @search + '%')";
            }

            if (order.Length > 0)
            {
                cmd.CommandText += " ORDER BY " + order;
            }

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                var employee = GetEmployeeModel((int)row["Employee_ID"]);
                var leaveBalance = GetLeaveBalanceModel((int)row["Employee_ID"]);
                model.Add(new Tuple<Employee, Balance>(employee, leaveBalance));
            }

            return model;
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