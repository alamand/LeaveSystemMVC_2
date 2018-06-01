using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrHolidaysCalendarController : BaseController
    {
        // GET: hrHolidaysCalendar
        public ActionResult Index(int? filterYear = null)
        {
            // if the filter is not selected (commonly when the page is loaded for the first time), display the current year.
            filterYear = (filterYear == null) ? DateTime.Now.Year : filterYear;

            var yearList = YearList();

            List<Holiday> model = GetHolidayList(filterYear);
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


            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT DISTINCT year(Date) as Year FROM dbo.Public_Holiday";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                years.Add((int)row["Year"], row["Year"].ToString());
            }

            return years;
        }

        [HttpGet]
        public ActionResult CreateHoliday()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateHoliday(Holiday holiday)
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
                        DataBase db = new DataBase();

                        SqlCommand cmd = new SqlCommand();
                        cmd.Parameters.Add("@name", SqlDbType.NChar).Value = holiday.holidayName;
                        cmd.Parameters.Add("@date", SqlDbType.NChar).Value = holiday.date.ToString("yyyy-MM-dd");
                        cmd.CommandText = "INSERT INTO dbo.Public_Holiday (Name, Date) VALUES(@name, @date)";
                        db.Execute(cmd);

                        // rebalance all employees with approved applications, 1 to indicate that a holiday is added
                        RebalanceCredit(holiday.date, 1);
                        AddPublicHolidayAudit(holiday.date);

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
                DataBase db = new DataBase();
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.Add("@holidayID", SqlDbType.NChar).Value = holidayID;
                cmd.CommandText = "DELETE FROM dbo.Public_Holiday WHERE Public_Holiday_ID = @holidayID";
                db.Execute(cmd);

                // rebalance all employees with approved applications, -1 to indicate that a holiday is removed
                RebalanceCredit(holiday.date, -1);
                RemovePublicHolidayAudit(holiday.date);

                TempData["SuccessMessage"] = "<b>" + holiday.holidayName + "</b> public holiday has been removed successfully.<br/>";

                if (GetHolidayList(holiday.date.Year).Count > 0)
                    return RedirectToAction("Index", new { filterYear = TempData["filterYear"] });
                else
                    return RedirectToAction("Index", new { filterYear = DateTime.Now.Year });
            }
        }

        private void AddPublicHolidayAudit(DateTime date)
        {
            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@holidayID", SqlDbType.Int).Value = GetPublicHolidayLastIdentity();
            cmd.Parameters.Add("@date", SqlDbType.NChar).Value = date.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@createdBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@createdOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Audit_Public_Holiday (Public_Holiday_ID, Column_Name, Value_After, Created_By, Created_On) " +
                "VALUES(@holidayID, 'Date', @date, @createdBy, @createdOn)";
            db.Execute(cmd);
        }

        private void RemovePublicHolidayAudit(DateTime date)
        {
            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@holidayID", SqlDbType.Int).Value = GetPublicHolidayLastIdentity();
            cmd.Parameters.Add("@date", SqlDbType.NChar).Value = date.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@modifiedBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@modifiedOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");

            cmd.CommandText = "INSERT INTO dbo.Audit_Public_Holiday (Public_Holiday_ID, Column_Name, Value_Before, Modified_By, Modified_On) " +
                "VALUES(@holidayID, 'Date', @date, @modifiedBy, @modifiedOn)";
            db.Execute(cmd);
        }

        private Holiday GetHoliday(int holidayID)
        {
            var holiday = new Holiday();
            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@holidayID", SqlDbType.NChar).Value = holidayID;
            cmd.CommandText = "SELECT * FROM dbo.Public_Holiday WHERE Public_Holiday_ID = @holidayID";

            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                holiday.holidayID = (int)row["Public_Holiday_ID"];
                holiday.holidayName = (string)row["Name"];
                holiday.date = (DateTime)row["Date"];
            }

            return holiday;
        }

        private List<Holiday> GetHolidayList(int? filterYear = null)
        {
            var holidayList = new List<Holiday>();

            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            filterYear = (filterYear == null) ? 0 : filterYear;
            cmd.Parameters.Add("@filterYear", SqlDbType.Int).Value = filterYear;
            cmd.CommandText = "SELECT * FROM dbo.Public_Holiday WHERE year(Date) = @filterYear";
            db.Execute(cmd);

            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                var holiday = new Holiday
                {
                    holidayID = (int)row["Public_Holiday_ID"],
                    holidayName = (string)row["Name"],
                    date = (DateTime)row["Date"]
                };
                holidayList.Add(holiday);

            }

            return holidayList;
        }

        private void RebalanceCredit(DateTime holiday, int addRemove)
        {
            if (holiday.DayOfWeek != DayOfWeek.Saturday && holiday.DayOfWeek != DayOfWeek.Friday)
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
                        Balance leaveType = GetLeaveBalanceModel();

                        if (leave.leaveTypeName.Equals("Short_Hours"))
                        {
                            if (leave.startDate == holiday)
                            {
                                TimeSpan span = leave.shortEndTime - leave.shortStartTime;
                                // Public holiday added
                                if (addRemove == 1)
                                {
                                    UpdateBalance(leave.employeeID, leaveType.shortHoursID, audit[leaveType.shortHoursID].Item3);
                                    UpdateAuditBalance(audit[leaveType.shortHoursID].Item1, leave.leaveAppID, audit[leaveType.shortHoursID].Item2, (audit[leaveType.shortHoursID].Item2 + audit[leaveType.shortHoursID].Item3), "Added Public Holiday");
                                }
                                else
                                {
                                    UpdateBalance(leave.employeeID, leaveType.shortHoursID, -audit[leaveType.shortHoursID].Item3);
                                    UpdateAuditBalance(audit[leaveType.shortHoursID].Item1, leave.leaveAppID, audit[leaveType.shortHoursID].Item2, (audit[leaveType.shortHoursID].Item2 - audit[leaveType.shortHoursID].Item3), "Removed Public Holiday");
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

                                    Dictionary dic = new Dictionary();
                                    Dictionary<int, string> leaveTypeList = dic.GetLeaveType();
                                    bool incremental = leaveTypeList[leaveTypeID].Equals("Unpaid") || leaveTypeList[leaveTypeID].Equals("Compassionate") ? true : false;

                                    // note that here we are reducing the total days of leave by 1 and adding balance to the employee by 1, and vice versa
                                    // if credit is positive, then we have added a holiday, which means subtract total leave days and add balance
                                    // if credit is negative, then we have removed a holiday, which means add total leave days and subtract balance
                                    UpdateLeave(leave.leaveAppID, -addRemove);
                                    UpdateBalance(leave.employeeID, leaveTypeID, (incremental) ? -addRemove : addRemove);
                                    UpdateAuditBalance(audit[leaveTypeID].Item1, leave.leaveAppID, audit[leaveTypeID].Item2, audit[leaveTypeID].Item2 + ((incremental) ? -addRemove : addRemove), (addRemove == 1) ? "Added Public Holiday" : "Removed Public Holiday");
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

        private int GetRebalancingLeaveTypeForAnnual(Leave leave, Dictionary<int, Tuple<int, decimal, decimal, decimal>> audit, Balance leaveType, int addRemove)
        {
            int leaveTypeID = 0;

            foreach (KeyValuePair<int, Tuple<int, decimal, decimal, decimal>> pair in audit)
            {
                int key = pair.Key;
                int balID = pair.Value.Item1;
                decimal currBal = pair.Value.Item2;
                decimal consumed = pair.Value.Item3;
                decimal ph = pair.Value.Item4;
                Debug.WriteLine("Leave Type: " + key + " | Balance ID: " + balID + " | Current Balance: " + currBal + " | Consumed: " + consumed + " | PH: " + ph);
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

        private int GetRebalancingLeaveTypeForSick(Leave leave, Dictionary<int, Tuple<int, decimal, decimal, decimal>> audit, Balance leaveType, int addRemove)
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

        private int GetRebalancingLeaveTypeForCompassionate(Leave leave, Dictionary<int, Tuple<int, decimal, decimal, decimal>> audit, Balance leaveType, int addRemove)
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

        private Dictionary<int, Tuple<int, decimal, decimal, decimal>> DBGetAuditLeave(int appID)
        {
            Dictionary<int, Tuple<int, decimal, decimal, decimal>> auditLeave = new Dictionary<int, Tuple<int, decimal, decimal, decimal>>();

            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
            cmd.CommandText = "SELECT Leave_Balance.Leave_Balance_ID, Leave_Type_ID, Balance, Value_Before, Value_After, Comment FROM dbo.Leave_Balance, dbo.Audit_Leave_Balance " +
                "WHERE Leave_Balance.Leave_Balance_ID = Audit_Leave_Balance.Leave_Balance_ID AND Leave_Application_ID = @appID";

            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                int balID = (int)row["Leave_Balance_ID"];
                int leaveType = (int)row["Leave_Type_ID"];
                decimal currentBalance = (decimal)row["Balance"];
                decimal valBefore = (!DBNull.Value.Equals(row["Value_Before"])) ? decimal.Parse((string)row["Value_Before"]) : (decimal)0.0;
                decimal valAfter = decimal.Parse((string)row["Value_After"]);
                string comment = (string)row["Comment"];

                if (comment.Equals("Approved Leave Application"))
                    auditLeave.Add(leaveType, new Tuple<int, decimal, decimal, decimal>(balID, currentBalance, valBefore - valAfter, 0));
                else if (comment.Equals("Added Public Holiday") || comment.Equals("Removed Public Holiday"))
                    auditLeave[leaveType] = new Tuple<int, decimal, decimal, decimal>(auditLeave[leaveType].Item1, auditLeave[leaveType].Item2, auditLeave[leaveType].Item3, auditLeave[leaveType].Item4 + valBefore - valAfter);
            }

            return auditLeave;
        }

        private void UpdateLeave(int appID, int duration)
        {
            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
            cmd.Parameters.Add("@duration", SqlDbType.Int).Value = duration;
            cmd.CommandText = "UPDATE dbo.Leave SET Total_Leave = Total_Leave+@duration WHERE Leave_Application_ID = @appID";
            db.Execute(cmd);
        }

        private void UpdateBalance(int empID, int typeID, decimal balance)
        {
            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = balance;
            cmd.CommandText = "UPDATE dbo.Leave_Balance SET Balance = Balance+@balance WHERE Employee_ID = @empID AND Leave_Type_ID = @typeID";
            db.Execute(cmd);
        }

        private void UpdateAuditBalance(int balanceID, int appID, decimal valueBefore, decimal valueAfter, string comment)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@balanceID", SqlDbType.Int).Value = balanceID;
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
            cmd.Parameters.Add("@valueBefore", SqlDbType.Decimal).Value = valueBefore;
            cmd.Parameters.Add("@valueAfter", SqlDbType.Decimal).Value = valueAfter;
            cmd.Parameters.Add("@modifiedBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@modifiedOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";
            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                  "VALUES(@balanceID, @appID, 'Balance', @valueBefore, @valueAfter, @modifiedBy, @modifiedOn, @comment)";

            DataBase db = new DataBase();
            db.Execute(cmd);
        }

        private int GetPublicHolidayLastIdentity()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT MAX(Public_Holiday_ID) AS LastID FROM dbo.Public_Holiday";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);
            int id = (int)dataTable.Rows[0][0];
            return id;
        }
    }
}
