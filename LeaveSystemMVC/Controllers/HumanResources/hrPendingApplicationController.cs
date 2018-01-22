﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Security.Claims;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using Hangfire;

namespace LeaveSystemMVC.Controllers
{
    public class hrPendingApplicationController : ControllerBase
    {
        // GET: hrPendingApplication

        [HttpGet]
        public ActionResult Index()
        {
            List<sLeaveModel> retrievedApplications = new List<sLeaveModel>();

            // extract all pending applications
            foreach (var leave in GetLeaveModel())
            {
                if (leave.leaveStatusName.Equals("Pending_HR"))
                    retrievedApplications.Add(leave);
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.WarningMessage = TempData["WarningMessage"];

            return View(retrievedApplications);
        }

        [HttpGet]
        public ActionResult Select(int appID)
        {
            sLeaveModel passingLeave = GetLeaveModel().First(leave => leave.leaveAppID == appID);
            var employee = GetEmployeeModel(passingLeave.employeeID);

            ViewData["Preview"] = AccumulationPreview(passingLeave);
            ViewData["LeaveHistory"] = GetLeaveHistory(passingLeave.employeeID);
            ViewData["Balances"] = GetLeaveBalanceModel(passingLeave.employeeID);
            ViewData["Gender"] = employee.gender;
            ViewData["Religion"] = DBReligionList()[employee.religionID];
            TempData["Employee"] = employee;

            return View(passingLeave);
        }

        [HttpPost]
        public ActionResult Select(sLeaveModel leave, string submit)
        {
            switch (submit)
            {
                case "Approve":
                    Approve(leave);
                    break;
                case "Reject":
                    Reject(leave);
                    break;

                default:
                    break; ;
            }

            if (TempData["SuccessMessage"] != null || TempData["WarningMessage"] != null)
                return RedirectToAction("Index");
            else
                return Select(leave.leaveAppID);
        }

        private Dictionary<string, decimal> AccumulationPreview(sLeaveModel leave)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();
            sleaveBalanceModel leaveBalance = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            switch (leave.leaveTypeName)
            {
                case "Annual":
                    balanceDeduction = LeaveAppAnnual(leaveBalance, numOfDays);
                    break;

                case "Sick":
                    balanceDeduction = LeaveAppSick(leaveBalance, numOfDays);
                    break;

                case "Maternity":
                    TimeSpan diff = leave.returnDate - leave.startDate;
                    balanceDeduction = LeaveAppMaternity(leaveBalance, (int)diff.Days);
                    break;

                case "Compassionate":
                    balanceDeduction.Add("Compassionate", (decimal)numOfDays);
                    break;

                case "Short_Hours_Per_Month":
                    TimeSpan span = (TimeSpan)leave.shortEndTime - (TimeSpan)leave.shortStartTime;
                    balanceDeduction.Add("Short_Hours_Per_Month", (decimal)span.TotalHours);
                    break;

                case "Pilgrimage":
                    balanceDeduction.Add("Pilgrimage", (decimal)leaveBalance.pilgrimage);
                    break;

                case "Unpaid":
                    balanceDeduction.Add("Unpaid", numOfDays);
                    break;

                default:
                    break; ;
            }

            return balanceDeduction;
        }

        private Dictionary<string, decimal> LeaveAppAnnual(sleaveBalanceModel lb, int numOfDays)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();

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

            // add what will be deducted in the dictionary
            if (deductDIL > 0)
                balanceDeduction.Add("DIL", deductDIL);
            if (deductAnnual > 0)
                balanceDeduction.Add("Annual", deductAnnual);
            if (addUnpaid > 0)
                balanceDeduction.Add("Unpaid", addUnpaid);

            return balanceDeduction;
        }

        private Dictionary<string, decimal> LeaveAppSick(sleaveBalanceModel lb, int numOfDays)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();

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

            // add what will be deducted in the dictionary
            if (deductSick > 0)
                balanceDeduction.Add("Sick", deductSick);
            if (deductDIL > 0)
                balanceDeduction.Add("DIL", deductDIL);
            if (deductAnnual > 0)
                balanceDeduction.Add("Annual", deductAnnual);
            if (addUnpaid > 0)
                balanceDeduction.Add("Unpaid", addUnpaid);

