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

        public ActionResult Index()
        {
            var calender = new LeaveSystemMVC.Models.hrHolidaysCalender();
            return RedirectToAction("Display");
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
                ModelState.AddModelError("holidayName", "Holiday Name Must be entered");
            }
            if (calender.holidayName.Length > 30)
            {
                ModelState.AddModelError("holidayName", "Holiday Name Too long. The Name should be no greater than 30 char");
            }
            else if (string.IsNullOrEmpty(calender.startDate.ToString()))
            {
                ModelState.AddModelError("startDate", "Start Date Must not be left empty");
            }

            else if (calender.startDate.Equals(DateTime.Today) || calender.startDate < DateTime.Today)
            {
                ModelState.AddModelError("startDate", "The Start Date cannot be the current or previous day.");
            }
            else if (calender.endDate.Equals(DateTime.Today) || calender.startDate < DateTime.Today)
            {
                ModelState.AddModelError("endDate", "The End Date cannot be the current or previous days");
            }

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
                            string insert = "INSERT INTO dbo.Public_Holiday (Name, Date) VALUES('" + calender.holidayName + "','" + date1 + "')";
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
                    Response.Write("<script> alert('Success!');location.href='CreateHoliday'</script>");
                }

                else { ModelState.AddModelError("errorMessage", "Error!Holiday already Exists."); }

                // return RedirectToAction("Display");
            }
            return CreateHoliday();
        }
        public ActionResult Display()
        {
            var model = new List<Models.hrHolidaysCalender>();
            //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
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

            return View(model);
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
            string queryString = "Select * from dbo.Leave where Status!='4' And year(Start_Date)='" + date.Year + "'";
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
                        status = (int)reader["Status"];
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
        public int getBalance(int eid, int lid)
        {
            int balance = 0;
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
                        balance = (int)reader["Balance"];
                    }
                }
                connection.Close();
            }
            return balance;
        }
    }


}