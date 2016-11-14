using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

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
            var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
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
            else if (string.IsNullOrEmpty(calender.startDate.ToString()))
            {
                ModelState.AddModelError("startDate", "Start Date Must not be left empty");
            }
            else if (calender.startDate.Equals(DateTime.Today))
            {
                ModelState.AddModelError("startDate", "The Start Date cannot be today");
            }
            else if (calender.endDate.Equals(DateTime.Today))
            {
                ModelState.AddModelError("endDate", "The End Date cannot be today");
            }
            if (ModelState.IsValid)

            {
                if (!isDateSame(calender.startDate, calender.endDate))
                {
                    string date = calender.startDate.ToString("yyyy-MM-dd");
                    var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
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
                        }
                    }
                    ModelState.AddModelError("sucessMessage", "Success! The Holidays Have been Successfully Added");
                }

                else { ModelState.AddModelError("errorMessage", "Error!Holiday already Exists."); }            
            
                // return RedirectToAction("Display");
            }
            return CreateHoliday();
    }
    public ActionResult Display()
    {
        var model = new List<Models.hrHolidaysCalender>();
        var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
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
            var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
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
            var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
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
        /*public ActionResult Index([Bind(Exclude = "description")] Models.hrHolidaysCalender calender)
        {
            return View();
        }*/
    } 
        
    
}