            return balanceDeduction;
        }

        private Dictionary<string, decimal> LeaveAppMaternity(sleaveBalanceModel lb, int numOfDays)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();

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

            // add what will be deducted in the dictionary
            if (deductMaternity > 0)
                balanceDeduction.Add("Maternity", deductMaternity);
            if (deductDIL > 0)
                balanceDeduction.Add("DIL", deductDIL);
            if (deductAnnual > 0)
                balanceDeduction.Add("Annual", deductAnnual);
            if (addUnpaid > 0)
                balanceDeduction.Add("Unpaid", addUnpaid);

            return balanceDeduction;
        }

        private int GetNumOfDays(DateTime sDate, DateTime eDate)
        {
            // @TODO: Test for all cases
            TimeSpan diff = eDate - sDate;
            int numOfDays = diff.Days;          // number of days excluding public holidays and weekends
            int fullNumOfDays = numOfDays;      // number of days including public holidays and weekends

            // exclude weekend
            // go through each day from start date up to number of days
            for (var i = 0; i < fullNumOfDays; i++)
            {
                switch (sDate.AddDays(i).DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        numOfDays--;            
                        break;
                    case DayOfWeek.Friday:
                        numOfDays--;
                        break;
                }
            }

            // exclude public holidays
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
                            if (sDate.AddDays(i).Equals(day))
                            {
                                numOfDays--;
                            }
                        }
                    }
                }
                connection.Close();
            }

            return numOfDays;
        }

        private void Reject(sLeaveModel leave)
        {
            int rejectedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Rejected_HR").Key;
            DBUpdateLeave(leave, rejectedID);

            string message = "Rejected";      //@TODO: write email
            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

            TempData["WarningMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>rejected</b> successfully.<br/>";
        }

        private void Approve(sLeaveModel leave)
        {
            switch (leave.leaveTypeName)
            {
                case "Annual":
                    ApproveAnnualLeave(leave);
                    break;

                case "Sick":
                    ApproveSickLeave(leave);
                    break;

                case "Maternity":
                    ApproveMaternityLeave(leave);
                    break;

                case "Compassionate":
                    ApproveCompassionate(leave);
                    break;

                case "Short_Hours_Per_Month":
                    ApproveShortHours(leave);
                    break;

                case "Pilgrimage":
                    ApprovePilgrimage(leave);
                    break;

                case "Unpaid":
                    ApproveUnpaid(leave);
                    break;

                default:
                    break; ;
            }
        }

        private void ApproveAnnualLeave(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

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

            int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
            DBUpdateLeave(leave, approvedID);

            DBUpdateBalance(leave.employeeID, lb.daysInLieuID, lb.daysInLieu - deductDIL);
            DBUpdateBalance(leave.employeeID, lb.annualID, lb.annual - deductAnnual);
            DBUpdateBalance(leave.employeeID, lb.unpaidID, lb.unpaid + addUnpaid);

            string message = "Approved"; //@TODO: Write an email

            // sends a notification email to the applicant
            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
        }

        private void ApproveSickLeave(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            // keeps track of how much credit points should be deducted from each balance type
            decimal deductDIL = 0;
            decimal deductSick = 0;
            decimal addUnpaid = 0;

            // deduction order: Sick --> DIL --> Unpaid
            // checks if the applicant has enough balance in sick, if yes, then simply deduct from sick, 
            // if not, deduct all the balance from sick and the remainder from DIL balance. if DIL balance 
            // is insufficient, deduct all the balance from DIL and then add the remaining number 
            // of days to unpaid balance.
            if (lb.sick < numOfDays)
            {
                deductSick = lb.sick;
                if (lb.sick + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    addUnpaid = numOfDays - deductSick - deductDIL;
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

            int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
            DBUpdateLeave(leave, approvedID);

            DBUpdateBalance(leave.employeeID, lb.sickID, (lb.sick - deductSick));
            DBUpdateBalance(leave.employeeID, lb.daysInLieuID, (lb.daysInLieu - deductDIL));
            DBUpdateBalance(leave.employeeID, lb.unpaidID, (lb.unpaid + addUnpaid));

            string message = "Approved"; //@TODO: Write an email

            // sends a notification email to the applicant
            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
        }

        private void ApproveMaternityLeave(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // Maternity leave includes weekends and public holidays
            TimeSpan diff = leave.returnDate - leave.startDate;

            // the duration of leave is the number of days between the two dates
            int numOfDays = diff.Days;

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

            int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
            DBUpdateLeave(leave, approvedID);

            DBUpdateBalance(leave.employeeID, lb.maternityID, lb.maternity - deductMaternity);
            DBUpdateBalance(leave.employeeID, lb.daysInLieuID, (lb.daysInLieu - deductDIL));
            DBUpdateBalance(leave.employeeID, lb.annualID, lb.annual - deductAnnual);
            DBUpdateBalance(leave.employeeID, lb.unpaidID, (lb.unpaid + addUnpaid));

            string message = "Approved"; //@TODO: Write an email

            // sends a notification email to the applicant
            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";

        }

        private void ApproveCompassionate(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            // does the user have enough balance?
            if (lb.compassionate >= numOfDays)
            {
                int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
                DBUpdateLeave(leave, approvedID);

                DBUpdateBalance(leave.employeeID, lb.compassionateID, lb.compassionate - numOfDays);

                string message = "Approved"; //@TODO: Write an email

                // sends a notification email to the applicant
                BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

                // sets the notification message to be displayed
                TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "Leave application ID <b>" + leave.leaveAppID + "</b> can't be approved, <b>" + leave.employeeName + "</b> does not have enough balance.";
            }
        }

        private void ApproveShortHours(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of hours
            TimeSpan span = (TimeSpan)leave.shortEndTime - (TimeSpan)leave.shortStartTime;

            // does the user have enough balance?
            if (lb.shortHours >= (decimal)span.TotalHours)
            {
                int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
                DBUpdateLeave(leave, approvedID);

                DBUpdateBalance(leave.employeeID, lb.shortHoursID, lb.shortHours - (decimal)span.TotalHours);

                string message = "Approved"; //@TODO: Write an email

                // sends a notification email to the applicant
                BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

                // sets the notification message to be displayed
                TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "Leave application ID <b>" + leave.leaveAppID + "</b> can't be approved, <b>" + leave.employeeName + "</b> does not have enough balance.";
            }
        }

        private void ApprovePilgrimage(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            // does the user have enough balance?
            if (lb.pilgrimage >= numOfDays)
            {
                int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
                DBUpdateLeave(leave, approvedID);

                DBUpdateBalance(leave.employeeID, lb.pilgrimageID, lb.pilgrimage - lb.pilgrimage);

                string message = "Approved"; //@TODO: Write an email

                // sends a notification email to the applicant
                BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

                // sets the notification message to be displayed
                TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "Leave application ID <b>" + leave.leaveAppID + "</b> can't be approved, <b>" + leave.employeeName + "</b> does not have enough balance.";
            }
        }

        private void ApproveUnpaid(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
            DBUpdateLeave(leave, approvedID);

            DBUpdateBalance(leave.employeeID, lb.unpaidID, lb.unpaid + numOfDays);

            string message = "Approved"; //@TODO: Write an email

            // sends a notification email to the applicant
            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leave.employeeID).email, message));

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
        }

        private void DBUpdateLeave(sLeaveModel leave, int approvalID)
        {
            string queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '" + approvalID + "', " +
                       "HR_Comment = '" + leave.hrComment + "' " +
                       "WHERE Leave_Application_ID = '" + leave.leaveAppID + "' ";
            DBExecuteQuery(queryString);
        }

        private void DBUpdateBalance(int empID, int leaveID, decimal balance)
        {
            string queryString = "UPDATE dbo.Leave_Balance SET Balance = '" + balance + "' " +
                       "WHERE Employee_ID = '" + empID + "' AND Leave_ID = '" + leaveID + "' ";
            DBExecuteQuery(queryString);
        }

        public void SendMail(string email, string message)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");
            mail.To.Add(new MailAddress(email));
            mail.Subject = "Leave Application Update";
            mail.Body = message + Environment.NewLine;

            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("project_ict333@murdochdubai.ac.ae", "ict@333");
            try
            {
                client.Send(mail);
                System.Diagnostics.Debug.WriteLine("Mail Sent");
            }
            catch (Exception e){}
        }

        private List<sLeaveModel> GetLeaveHistory(int empID)
        {
            var leaveHistory = new List<sLeaveModel>();
            var leaves = GetLeaveModel("Employee.Employee_ID", empID);

            foreach (var leave in leaves)
            {
                if (!leave.leaveStatusName.Equals("Pending_LM") && !leave.leaveStatusName.Equals("Pending_HR"))
                    leaveHistory.Add(leave);
            }

            return leaveHistory;
        }
    }
}