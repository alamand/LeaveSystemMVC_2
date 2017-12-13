using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Collections;
using System.Data;

namespace LeaveSystemMVC.Controllers
{
    public class hrHolidaysCalenderController : Controller
    {
        // GET: hrHolidaysCalender
        public ActionResult Index(int? filterYear = null)
        {
            var model = new List<Models.hrHolidaysCalender>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            if (filterYear == null)
            {
                filterYear = DateTime.Now.Year;
            }

            string queryString = "SELECT * FROM dbo.Public_Holiday WHERE year(Date) = " + filterYear;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var calender = new Models.hrHolidaysCalender();
                        calender.holidayName = (string)reader["Name"];
                        calender.startDate = (DateTime)reader["Date"];
                        model.Add(calender);
                    }
                }

                connection.Close();
            }

            ViewData["YearList"] = YearList();
            ViewData["SelectedYear"] = filterYear;

            return View(model);
        }

        [HttpPost]
        public ActionResult FilterListByYear(FormCollection form)
        {
            int year = Convert.ToInt32(form["selectedYear"]);
            return RedirectToAction("Index", new { filterYear = year });
        }

        public Dictionary<int, string> YearList()
        {
            Dictionary<int, string> years = new Dictionary<int, string>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT DISTINCT year(Date) as Year FROM dbo.Public_Holiday";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        years.Add((int)reader["Year"], reader["Year"].ToString());
                    }
                }
                connection.Close();
            }
            
            return years;
        }

        [HttpGet]
        public ActionResult CreateHoliday()
        {
            var model = new List<Models.hrHolidaysCalender>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            string queryString = "Select * FROM dbo.Public_Holiday";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var calender = new Models.hrHolidaysCalender();
                        calender.holidayName = (string)reader["Name"];
                        calender.startDate = (DateTime)reader["Date"];
                        model.Add(calender);
                    }
                }

                connection.Close();
            }

            return View();
        }

        [HttpPost]
        public ActionResult CreateHoliday([Bind(Exclude = "description")] Models.hrHolidaysCalender calender)
        {
            if (string.IsNullOrWhiteSpace(calender.holidayName))
            {
                ModelState.AddModelError("holidayName", "A holiday name must be entered.");
            }
            if (!string.IsNullOrWhiteSpace(calender.holidayName) && calender.holidayName.Length > 30)
            {
                ModelState.AddModelError("holidayName", "The holiday name is too long. The name should be no greater than 30 characters.");
            }
            else if (string.IsNullOrEmpty(calender.startDate.ToString()))
            {
                ModelState.AddModelError("startDate", "The start date must not be left empty.");
            }

            //Should be able to add holidays in the past e.g. when the announcement is made late.
            /*else if (calender.startDate.Equals(DateTime.Today) || calender.startDate < DateTime.Today)
            {
                ModelState.AddModelError("startDate", "The Start Date cannot be the current or previous day.");
            }
            else if (calender.endDate.Equals(DateTime.Today) || calender.startDate < DateTime.Today)
            {
                ModelState.AddModelError("endDate", "The End Date cannot be the current or previous days");
            }*/

            if (ModelState.IsValid)

            {
                if (!isDateSame(calender.startDate, calender.endDate))
                {
                    string date = calender.startDate.ToString("yyyy-MM-dd");
                    var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;

                    int totalDay = totalDays(calender.startDate, calender.endDate);
                    for (int i = 0; i <= totalDay; i++)
                    {
                        DateTime d = calender.startDate.AddDays(i);
                        string date1 = d.ToString("yyyy-MM-dd");
                        if (!isDateSame(d))
                        {
                            date1 = d.ToString("yyyy-MM-dd");
                            string insert = "INSERT INTO dbo.Public_Holiday (Name, Date) VALUES('" + GetSqlString(calender.holidayName) + "','" + date1 + "')";
                            using (var connection = new SqlConnection(connectionString))
                            {
                                var command = new SqlCommand(insert, connection);
                                connection.Open();
                                using (var reader = command.ExecuteReader())
                                    connection.Close();
                            }
                            AddCredit(d);
                        }

                    }
                    Response.Write("<script> alert('Success!');</script>");
                }

                else { ModelState.AddModelError("errorMessage", "This holiday already exists."); }

                // return RedirectToAction("Display");
            }
            return CreateHoliday();
        }

        public bool isDateSame(DateTime date)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            string queryString = "Select * FROM dbo.Public_Holiday where Date= '" + date.ToString("yyyy-MM-dd") + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                try
                {
                    int dateCount = (int)command.ExecuteScalar();
                    if (dateCount > 0)
                    { return true; }
                }
                catch { return false; }
                connection.Close();
            }

            return false;
        }

        public bool isDateSame(DateTime start, DateTime end)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            int totalDay = totalDays(start, end);
            for (int i = 0; i <= totalDay; i++)
            {
                DateTime d = start.AddDays(i);
                string date1 = d.ToString("yyyy-MM-dd");
                if (isDateSame(d))
                {
                    return true;
                }
            }
            return false;
        }

        public int totalDays(DateTime start, DateTime end)
        {
            TimeSpan ts = start - end;
            return Math.Abs(ts.Days);
        }

        public bool AddCredit(DateTime date)
        {
            int empid, appid, lid, status;
            var details = new List<Emp>();
            DateTime start, end;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select * from dbo.Leave where Leave_Status_ID!='4' And year(Start_Date)='" + date.Year + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                if (connection.State != ConnectionState.Open)
                {
                    connection.Close();
                    connection.Open();
                }
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        appid = (int)reader["Leave_Application_ID"];
                        lid = (int)reader["Leave_ID"];
                        empid = (int)reader["Employee_ID"];
                        start = (DateTime)reader["Start_Date"];
                        end = (DateTime)reader["End_Date"];
                        status = (int)reader["Leave_Status_ID"];
                        details.Add(new Emp(appid, empid, lid, start, end, status));
                    }
                }
                connection.Close();
                foreach (var item in details)
                {
                    if (isDateCheck(date, item.start, item.end) && (item.status == 0 || item.status == 1))
                    {
                        if (date.DayOfWeek != DayOfWeek.Friday && date.DayOfWeek != DayOfWeek.Saturday) { AddHolidayBalance(item.app_id, item.emp_id, item.leave_id, 1, date); }
                    }
                    else if (isDateCheck(date, item.start, item.end) && (item.status == 2))
                    {
                        if (date.DayOfWeek != DayOfWeek.Friday && date.DayOfWeek != DayOfWeek.Saturday) { AddHolidayBalance(item.app_id, item.emp_id, item.leave_id, 0, date); }
                    }
                }
            }
            return false;
        }

        public struct Emp
        {
            public Emp(int apid, int eid, int lid, DateTime strt, DateTime endDate, int stat) { app_id = apid; leave_id = lid; emp_id = eid; start = strt; end = endDate; status = stat; }
            public int app_id { get; set; }
            public int status { get; set; }
            public int emp_id { get; set; }
            public int leave_id { set; get; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
        }

        public bool isDateCheck(DateTime a, DateTime b_start, DateTime b_end)
        {
            int total = totalDays(b_start, b_end);
            for (int i = 0; i <= total; i++)
            {
                if (a.CompareTo(b_start.AddDays(i)) == 0) { return true; }
            }//else return false;
            return false;
        }

        public void AddHolidayBalance(int appid, int eid, int lid, int bal, DateTime sdate)
        {
            //bal += getBalance(eid, lid);
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Update dbo.Leave_Balance SET Balance= Balance + " + bal + " WHERE Employee_ID= '" + eid + "' And Leave_ID= '" + lid + "' ";
            using (var connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Close();
                    connection.Open();
                }
                var command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }
            if (sdate.DayOfWeek != DayOfWeek.Friday && sdate.DayOfWeek != DayOfWeek.Saturday)
            { DecrementTotalDays(appid, 1, eid, lid); }

        }

        public void DecrementTotalDays(int apid, int duration, int eid, int lid)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Update dbo.Leave SET Total_Leave_Days= Total_Leave_Days -" + duration + " WHERE Leave_Application_ID =" + apid + "";
            using (var connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Close();
                    connection.Open();
                }
                var command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }
        }

        public decimal getBalance(int eid, int lid)
        {
            decimal balance = 0;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select * from dbo.Leave_Balance Where Employee_ID='" + eid + "' And Leave_ID='" + lid + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Close();
                    connection.Open();
                }
                var command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        balance = (decimal)reader["Balance"];
                    }
                }
                connection.Close();
            }
            return balance;
        }

        public string GetSqlString(string fieldvalue)
        {
            if (fieldvalue != null)
                return fieldvalue.Replace("'", "''");
            return fieldvalue;
        }
    }


}
