using System;
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

namespace LeaveSystemMVC.Controllers
{
    // @TODO: Need to optimize this more, after new changes on the Department Table
    public class hrPendingApplicationController : ControllerBase
    {
        [HttpGet]
        // GET: hrPendingApplication
        public ActionResult Index()
        {
            List<sLeaveModel> retrievedApplications = new List<sLeaveModel>();

            foreach (var leave in GetLeaveModel())
            {
                if (leave.leaveStatusName.Equals("Pending_HR"))
                    retrievedApplications.Add(leave);
            }

            return View(retrievedApplications);
        }

        [HttpGet]
        public ActionResult Select(int appID)
        {
            sLeaveModel passingLeave = GetLeaveModel().First(leave => leave.leaveAppID == appID);
            var employee = GetEmployeeModel(passingLeave.employeeID);

            ViewData["Message"] = CalculatedBalance(passingLeave);
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
                    int rejectedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Rejected_HR").Key;
                    DBUpdateLeave(leave, rejectedID);

                    string message = "Your " + leave.leaveTypeName + " leave application " + "from " + leave.startDate + " to " + leave.returnDate + " has been rejected by Human Resources.";
                    SendMail(GetEmployeeModel(leave.employeeID).email, message);
                    break;

                default:
                    break; ;
            }

            return RedirectToAction("Index");
        }

        private string CalculatedBalance(sLeaveModel leave)
        {
            string message = "";

            sleaveBalanceModel leaveBalance = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            switch (leave.leaveTypeName)
            {
                case "Annual":
                    message = LeaveAppAnnual(leaveBalance, numOfDays);
                    break;

                case "Sick":
                    message = LeaveAppSick(leaveBalance, numOfDays);
                    break;

                case "Maternity":
                    TimeSpan diff = leave.returnDate - leave.startDate;
                    message = (leaveBalance.maternity < diff.Days) ? "Not enough credit balance." : "<b>" + numOfDays + " day(s)</b> will be accumulated.";
                    break;

                case "Compassionate":
                    message = (leaveBalance.compassionate < numOfDays) ? "Not enough credit balance." : "<b>" + numOfDays + " day(s)</b> will be accumulated.";
                    break;

                case "Short_Hours_Per_Month":
                    TimeSpan span = (TimeSpan)leave.shortEndTime - (TimeSpan)leave.shortStartTime;
                    message = (leaveBalance.compassionate < numOfDays) ? "Not enough credit balance." : "<b>" + span.TotalMinutes + " minutes(s)</b> will be accumulated.";
                    break;

                case "Pilgrimage":
                    message = (leaveBalance.pilgrimage < numOfDays) ? "Not enough credit balance." : "<b>" + numOfDays + " day(s)</b> will be accumulated.";
                    break;

                default:
                    break; ;
            }

            return message;
        }

        private string LeaveAppAnnual(sleaveBalanceModel lb, int numOfDays)
        {
            string message = "";

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

            // sets the notification message to be displayed to the applicant
            message += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
            message += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
            message += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";
            return message;
        }

        private string LeaveAppSick(sleaveBalanceModel lb, int numOfDays)
        {
            string message = "";

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

            // sets the notification message to be displayed to the applicant
            message += (deductSick > 0) ? deductSick + " day(s) will be deducted from Sick balance.<br/>" : "";
            message += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
            message += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
            message += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";

            return message;
        }

        private int GetNumOfDays(DateTime sDate, DateTime eDate)
        {
            // @TODO: Test for all cases
            TimeSpan diff = eDate - sDate;
            int numOfDays = diff.Days;
            int fullNumOfDays = numOfDays;

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

            // sets the notification message to be displayed to the applicant
            TempData["SuccessMessage"] = "leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
            TempData["SuccessMessage"] += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
            TempData["SuccessMessage"] += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
            TempData["SuccessMessage"] += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";

            string message = "Approved"; //@TODO: Write an email

            // sends a notification email to the applicant
            SendMail(GetEmployeeModel(leave.employeeID).email, message);
        }

