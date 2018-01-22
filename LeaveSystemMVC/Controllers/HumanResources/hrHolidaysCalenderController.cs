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
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrHolidaysCalenderController : ControllerBase
    {
        // GET: hrHolidaysCalender
        public ActionResult Index(int? filterYear = null)
        {
            // if the filter is not selected (commonly when the page is loaded for the first time), display the current year.
            filterYear = (filterYear == null) ? DateTime.Now.Year : filterYear;

            var model = GetHolidayList(filterYear);

            // to be passed between methods
            TempData["filterYear"] = filterYear;

            // dropdown list for filtering
            ViewData["YearList"] = YearList();

            //user selected year
            ViewData["SelectedYear"] = filterYear;

            ViewBag.SuccessMessage = TempData["SuccessMessage"];

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
            return View();
        }

        [HttpPost]
        public ActionResult CreateHoliday(hrHolidaysCalender holiday)
        {
            if (string.IsNullOrWhiteSpace(holiday.holidayName))
            {
                ModelState.AddModelError("holidayName", "A holiday name must be entered.");
            }
            if (holiday.holidayName.Length > 30)
            {
                ModelState.AddModelError("holidayName", "Holiday name is too long.");
            }
            else if (string.IsNullOrEmpty(holiday.date.ToString()))
            {
                ModelState.AddModelError("date", "Holiday date must be selected.");
            }

            if (ModelState.IsValid)
            {
                if (!IsPublicHoliday(holiday.date))
                {
                    string queryString = "INSERT INTO dbo.Public_Holiday (Name, Date) VALUES('" + holiday.holidayName + "','" + holiday.date.ToString("yyyy-MM-dd") + "')";
                    DBExecuteQuery(queryString);

                    // rebalance all employees with approved applications, 1 to indicate that a holiday is added
                    RebalanceCredit(holiday.date, 1);

                    TempData["SuccessMessage"] = "<b>" + holiday.holidayName + "</b> public holiday has been created successfully.<br/>";
                }
                else
                {
                    ViewBag.ErrorMessage = "<b>" + holiday.date.ToString("yyyy-MM-dd") + "</b> is already a public holiday.";
                }
            }

            // if the holiday is created, then navigate to the holiday calendar page with the created holiday year as filter.
            if (TempData["SuccessMessage"] != null)
                return RedirectToAction("Index", new { filterYear = holiday.date.Year });
            else
                return View(holiday);
        }

        public ActionResult Remove(int holidayID)
        {
            var holiday = GetHoliday(holidayID);
            string queryString = "DELETE FROM dbo.Public_Holiday WHERE Public_Holiday_ID = " + holidayID;
            DBExecuteQuery(queryString);
            
            // rebalance all employees with approved applications, -1 to indicate that a holiday is removed
            RebalanceCredit(holiday.date, -1);

            TempData["SuccessMessage"] = "<b>" + holiday.holidayName + "</b> public holiday has been removed successfully.<br/>";

            if (GetHolidayList(holiday.date.Year).Count > 0)
                return RedirectToAction("Index", new { filterYear = TempData["filterYear"] });
            else 
                return RedirectToAction("Index", new { filterYear = DateTime.Now.Year }); 
        }

        private bool IsWeekend(DateTime date)
        {
            bool isWeekend = false;

            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Friday)
                isWeekend = true;

            return isWeekend;
        }

        private bool IsPublicHoliday(DateTime date)
        {
            bool isPublicHoliday = false;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT * FROM dbo.Public_Holiday WHERE Date = '" + date.ToString("yyyy-MM-dd") + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime day = (DateTime)reader["Date"];
                        isPublicHoliday = date.Equals(day) ? true : false;
                    }
                }
                connection.Close();
            }

            return isPublicHoliday;
        }

        private hrHolidaysCalender GetHoliday(int holidayID)
        {
            var holiday = new hrHolidaysCalender();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT * FROM dbo.Public_Holiday WHERE Public_Holiday_ID = " + holidayID;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        holiday.holidayID = (int)reader["Public_Holiday_ID"];
                        holiday.holidayName = (string)reader["Name"];
                        holiday.date = (DateTime)reader["Date"];
                    }
                }
                connection.Close();
            }

            return holiday;
        }

        private List<hrHolidaysCalender> GetHolidayList(int? filterYear = null)
        {
            var holidayList = new List<hrHolidaysCalender>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            string queryString = "SELECT * FROM dbo.Public_Holiday WHERE year(Date) = " + filterYear;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var holiday = new hrHolidaysCalender
                        {
                            holidayID = (int)reader["Public_Holiday_ID"],
                            holidayName = (string)reader["Name"],
                            date = (DateTime)reader["Date"]
                        };
                        holidayList.Add(holiday);
                    }
                }
                connection.Close();
            }
            return holidayList;
        }

        private void RebalanceCredit(DateTime holiday, int addRemove)
        {
            if (!IsWeekend(holiday))
            {
                // extract all approved applications
                foreach (var leave in GetLeaveModel())
                {
                    if (leave.leaveStatusName.Equals("Approved") && !leave.leaveTypeName.Equals("Maternity"))
                    {
                        if (leave.leaveTypeName.Equals("Short_Hours_Per_Month"))
                        {
                            if (leave.startDate == holiday)
                            {
                                TimeSpan span = leave.shortEndTime - leave.shortStartTime;

                                // note that short leaves does not have a duration in days, only in time
                                // if we are adding a holiday (credit=positive), then we will give back the employee his/her consumed balance
                                // else if we are removing a holiday (credit=negative), then we are subtracting balance from the employee
                                DBUpdateLeave(leave.leaveAppID, 0);
                                DBUpdateBalance(leave, addRemove * (decimal)span.TotalHours);
                            }
                        }
                        else
                        {
                            // starting leave date
                            DateTime date = leave.startDate;
                            do
                            {
                                if (date == holiday)
                                {
                                    // note that here we are reducing the total days of leave by 1 and adding balance to the employee by 1, and vice versa
                                    // if credit is positive, then we have added a holiday, which means subtract total leave days and add balance
                                    // if credit is negative, then we have removed a holiday, which means add total leave days and subtract balance
                                    DBUpdateLeave(leave.leaveAppID, -addRemove);
                                    DBUpdateBalance(leave, addRemove);
                                }
                                // move to the next date
                                date = date.AddDays(1);
                                // loop back if the date did not reach the returning date
                            } while (date < leave.returnDate);
                        }
                    }
                }
            }
        }

        private void DBUpdateLeave(int appID, int duration)
        {
            string queryString = "UPDATE dbo.Leave SET Total_Leave = Total_Leave+'"+ duration + "' WHERE Leave_Application_ID = '" + appID + "'";
            DBExecuteQuery(queryString);
        }

        private void DBUpdateBalance(sLeaveModel leave, decimal balance)
        {
            string queryString = "UPDATE dbo.Leave_Balance SET Balance = Balance+'"+ balance + "' WHERE Employee_ID = '" + leave.employeeID + "' AND Leave_ID = '" + leave.leaveTypeID + "'";
            DBExecuteQuery(queryString);
        }
    }
}
