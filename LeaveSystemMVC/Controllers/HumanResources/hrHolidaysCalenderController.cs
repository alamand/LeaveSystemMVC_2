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

            var yearList = YearList();

            List<hrHolidaysCalender> model = GetHolidayList(filterYear);
            if (model.Count == 0 && yearList.Count > 0)
            {
                model = GetHolidayList(yearList.Keys.First());
                filterYear = yearList.Keys.First();
            }

            // to be passed between methods
            TempData["filterYear"] = filterYear;

            // dropdown list for filtering
            ViewData["YearList"] = yearList;

            //user selected year
            ViewData["SelectedYear"] = filterYear;

            SetMessageViewBags();
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
            if (holiday.holidayName.Length > 30)
            {
                ModelState.AddModelError("holidayName", "The holiday name is too long.");
            }

            if (holiday.date.Year < DateTime.Today.Year)
            {
                ModelState.AddModelError("date", "You cannot add a holiday to the previous year.");
            }

            if (ModelState.IsValid)
            {
                if (!IsPublicHoliday(holiday.date))
                {
                    if (holiday.date.Year >= DateTime.Today.Year)
                    {
                        string queryString = "INSERT INTO dbo.Public_Holiday (Name, Date) VALUES('" + holiday.holidayName + "','" + holiday.date.ToString("yyyy-MM-dd") + "')";
                        DBExecuteQuery(queryString);

                        // rebalance all employees with approved applications, 1 to indicate that a holiday is added
                        RebalanceCredit(holiday.date, 1);

                        string auditString = "INSERT INTO dbo.Audit_Public_Holiday (Public_Holiday_ID, Column_Name, Value_After, Created_By, Created_On) " +
                            "VALUES('" + DBLastIdentity("Public_Holiday_ID", "dbo.Public_Holiday") + "', 'Date' ,'" + holiday.date.ToString("yyyy-MM-dd") + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
                        DBExecuteQuery(auditString);

                        TempData["SuccessMessage"] = "<b>" + holiday.holidayName + "</b> public holiday has been created successfully.<br/>";
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "You cannot add a holiday to the previous year.";
                    }
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
            if (holiday.date.Date.CompareTo(DateTime.Now.Date) < 0) //may not remove holidays that are in the past
            {
                TempData["ErrorMessage"] = "<b>" + holiday.holidayName + "</b> cannot be removed since the date has already passed.<br/>";
                return RedirectToAction("Index", new { filterYear = DateTime.Now.Year });
            }
            else
            {
                string queryString = "DELETE FROM dbo.Public_Holiday WHERE Public_Holiday_ID = " + holidayID;
                DBExecuteQuery(queryString);

                // rebalance all employees with approved applications, -1 to indicate that a holiday is removed
                RebalanceCredit(holiday.date, -1);

                string auditString = "INSERT INTO dbo.Audit_Public_Holiday (Public_Holiday_ID, Column_Name, Value_Before, Modified_By, Modified_On) " +
                      "VALUES('" + holidayID + "', 'Date' ,'" + holiday.date.ToString("yyyy-MM-dd") + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
                DBExecuteQuery(auditString);

                TempData["SuccessMessage"] = "<b>" + holiday.holidayName + "</b> public holiday has been removed successfully.<br/>";

                if (GetHolidayList(holiday.date.Year).Count > 0)
                    return RedirectToAction("Index", new { filterYear = TempData["filterYear"] });
                else
                    return RedirectToAction("Index", new { filterYear = DateTime.Now.Year });
            }
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

            string queryString = "SELECT * FROM dbo.Public_Holiday WHERE year(Date) = " + ((filterYear == null) ? 0 : filterYear);

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
                    if (leave.leaveStatusName.Equals("Approved"))
                    {
                        // key: Leave Type
                        // Value.Item1 : Leave Balance ID
                        // Value.Item2 : Current Balance
                        // Value.Item3 : Leave Application Duration
                        // Value.Item4 : Public Holiday(s) Removed/Added
                        Dictionary<int, Tuple<int, decimal, decimal, decimal>> audit = DBGetAuditLeave(leave.leaveAppID);
                        sleaveBalanceModel leaveType = GetLeaveBalanceModel();

                        if (leave.leaveTypeName.Equals("Short_Hours"))
                        {
                            if (leave.startDate == holiday)
                            {
                                TimeSpan span = leave.shortEndTime - leave.shortStartTime;
                                // Public holiday added
                                if (addRemove == 1)
                                {
                                    DBUpdateBalance(leave.employeeID, leaveType.shortHoursID, audit[leaveType.shortHoursID].Item3);
                                    DBUpdateAuditBalance(audit[leaveType.shortHoursID].Item1, leave.leaveAppID, audit[leaveType.shortHoursID].Item2, (audit[leaveType.shortHoursID].Item2 + audit[leaveType.shortHoursID].Item3), "Added Public Holiday");
                                }
                                else
                                {
                                    DBUpdateBalance(leave.employeeID, leaveType.shortHoursID, -audit[leaveType.shortHoursID].Item3);
                                    DBUpdateAuditBalance(audit[leaveType.shortHoursID].Item1, leave.leaveAppID, audit[leaveType.shortHoursID].Item2, (audit[leaveType.shortHoursID].Item2 - audit[leaveType.shortHoursID].Item3), "Removed Public Holiday");
                                }
                            }
                        }
                        else
                        {
                            int leaveTypeID = 0;
                            DateTime date = leave.startDate;

                            do
                            {
                                if (date == holiday)
                                {
                                    switch (leave.leaveTypeName)
                                    {
                                        case "Annual":
                                            leaveTypeID = GetRebalancingLeaveTypeForAnnual(leave, audit, leaveType, addRemove);
                                            break;

                                        case "Sick":
                                            leaveTypeID = GetRebalancingLeaveTypeForSick(leave, audit, leaveType, addRemove);
                                            break;
                                            
                                        case "Compassionate":
                                            leaveTypeID = GetRebalancingLeaveTypeForCompassionate(leave, audit, leaveType, addRemove);
                                            break;

                                        case "DIL":
                                            leaveTypeID = leaveType.daysInLieuID;
                                            break;

                                        case "Pilgrimage":
                                            leaveTypeID = leaveType.pilgrimageID;
                                            break;

                                        case "Unpaid":
                                            leaveTypeID = leaveType.unpaidID;
                                            break;

                                        case "Maternity":
                                            leaveTypeID = leaveType.maternityID;
                                            break;

                                        default:
                                            break;
                                    }

                                    Dictionary<int, string> leaveTypeList = DBLeaveTypeList();
                                    Boolean incremental = leaveTypeList[leaveTypeID].Equals("Unpaid") || leaveTypeList[leaveTypeID].Equals("Compassionate") ? true : false;

                                    // note that here we are reducing the total days of leave by 1 and adding balance to the employee by 1, and vice versa
                                    // if credit is positive, then we have added a holiday, which means subtract total leave days and add balance
                                    // if credit is negative, then we have removed a holiday, which means add total leave days and subtract balance
                                    DBUpdateLeave(leave.leaveAppID, -addRemove);
                                    DBUpdateBalance(leave.employeeID, leaveTypeID, (incremental) ? -addRemove : addRemove);
                                    DBUpdateAuditBalance(audit[leaveTypeID].Item1, leave.leaveAppID, audit[leaveTypeID].Item2, audit[leaveTypeID].Item2 + ((incremental) ? -addRemove : addRemove), (addRemove == 1) ? "Added Public Holiday" : "Removed Public Holiday");
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

        private int GetRebalancingLeaveTypeForAnnual(sLeaveModel leave, Dictionary<int, Tuple<int, decimal, decimal, decimal>> audit, sleaveBalanceModel leaveType, int addRemove)
        {
            int leaveTypeID = 0;

            foreach (KeyValuePair<int, Tuple<int, decimal, decimal, decimal>> pair in audit)
            {
                int key = pair.Key;
                int balID = pair.Value.Item1;
                decimal currBal = pair.Value.Item2;
                decimal consumed = pair.Value.Item3;
                decimal ph = pair.Value.Item4;
                Output("Leave Type: " + key + " | Balance ID: " + balID + " | Current Balance: " + currBal + " | Consumed: " + consumed + " | PH: " + ph);
            }

            // Public holiday is added
            if (addRemove == 1)
            {
                if (audit.ContainsKey(leaveType.unpaidID) && audit[leaveType.unpaidID].Item3 + audit[leaveType.unpaidID].Item4 < 0)
                    leaveTypeID = leaveType.unpaidID;
                else if (audit.ContainsKey(leaveType.annualID) && audit[leaveType.annualID].Item3 + audit[leaveType.annualID].Item4 > 0)
                    leaveTypeID = leaveType.annualID;
                else
                    leaveTypeID = leaveType.daysInLieuID;
            }
            else
            {
                if (audit.ContainsKey(leaveType.daysInLieuID) && audit[leaveType.daysInLieuID].Item2 > 0)
                    leaveTypeID = leaveType.daysInLieuID;
                else if (audit.ContainsKey(leaveType.annualID) && audit[leaveType.annualID].Item2 > 0)
                    leaveTypeID = leaveType.annualID;
                else
                    leaveTypeID = leaveType.unpaidID;
            }

            return leaveTypeID;
        }

        private int GetRebalancingLeaveTypeForSick(sLeaveModel leave, Dictionary<int, Tuple<int, decimal, decimal, decimal>> audit, sleaveBalanceModel leaveType, int addRemove)
        {
            int leaveTypeID = 0;

            // Public holiday is added
            if (addRemove == 1)
            {
                if (audit.ContainsKey(leaveType.unpaidID) && audit[leaveType.unpaidID].Item3 + audit[leaveType.unpaidID].Item4 < 0)
                    leaveTypeID = leaveType.unpaidID;
                else if (audit.ContainsKey(leaveType.annualID) && audit[leaveType.annualID].Item3 + audit[leaveType.annualID].Item4 > 0)
                    leaveTypeID = leaveType.annualID;
                else if (audit.ContainsKey(leaveType.sickID) && audit[leaveType.sickID].Item3 + audit[leaveType.sickID].Item4 > 0)
                    leaveTypeID = leaveType.sickID;
                else
                    leaveTypeID = leaveType.daysInLieuID;
            }
            else
            {
                if (audit.ContainsKey(leaveType.daysInLieuID) && audit[leaveType.daysInLieuID].Item2 > 0)
                    leaveTypeID = leaveType.daysInLieuID;
                else if (audit.ContainsKey(leaveType.sickID) && audit[leaveType.sickID].Item2 > 0)
                    leaveTypeID = leaveType.sickID;
                else if (audit.ContainsKey(leaveType.annualID) && audit[leaveType.annualID].Item2 > 0)
                    leaveTypeID = leaveType.annualID;
                else
                    leaveTypeID = leaveType.unpaidID;
            }

            return leaveTypeID;
        }

        private int GetRebalancingLeaveTypeForCompassionate(sLeaveModel leave, Dictionary<int, Tuple<int, decimal, decimal, decimal>> audit, sleaveBalanceModel leaveType, int addRemove)
        {
            int leaveTypeID = 0;

            // Public holiday is added
            if (addRemove == 1)
            {
                if (audit.ContainsKey(leaveType.unpaidID) && audit[leaveType.unpaidID].Item3 + audit[leaveType.unpaidID].Item4 < 0)
                    leaveTypeID = leaveType.unpaidID;
                else if (audit.ContainsKey(leaveType.daysInLieuID) && audit[leaveType.daysInLieuID].Item3 + audit[leaveType.daysInLieuID].Item4 > 0)
                    leaveTypeID = leaveType.daysInLieuID;
                else if (audit.ContainsKey(leaveType.annualID) && audit[leaveType.annualID].Item3 + audit[leaveType.annualID].Item4 > 0)
                    leaveTypeID = leaveType.annualID;
                else
                    leaveTypeID = leaveType.compassionateID;
            }
            else
            {
                if (audit.ContainsKey(leaveType.compassionateID) && audit[leaveType.compassionateID].Item4 < leaveType.compassionate)
                    leaveTypeID = leaveType.compassionateID;
                else if (audit.ContainsKey(leaveType.annualID) && audit[leaveType.annualID].Item2 > 0)
                    leaveTypeID = leaveType.annualID;
                else if (audit.ContainsKey(leaveType.daysInLieuID) && audit[leaveType.daysInLieuID].Item2 > 0)
                    leaveTypeID = leaveType.daysInLieuID;
                else
                    leaveTypeID = leaveType.unpaidID;
            }

            return leaveTypeID;
        }

        protected Dictionary<int, Tuple<int, decimal, decimal, decimal>> DBGetAuditLeave(int appID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            var queryString = "SELECT Leave_Balance.Leave_Balance_ID, Leave_Type_ID, Balance, Value_Before, Value_After, Comment FROM dbo.Leave_Balance, dbo.Audit_Leave_Balance " +
                "WHERE Leave_Balance.Leave_Balance_ID = Audit_Leave_Balance.Leave_Balance_ID AND Leave_Application_ID = '" + appID + "'";
            Dictionary<int, Tuple<int, decimal, decimal, decimal>> auditLeave = new Dictionary<int, Tuple<int, decimal, decimal, decimal>>();

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int balID = (int)reader["Leave_Balance_ID"];
                        int leaveType = (int)reader["Leave_Type_ID"];
                        decimal currentBalance = (decimal)reader["Balance"];
                        decimal valBefore = (!DBNull.Value.Equals(reader["Value_Before"])) ? decimal.Parse((string)reader["Value_Before"]) : (decimal)0.0;
                        decimal valAfter = decimal.Parse((string)reader["Value_After"]);
                        string comment = (string)reader["Comment"];

                        if (comment.Equals("Approved Leave Application"))
                            auditLeave.Add(leaveType, new Tuple<int, decimal, decimal, decimal>(balID, currentBalance, valBefore-valAfter, 0));
                        else if (comment.Equals("Added Public Holiday") || comment.Equals("Removed Public Holiday"))
                            auditLeave[leaveType] = new Tuple<int, decimal, decimal, decimal>(auditLeave[leaveType].Item1, auditLeave[leaveType].Item2, auditLeave[leaveType].Item3 ,auditLeave[leaveType].Item4 + valBefore - valAfter);
                    }
                }
                connection.Close();
            }

            return auditLeave;
        }

        private void DBUpdateLeave(int appID, int duration)
        {
            string queryString = "UPDATE dbo.Leave SET Total_Leave = Total_Leave+'"+ duration + "' WHERE Leave_Application_ID = '" + appID + "'";
            DBExecuteQuery(queryString);
        }

        private void DBUpdateBalance(int empID, int leaveTypeID, decimal balance)
        {
            string queryString = "UPDATE dbo.Leave_Balance SET Balance = Balance+'"+ balance + "' WHERE Employee_ID = '" + empID + "' AND Leave_Type_ID = '" + leaveTypeID + "'";
            DBExecuteQuery(queryString);
        }

    }
}
