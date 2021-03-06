﻿using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

// @TODO: More testing and optimization
namespace LeaveSystemMVC.Controllers
{
    public class LeaveApplicationController : BaseController
    {
        // GET: LeaveApplication
        [HttpGet]
        public ActionResult Index(int leaveTypeID = 0)
        {
            Employee emp = GetEmployeeModel(GetLoggedInID());
            SetViewData(emp, leaveTypeID);
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View();
        }

        [HttpPost]
        public ActionResult Select(FormCollection form)
        {
            int leaveID = Convert.ToInt32(form["selectedLeaveTypeID"]);
            return RedirectToAction("Index", new { leaveTypeID = leaveID });
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file, Leave model, FormCollection form)
        {
            Employee emp = GetEmployeeModel(GetLoggedInID());
            Balance leaveBalance = GetLeaveBalanceModel(GetLoggedInID());
            ModelState.Clear();

            // checks if the dates are the same, and if the end date is before than start date (sets ModelState to invalid if one of them is true)
            if (!model.leaveTypeName.Equals("Short_Hours"))
                CompareDates(model.startDate, model.returnDate, model);

            if (ModelState.IsValid)
            {
                // these leave types use half days
                if (model.leaveTypeName.Equals("Annual") || model.leaveTypeName.Equals("Sick") || model.leaveTypeName.Equals("Compassionate") || model.leaveTypeName.Equals("Unpaid") || model.leaveTypeName.Equals("DIL"))
                {
                    AdjustHalfDays(model);
                }

                // gets the total number of days, this involves excluding weekends and public holidays
                decimal numOfDays = GetNumOfDays(model.startDate, model.returnDate);

            if ((model.leaveTypeName.Equals("Short_Hours")) || (numOfDays > 0 && (model.shortStartTime != null || model.shortEndTime != null)))
            {
                switch (model.leaveTypeName)
                {
                    case "Annual":
                        LeaveAppAnnual(model, leaveBalance, emp, file, numOfDays);
                        break;

                    case "Sick":
                        LeaveAppSick(model, leaveBalance, emp, file, numOfDays);
                        break;

                    case "Maternity":
                        LeaveAppMaternity(model, leaveBalance, emp, file);
                        break;

                    case "Compassionate":
                        LeaveAppCompassionate(model, leaveBalance, emp, file, numOfDays);
                        break;

                    case "Short_Hours":
                        int duration = Convert.ToInt32(form["selectedDuration"]);
                        LeaveAppShortHours(model, leaveBalance, emp, duration);                        
                        break;

                    case "Pilgrimage":
                        LeaveAppPilgrimage(model, leaveBalance, emp, file, numOfDays);
                        break;

                    case "Unpaid":
                        LeaveAppUnpaid(model, emp, file, numOfDays);
                        break;

                    case "DIL":
                        LeaveAppDIL(model, emp, leaveBalance, file, numOfDays);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid application. The selected date(s) is/are weekend(s), and/or public holiday(s), and/or the leave duration is zero.";
            }

            SetViewData(emp, model.leaveTypeID);
            SetMessageViewBags();

            // if the application was submitted successfully, then display success message, else show the application page again.
            if (TempData["SuccessMessage"] != null)
                return RedirectToAction("Index");
            else
                return View(model);
            }
            else
            {
                SetViewData(emp, model.leaveTypeID);
                SetMessageViewBags();
                return View(model);
            }
        }

        private void AdjustHalfDays(Leave model)
        {
            //half a day of leave
            if (model.startDate.Equals(model.returnDate) && (model.isStartDateHalfDay == true || model.isReturnDateHalfDay == true))
            {
                model.returnDate = model.returnDate.AddDays(0.5);
            }
            else
            {
                if (model.isStartDateHalfDay == true && !IsPublicHoliday(model.startDate)) // leave starts at 12pm on startDate
                    model.startDate = model.startDate.AddDays(0.5);

                if (model.isReturnDateHalfDay == true && !IsPublicHoliday(model.returnDate)) // leave ends at 12pm on returnDate
                    model.returnDate = model.returnDate.AddDays(0.5);
            }
        }

        private void LeaveAppAnnual(Leave model, Balance lb, Employee emp, HttpPostedFileBase file, decimal numOfDays)
        {
            // keeps track of how much credit points should be deducted from each balance type
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in DIL, if yes, then simply deduct from DIL, 
            // if not, deduct all the balance from DIL and the remainder from annual balance. if annual balance 
            // is insufficient, then add the remaining number of days to unpaid balance.
            if (lb.daysInLieu < numOfDays)
            {
                deductDIL = lb.daysInLieu;
                if (lb.daysInLieu + lb.annual < numOfDays)
                {
                    deductAnnual = lb.annual;
                    addUnpaid = numOfDays - deductDIL - deductAnnual;
                }
                else
                {
                    deductAnnual = numOfDays - deductDIL;
                }
            }
            else
            {
                deductDIL = numOfDays;
            }

            if (ModelState.IsValid)
            {
                // uploads the file to App_Data/Documentation
                string fileName = UploadFile(file);

                if (TempData["ErrorMessage"] == null)
                {
                    // inserts the data to the database
                    ApplyLeave(model, numOfDays, fileName);

                    Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                    new Email().PendingLeaveApplicationByLM(emp, empLM, model);

                    // sets the notification message to be displayed to the applicant
                    TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                    TempData["SuccessMessage"] += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
                    TempData["SuccessMessage"] += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
                    TempData["SuccessMessage"] += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";
                }

            }
        }

        private void LeaveAppSick(Leave model, Balance lb, Employee emp, HttpPostedFileBase file, decimal numOfDays)
        {
            // keeps track of how much credit points should be deducted from each balance type
            decimal deductDIL = 0;
            decimal deductSick = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Sick --> DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in sick, if yes, then simply deduct from sick, 
            // if not, deduct all the balance from sick and the remainder from DIL balance. if DIL balance 
            // is insufficient, deduct all the balance from DIL and the remainder from annual balance. 
            // if annual balance is insufficient, deduct all from annual and then add the remaining number 
            // of days to unpaid balance.
            if (lb.sick < numOfDays)
            {
                deductSick = lb.sick;
                if (lb.sick + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (lb.sick + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - deductSick - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - deductSick - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - deductSick;
                }
            }
            else
            {
                deductSick = numOfDays;
            }

            if (ModelState.IsValid)
            {
                // uploads the file to App_Data/Documentation
                string fileName = UploadFile(file);

                if (TempData["ErrorMessage"] == null)
                {
                    // inserts the data to the database
                    ApplyLeave(model, numOfDays, fileName);

                    Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                    new Email().PendingLeaveApplicationByLM(emp, empLM, model);

                    // sets the notification message to be displayed to the applicant
                    TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                    TempData["SuccessMessage"] += (deductSick > 0) ? deductSick + " day(s) will be deducted from Sick balance.<br/>" : "";
                    TempData["SuccessMessage"] += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
                    TempData["SuccessMessage"] += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
                    TempData["SuccessMessage"] += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";
                }
            }
        }

        private void LeaveAppMaternity(Leave model, Balance lb, Employee emp, HttpPostedFileBase file)
        {
            
            TimeSpan diff = model.returnDate - model.startDate;

            int numOfPublicHolidays = GetNumOfPublicHolidays(model.startDate, model.returnDate);

            // Maternity leave includes weekends but excludes public holidays
            int numOfDays = diff.Days - numOfPublicHolidays;

            // keeps track of how much credit points should be deducted from each balance type
            decimal deductMaternity = 0;
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Maternity --> DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in maternity, if yes, then simply deduct from maternity, 
            // if not, deduct all the balance from maternity and the remainder from DIL balance. if DIL balance 
            // is insufficient, deduct all the balance from DIL and the renmainder from annual, finnaly if annual 
            // is insufficient, deduct all from annual and add the remaining number of days to unpaid balance.
            if (lb.maternity < numOfDays)
            {
                deductMaternity = lb.maternity;
                if (lb.maternity + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (lb.maternity + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - deductMaternity - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - deductMaternity - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - deductMaternity;
                }
            }
            else
            {
                deductMaternity = numOfDays;
            }

            if (ModelState.IsValid)
            {
                // uploads the file to App_Data/Documentation
                string fileName = UploadFile(file);

                if (TempData["ErrorMessage"] == null)
                {
                    // inserts the data to the database
                    ApplyLeave(model, numOfDays, fileName);

                    Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                    new Email().PendingLeaveApplicationByLM(emp, empLM, model);

                    // sets the notification message to be displayed to the applicant
                    TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                    TempData["SuccessMessage"] += (deductMaternity > 0) ? deductMaternity + " day(s) will be deducted from Maternity balance.<br/>" : "";
                    TempData["SuccessMessage"] += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
                    TempData["SuccessMessage"] += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
                    TempData["SuccessMessage"] += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";
                }
            }
        }

        private void LeaveAppCompassionate(Leave model, Balance lb, Employee emp, HttpPostedFileBase file, decimal numOfDays)
        {
            decimal maxDIL = GetLeaveBalanceModel().compassionate;

            // keeps track of how much credit points should be deducted from each balance type
            decimal addCompassionate = 0;
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Compassionate --> DIL --> Annual --> Unpaid
            // checks if the applicant has not exceeded the compassionate limit, if yes, then simply add to compassionate, 
            // if not, deduct all the balance from DIL and the remainder from Annual balance. if Annual balance 
            // is insufficient, deduct all the balance from DIL and the remainder from annual balance. 
            // if annual balance is insufficient, deduct all from annual and then add the remaining number 
            // of days to unpaid balance.
            if (maxDIL < numOfDays)
            {
                addCompassionate = maxDIL;
                if (maxDIL + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (maxDIL + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - maxDIL - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - maxDIL - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - maxDIL;
                }
            }
            else
            {
                addCompassionate = numOfDays;
            }

            if (ModelState.IsValid)
            {
                // uploads the file to App_Data/Documentation
                string fileName = UploadFile(file);

                if (TempData["ErrorMessage"] == null)
                {
                    // inserts the data to the database
                    ApplyLeave(model, numOfDays, fileName);

                    Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                    new Email().PendingLeaveApplicationByLM(emp, empLM, model);

                    // sets the notification message to be displayed to the applicant
                    TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                    TempData["SuccessMessage"] += (addCompassionate > 0) ? addCompassionate + " day(s) will be added to Compassionate balance.<br/>" : "";
                    TempData["SuccessMessage"] += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
                    TempData["SuccessMessage"] += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
                    TempData["SuccessMessage"] += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";
                }
            }
        }

        private void LeaveAppShortHours(Leave model, Balance lb, Employee emp, int duration)
        {
            // clears the model state as the endDate was not set in the View page
            ModelState.Clear();

            // the applicant enters only one date, the leave is for 0 days
            model.returnDate = model.startDate;
            model.shortEndTime = model.shortStartTime;
            model.shortEndTime = new TimeSpan(model.shortStartTime.Hours + (duration / 60), model.shortStartTime.Minutes + (duration % 60), 0);
            
            // checks if the selected date is a weekend
            bool isWeekend = model.startDate.DayOfWeek.Equals(DayOfWeek.Friday) || model.startDate.DayOfWeek.Equals(DayOfWeek.Saturday) ? true : false;

            // checks if the selected date is a public holiday
            bool isPublicHoliday = IsPublicHoliday(model.startDate);

            bool isValidHours = false;
            // checks that the hours are between 6am and 10pm
            if ((model.shortStartTime.Hours >= 6 && model.shortEndTime.TotalHours <= 21) || (model.shortStartTime.Hours >= 6 && model.shortEndTime.TotalHours == 22 && model.shortEndTime.Minutes == 0))
                isValidHours = true;

            // applies for leave ONLY if the date is not a weekend or public holiday,
            // the leave duration is less than 2:30 hours, the applicant has enough balance, and the times are within a valid range
            if (ModelState.IsValid)
            {
                if (isValidHours)
                {
                    if (!isWeekend)
                    {
                        if (!isPublicHoliday)
                        {
                            if ((decimal)duration / 60 <= lb.shortHours)
                            {
                                // inserts the data to the database
                                ApplyLeave(model);

                                Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                                new Email().PendingLeaveApplicationByLM(emp, empLM, model);

                                // sets the notification message to be displayed to the applicant
                                TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + duration + " minutes</b> has been submitted successfully.<br/>";
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "You do not have enough balance.";
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "The selected date is a public holiday.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "The selected date is a weekend.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "The selected hours are invalid.";
                }
            }
        }

        private void LeaveAppPilgrimage(Leave model, Balance lb, Employee emp, HttpPostedFileBase file, decimal numOfDays)
        {
            if (ModelState.IsValid)
            {
                // does the user have enough balance?
                if (lb.pilgrimage < numOfDays)
                {
                    TempData["ErrorMessage"] = "You do not have enough balance.";
                }
                else
                { 
                    // uploads the file to App_Data/Documentation
                    string fileName = UploadFile(file);

                    if (TempData["ErrorMessage"] == null)
                    {
                        // inserts the data to the database
                        ApplyLeave(model, numOfDays, fileName);

                        Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                        new Email().PendingLeaveApplicationByLM(emp, empLM, model);

                        // sets the notification message to be displayed to the applicant
                        TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                    }
                }
            }
        }

        private void LeaveAppUnpaid(Leave model, Employee emp, HttpPostedFileBase file, decimal numOfDays)
        {
            if (ModelState.IsValid)
            {
                // uploads the file to App_Data/Documentation
                string fileName = UploadFile(file);

                if (TempData["ErrorMessage"] == null)
                {
                    // inserts the data to the database
                    ApplyLeave(model, numOfDays, fileName);

                    Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                    new Email().PendingLeaveApplicationByLM(emp, empLM, model);

                    // sets the notification message to be displayed to the applicant
                    TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                }
            }
        }

        private void LeaveAppDIL(Leave model, Employee emp, Balance lb, HttpPostedFileBase file, decimal numOfDays)
        {
            if (ModelState.IsValid)
            {
                // does the user have enough balance?
                if (lb.daysInLieu < numOfDays)
                {
                    TempData["ErrorMessage"] = "You do not have enough balance.";
                }
                else
                {
                    // uploads the file to App_Data/Documentation
                    string fileName = UploadFile(file);

                    if (TempData["ErrorMessage"] == null)
                    {
                        // inserts the data to the database
                        ApplyLeave(model, numOfDays, fileName);

                        Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                        new Email().PendingLeaveApplicationByLM(emp, empLM, model);

                        // sets the notification message to be displayed to the applicant
                        TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                    }
                }
            }
        }

        private void SetViewData(Employee emp, int leaveID)
        {
            var leaveTypesNames = GetAvailableLeaveTypesAndNames(emp);
            ViewData["LeaveTypesNames"] = leaveTypesNames;
            ViewData["SelectedLeaveTypeID"] = leaveID;
            ViewData["SelectedLeaveType"] = leaveTypesNames.Item1[leaveID];
            ViewData["ShortHourDuration"] = GetShortHourDurationList();
        }

        private Tuple<Dictionary<int, string>, Dictionary<int, string>> GetAvailableLeaveTypesAndNames(Employee emp)
        {
            Dictionary dic = new Dictionary();
            var leaveTypes = dic.AddDefaultToDictionary(dic.GetLeaveType(), 0, "- Select Leave Type -");
            var leaveNames = dic.AddDefaultToDictionary(dic.GetLeaveType(), 0, "- Select Leave Type -");

            // employees on probation can only apply for DIL leave, while off probation employees can apply for any.
            if (emp.onProbation)
            {
                var tempLeaveTypes = new Dictionary<int, string>(leaveTypes);
                foreach (var entity in tempLeaveTypes)
                {
                    if (!entity.Value.Equals("DIL") && !entity.Value.Equals("Unpaid") && entity.Key != 0)
                    {
                        leaveTypes.Remove(entity.Key);
                        leaveNames.Remove(entity.Key);
                    }
                }
            }
            else
            {
                // only female can apply for maternity leaves
                if (emp.gender != 'F')
                {
                    int maternityID = leaveTypes.FirstOrDefault(obj => obj.Value == "Maternity").Key;
                    leaveTypes.Remove(maternityID);
                    leaveNames.Remove(maternityID);
                }

                // only muslims with an employment period of 5 years or greater can apply for pilgrimage leave
                int pilgrimageID = leaveTypes.FirstOrDefault(obj => obj.Value == "Pilgrimage").Key;
                if (!IsPilgrimageAllowed(GetLoggedInID()))
                {
                    leaveTypes.Remove(pilgrimageID);
                    leaveNames.Remove(pilgrimageID);
                }

                int key = leaveTypes.FirstOrDefault(obj => obj.Value == "DIL").Key;
                leaveTypes.Remove(key);
                leaveNames.Remove(key);
            }

            return new Tuple<Dictionary<int, string>, Dictionary<int, string>>(leaveTypes, leaveNames);
        }

        private bool IsPilgrimageAllowed(int empID)
        {
            Employee emp = GetEmployeeModel(empID);
            List<Employee> employmentList = GetEmploymentPeriod(empID);

            // gets the latest employment period.
            Employee latestEmployment = employmentList[employmentList.Count - 1];
            TimeSpan diff = DateTime.Today - latestEmployment.empStartDate;
            double years = diff.TotalDays / 365.25;

            Dictionary dic = new Dictionary();
            if (dic.GetReligion()[emp.religionID].Equals("Muslim") && years >= 5)
                return true;
            else
                return false;
        }

        private void ApplyLeave(Leave lm, decimal numOfDays = 0, string fileName = "")
        {
            Dictionary dic = new Dictionary();
            int statusID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Pending_LM").Key;

            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@fileName", SqlDbType.NChar).Value = fileName;
            cmd.Parameters.Add("@sDate", SqlDbType.NChar).Value = lm.startDate.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@eDate", SqlDbType.NChar).Value = lm.returnDate.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@sTime", SqlDbType.NChar).Value = lm.shortStartTime.ToString();
            cmd.Parameters.Add("@eTime", SqlDbType.NChar).Value = lm.shortEndTime.ToString();
            cmd.Parameters.Add("@leaveTypeID", SqlDbType.Int).Value = lm.leaveTypeID;
            cmd.Parameters.Add("@contactDetails", SqlDbType.NChar).Value = lm.contactDetails ?? "";
            cmd.Parameters.Add("@comments", SqlDbType.NChar).Value = lm.comments ?? "";
            cmd.Parameters.Add("@bookAirTicket", SqlDbType.Bit).Value = lm.bookAirTicket;
            cmd.Parameters.Add("@numOfDays", SqlDbType.Decimal).Value = numOfDays;
            cmd.Parameters.Add("@statusID", SqlDbType.Int).Value = statusID;
            cmd.Parameters.Add("@email", SqlDbType.NChar).Value = lm.email ?? "";
            cmd.Parameters.Add("@isStartDateHalfDay", SqlDbType.Bit).Value = lm.isStartDateHalfDay;
            cmd.Parameters.Add("@isReturnDateHalfDay", SqlDbType.Bit).Value = lm.isReturnDateHalfDay;
            cmd.CommandText = "INSERT INTO dbo.Leave (Employee_ID, Documentation, Start_Date, Reporting_Back_Date, Start_Hrs, End_Hrs, Leave_Type_ID, " +
                "Contact_Outside_UAE, Comment, Flight_Ticket, Total_Leave, Leave_Status_ID, Personal_Email, Is_Half_Start_Date, Is_Half_Reporting_Back_Date) " +
                "VALUES (@empID, @fileName, @sDate, @eDate, @sTime, @eTime, @leaveTypeID, @contactDetails, @comments, @bookAirTicket, @numOfDays, @statusID, @email, @isStartDateHalfDay, @isReturnDateHalfDay);";
            db.Execute(cmd);

            cmd.Parameters.Add("@lastIdentity", SqlDbType.Int).Value = GetLeaveLastIdentity();
            cmd.Parameters.Add("@createdBy", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@createdOn", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Application (Leave_Application_ID, Column_Name, Value_After, Created_By, Created_On) " +
                  "VALUES(@lastIdentity, 'Leave_Status_ID', @statusID, @createdBy, @createdOn)";
            db.Execute(cmd);
        }

        private int GetNextApplicationID()
        {
            int nextApplicationID = 0;
            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.CommandText = "SELECT Leave_Application_ID FROM dbo.Leave";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(sqlCommand);

            foreach (DataRow row in dataTable.Rows)
            {
                if ((int)row["Leave_Application_ID"] > nextApplicationID)
                    nextApplicationID = (int)row["Leave_Application_ID"];
            }

            return nextApplicationID + 1;
        }

        private string UploadFile(HttpPostedFileBase file)
        {
            string fileName = "";

            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    // extract only the filename
                    fileName = Path.GetFileName(file.FileName);
                    string fName = GetNextApplicationID() + "-" + fileName;
                    string ext = Path.GetExtension(file.FileName);
                    if (ext != ".doc" && ext != ".docx" && ext != ".pdf" && ext != ".txt" && ext != ".rtf" &&
                        ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".bmp" &&
                        ext != ".csv" && ext != ".xls" && ext != ".xlsx" && ext != ".odf")
                    {
                        TempData["ErrorMEssage"] = "You have selected an invalid file type. " +
                            "<br /> Please upload one of the following file types; <b>.doc</b>, <b>.docx</b>, <b>.pdf</b>, <b>.txt</b>, <b>.rtf</b>, <b>.png</b>" +
                            ", <b>.jpg</b>, <b>.jpg</b>, <b>.jpeg</b>, <b>.bmp</b>, <b>.csv</b>, <b>.xls</b>, <b>.xlsx</b> or <b>.odf</b>";
                    }
                    else
                    {
                        // store the file inside ~/App_Data/Documentation folder
                        var path = Path.Combine(Server.MapPath("~/App_Data/Documentation"), fName);
                        file.SaveAs(path);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMEssage"] = "ERROR:" + ex.Message.ToString();
                }
            }

            return fileName;
        }

        private void CompareDates(DateTime sDate, DateTime rDate, Leave model)
        {
            int result = DateTime.Compare(sDate, rDate);
            if (result > 0)
            {
                ModelState.AddModelError("endDate", "Reporting Back date cannot be earlier than Start Date.");
                TempData["ErrorMessage"] = "Reporting Back date cannot be earlier than Start Date.";
            }
            else if (result == 0 && ((model.isStartDateHalfDay == false && model.isReturnDateHalfDay == false))) //zero days is only an error if no checkboxes are selected
            {
                ModelState.AddModelError("endDate", "Start and Reporting Back dates cannot be the same.");
                TempData["ErrorMessage"] = "Start and Reporting Back dates cannot be the same.";
            }
            if (sDate.Year < DateTime.Now.Year)
                ModelState.AddModelError("startDate", "Starting year cannot be before current year.");

            TimeSpan diff = rDate - sDate;
            if(diff.Days >= 1000)
                ModelState.AddModelError("startDate", "Amount of leave exceeds maximum limit.");
        }

        private void CompareHours(TimeSpan sTime, TimeSpan eTime)
        {
            int result = TimeSpan.Compare(sTime, eTime);
            if (result > 0)
                ModelState.AddModelError("shortEndTime", "End Time cannot be earlier than Start Time.");
            else if (result == 0)
                ModelState.AddModelError("shortEndTime", "Start and End Time cannot be the same.");
        }

        private decimal GetNumOfDays(DateTime sDate, DateTime eDate)
        {
            bool isWeekend = false;
            bool isPublicHoliday = false;
            TimeSpan diff = eDate - sDate;
            decimal numOfDays = diff.Days + (diff.Hours / (decimal)24.0); //must consider the hours too e.g. 0.5 hours of leave
            decimal fullNumOfDays = numOfDays;

            for (var i = 0; i < fullNumOfDays; i++)
            {
                switch (sDate.AddDays(i).DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        numOfDays--;
                        isWeekend = true;
                        break;

                    case DayOfWeek.Friday:
                        numOfDays--;
                        isWeekend = true;
                        break;
                }
            }

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT * FROM dbo.Public_Holiday WHERE Date BETWEEN'" + sDate.ToString("yyyy-MM-dd") + "' AND '" + eDate.ToString("yyyy-MM-dd") + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime day = (DateTime)reader["Date"];
                        for (var i = 0; i < fullNumOfDays; i++)
                        {
                            if (((sDate.AddDays(i)).Date).Equals(day.Date)) //do not use the time portion for comparison
                            {
                                numOfDays--;
                                isPublicHoliday = true;
                            }
                        }
                    }
                }
                connection.Close();
            }

            if (numOfDays <= 0 && isWeekend) //leave can go negative if numOfDays was 0.5
            {
                ViewBag.WarningMessage = "The selected date(s) is/are weekend(s).";
                ModelState.AddModelError("startDate", " ");
            }

            if (numOfDays <= 0 && isPublicHoliday) //leave can go negative if numOfDays was 0.5
            {
                ViewBag.WarningMessage = "The selected date(s) is/are public holiday(s).";
                ModelState.AddModelError("startDate", " ");
            }

            return numOfDays;
        }

        private int GetNumOfPublicHolidays(DateTime sDate, DateTime eDate)
        {
            TimeSpan diff = eDate - sDate;
            int numOfDays = diff.Days;
            int fullNumOfDays = numOfDays;
            int numOfPublicHolidays = 0;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@sDate", SqlDbType.NChar).Value = sDate.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@eDate", SqlDbType.NChar).Value = eDate.ToString("yyyy-MM-dd");
            cmd.CommandText = "SELECT * FROM dbo.Public_Holiday WHERE Date BETWEEN @sDate AND @eDate";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                DateTime day = (DateTime)row["Date"];
                for (var i = 0; i < fullNumOfDays; i++)
                {
                    if (sDate.AddDays(i).Equals(day))
                        numOfPublicHolidays++;
                }
            }

            return numOfPublicHolidays;
        }       

        private Dictionary<int, string> GetShortHourDurationList()
        {
            var durationList = new Dictionary<int, string>
            {
                { 30, "0:30 mins" },
                { 60, "1:00 hr" },
                { 90, "1:30 hrs" },
                { 120, "2:00 hrs" },
                { 150, "2:30 hrs" }
            };
            return durationList;
        }

        private int GetLeaveLastIdentity()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT MAX(Leave_Application_ID) AS LastID FROM dbo.Leave";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);
            int id = (int)dataTable.Rows[0][0];
            return id;
        }
    }
}