        private void ApproveSickLeave(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

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
            System.Diagnostics.Debug.WriteLine("Days: " + numOfDays);
            System.Diagnostics.Debug.WriteLine("D-DIL: " + deductDIL);
            System.Diagnostics.Debug.WriteLine("D-Sick: " + deductSick);
            System.Diagnostics.Debug.WriteLine("D-Annual: " + deductAnnual);
            System.Diagnostics.Debug.WriteLine("D-Unpaid: " + addUnpaid);
            System.Diagnostics.Debug.WriteLine("B-DIL: " + lb.daysInLieu);
            System.Diagnostics.Debug.WriteLine("B-Sick: " + lb.sick);
            System.Diagnostics.Debug.WriteLine("B-Annual: " + lb.annual);
            System.Diagnostics.Debug.WriteLine("B-Unpaid: " + lb.unpaid);
            System.Diagnostics.Debug.WriteLine("B-D-DIL: " + (lb.daysInLieu - deductDIL));
            System.Diagnostics.Debug.WriteLine("B-D-Sick: " + (lb.sick - deductSick));
            System.Diagnostics.Debug.WriteLine("B-D-Annual: " + (lb.annual - deductAnnual));
            System.Diagnostics.Debug.WriteLine("B-D-Unpaid: " + (lb.unpaid + addUnpaid));

            int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
            DBUpdateLeave(leave, approvedID);

            DBUpdateBalance(leave.employeeID, lb.sickID, (lb.sick - deductSick));
            DBUpdateBalance(leave.employeeID, lb.daysInLieuID, (lb.daysInLieu - deductDIL));
            DBUpdateBalance(leave.employeeID, lb.annualID, (lb.annual - deductAnnual));
            DBUpdateBalance(leave.employeeID, lb.unpaidID, (lb.unpaid + addUnpaid));

            // sets the notification message to be displayed to the applicant
            TempData["SuccessMessage"] += (deductSick > 0) ? deductSick + " day(s) will be deducted from Sick balance.<br/>" : "";
            TempData["SuccessMessage"] += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
            TempData["SuccessMessage"] += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
            TempData["SuccessMessage"] += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";

            string message = "Approved"; //@TODO: Write an email

            // sends a notification email to the applicant
            SendMail(GetEmployeeModel(leave.employeeID).email, message);
        }

        private void ApproveMaternityLeave(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // Maternity leave includes weekends and public holidays
            TimeSpan diff = leave.returnDate - leave.startDate;

            // the duration of leave is the number of days between the two dates
            int numOfDays = diff.Days;

            // does the user have enough balance?
            if (lb.maternity >= numOfDays)
            {
                int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
                DBUpdateLeave(leave, approvedID);

                DBUpdateBalance(leave.employeeID, lb.maternityID, lb.maternity - numOfDays);

                string message = "Approved"; //@TODO: Write an email

                // sends a notification email to the applicant
                SendMail(GetEmployeeModel(leave.employeeID).email, message);

                // sets the notification message to be displayed to the applicant
                TempData["SuccessMessage"] = "Your " + leave.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
            }
            else             
            {
                ViewBag.ErrorMessage = "You do not have enough balance.";
            }
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
                SendMail(GetEmployeeModel(leave.employeeID).email, message);

                // sets the notification message to be displayed to the applicant
                TempData["SuccessMessage"] = "Your " + leave.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "You do not have enough balance.";
            }
        }

        private void ApproveShortHours(sLeaveModel leave)
        {
            sleaveBalanceModel lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of hours
            TimeSpan span = (TimeSpan)leave.shortEndTime - (TimeSpan)leave.shortStartTime;

            if (span.Hours < lb.shortHours)
            {
                int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
                DBUpdateLeave(leave, approvedID);

                DBUpdateBalance(leave.employeeID, lb.shortHoursID, lb.shortHours - span.Hours);

                string message = "Approved"; //@TODO: Write an email

                // sends a notification email to the applicant
                SendMail(GetEmployeeModel(leave.employeeID).email, message);

                // sets the notification message to be displayed to the applicant
                TempData["SuccessMessage"] = "Your " + leave.leaveTypeName + " leave application for <b>" + span.TotalMinutes + " minutes</b> has been submitted successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "You do not have enough balance.";
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

                DBUpdateBalance(leave.employeeID, lb.pilgrimageID, lb.pilgrimage - numOfDays);

                string message = "Approved"; //@TODO: Write an email

                // sends a notification email to the applicant
                SendMail(GetEmployeeModel(leave.employeeID).email, message);

                // sets the notification message to be displayed to the applicant
                TempData["SuccessMessage"] = "Your " + leave.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "You do not have enough balance.";
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
            SendMail(GetEmployeeModel(leave.employeeID).email, message);

            // sets the notification message to be displayed to the applicant
            TempData["SuccessMessage"] = "Your " + leave.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
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

        private void SendMail(string email, string message)
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
            }
            catch (Exception e)
            {
                Response.Write("<script> alert('The email could not be sent due to a network error.');</script>");
            }
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