using LeaveSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Claims;
using System.IO;
using System.Net.Mail;
using System.Net;

// @TODO: More testing and optimization
// @Mandy: Give some good words for pop up messages.

namespace LeaveSystemMVC.Controllers
{
    public class LApplicationController : ControllerBase
    {
        // GET: LApplication
        [HttpGet]
        public ActionResult Index(int leaveTypeID = 0)
        {
            sEmployeeModel emp = GetEmployeeModel(GetLoggedInID());
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
        public ActionResult Index(HttpPostedFileBase file, sLeaveModel model)
        {
            sEmployeeModel emp = GetEmployeeModel(GetLoggedInID());
            sleaveBalanceModel leaveBalance = GetLeaveBalanceModel(GetLoggedInID());
            ModelState.Clear();

            // checks if the dates are the same, and if the end date is before than start date (sets ModelState to invalid if one of them is true)
            CompareDates(model.startDate, model.endDate);

            // gets the total number of days, this involves excluding weekends and public holidays
            int numOfDays = GetNumOfDays(model.startDate, model.endDate);

            if (numOfDays > 0 || (model.shortStartTime != null && model.shortEndTime != null))
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

                    case "Short_Hours_Per_Month":
                        LeaveAppShortHours(model, leaveBalance, emp);
                        break;

                    case "Pilgrimage":
                        LeaveAppPilgrimage(model, leaveBalance, emp, file, numOfDays);
                        break;

                    case "Unpaid":
                        LeaveAppUnpaid(model, emp, file, numOfDays);
                        break;

                    default:
                        break; ;
                }
            }
            else
            {
                ViewBag.WarningMessage = "The selected date(s) is/are weekend(s), and/or public holiday(s)";
            }

            SetViewData(emp, model.leaveTypeID);

            // if the application was submitted successfully, then display success message, else show the application page again.
            if (TempData["SuccessMessage"] != null)
                return RedirectToAction("Index");
            else
                return View(model);
        }

        private void LeaveAppAnnual(sLeaveModel model, sleaveBalanceModel lb, sEmployeeModel emp, HttpPostedFileBase file, int numOfDays)
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
                
                // inserts the data to the database
                ApplyLeave(model, numOfDays, fileName);

                // sends a notification email to the applicant
                SendMail(model, emp);

                // sets the notification message to be displayed to the applicant
                TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                TempData["SuccessMessage"] += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
                TempData["SuccessMessage"] += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
                TempData["SuccessMessage"] += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";
            }
        }

        private void LeaveAppSick(sLeaveModel model, sleaveBalanceModel lb, sEmployeeModel emp, HttpPostedFileBase file, int numOfDays)
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
                
                // inserts the data to the database
                ApplyLeave(model, numOfDays, fileName);
                
                // sends a notification email to the applicant
                SendMail(model, emp);
                
                // sets the notification message to be displayed to the applicant
                TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                TempData["SuccessMessage"] += (deductSick > 0) ? deductSick + " day(s) will be deducted from Sick balance.<br/>" : "";
                TempData["SuccessMessage"] += (deductDIL > 0) ? deductDIL + " day(s) will be deducted from Days In Lieu balance.<br/>" : "";
                TempData["SuccessMessage"] += (deductAnnual > 0) ? deductAnnual + " day(s) will be deducted from Annual balance.<br/>" : "";
                TempData["SuccessMessage"] += (addUnpaid > 0) ? addUnpaid + " day(s) will be added to Unpaid balance.<br/>" : "";
            }
        }

        private void LeaveAppMaternity(sLeaveModel model, sleaveBalanceModel lb, sEmployeeModel emp, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                // Maternity leave includes weekends and public holidays
                TimeSpan diff = model.endDate - model.startDate;

                // the duration of leave is the number of days between the two dates
                int numOfDays = diff.Days;

                // does the user have enough balance?
                if (lb.maternity < numOfDays)
                {
                    ViewBag.ErrorMessage = "You do not have enough balance.";
                }
                else
                {
                    // uploads the file to App_Data/Documentation
                    string fileName = UploadFile(file);
                    
                    // inserts the data to the database
                    ApplyLeave(model, numOfDays, fileName);
                    
                    // sends a notification email to the applicant
                    SendMail(model, emp);

                    // sets the notification message to be displayed to the applicant
                    TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                }
            }
        }

        private void LeaveAppCompassionate(sLeaveModel model, sleaveBalanceModel lb, sEmployeeModel emp, HttpPostedFileBase file, int numOfDays)
        {
            if (ModelState.IsValid)
            {
                // does the user have enough balance?
                if (lb.compassionate < numOfDays)
                {
                    ViewBag.ErrorMessage = "You do not have enough balance.";
                }
                else
                {
                    // uploads the file to App_Data/Documentation
                    string fileName = UploadFile(file);
                   
                    // inserts the data to the database
                    ApplyLeave(model, numOfDays, fileName);
                   
                    // sends a notification email to the applicant
                    SendMail(model, emp);
                   
                    // sets the notification message to be displayed to the applicant
                    TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                }
            }
        }

        private void LeaveAppShortHours(sLeaveModel model, sleaveBalanceModel lb, sEmployeeModel emp)
        {
            // clears the model state as the endDate was not set in the View page
            ModelState.Clear();

            // the applicant enters only one date, the leave is for 0 days
            model.endDate = model.startDate;

            // checks if the times are the same, and if the end time is before than start time (sets ModelState to invalid if one of them is true)
            CompareHours((TimeSpan)model.shortStartTime, (TimeSpan)model.shortEndTime);

            // gets the total number of hours
            TimeSpan span = (TimeSpan)model.shortEndTime - (TimeSpan)model.shortStartTime;

            // checks if the selected date is a weekend
            bool isWeekend = model.startDate.DayOfWeek.Equals(DayOfWeek.Friday) || model.startDate.DayOfWeek.Equals(DayOfWeek.Saturday) ? true : false;

            // checks if the selected date is a public holiday
            bool isPublicHoliday = IsPublicHoliday(model.startDate);

            // applys for leave ONLY if the date is not a weekend or public holiday,
            // the leave duration is less than 2:30 hours, and the applicant has enough balance
            if (ModelState.IsValid)
            {
                if (!isWeekend)
                {
                    if (!isPublicHoliday)
                    {
                        if (span.TotalMinutes <= 150)        // 2:30 hours
                        {
                            if (span.Hours < lb.shortHours)
                            {
                                // inserts the data to the database
                                ApplyLeave(model);
                                
                                // sends a notification email to the applicant
                                SendMail(model, emp);
                                
                                // sets the notification message to be displayed to the applicant
                                TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + span.TotalMinutes + " minutes</b> has been submitted successfully.<br/>";
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "You do not have enough balance.";
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "You cannot apply for more than 2:30 hrs.";
                        }
                    }
                    else
                    {
                        ViewBag.WarningMessage = "The selected date is a public holiday.";
                    }
                }
                else
                {
                    ViewBag.WarningMessage = "The selected date is a weekend.";
                }
            }
        }

        private void LeaveAppPilgrimage(sLeaveModel model, sleaveBalanceModel lb, sEmployeeModel emp, HttpPostedFileBase file, int numOfDays)
        {
            if (ModelState.IsValid)
            {
                // does the user have enough balance?
                if (lb.pilgrimage > numOfDays)
                {
                    // uploads the file to App_Data/Documentation
                    string fileName = UploadFile(file);

                    // inserts the data to the database
                    ApplyLeave(model, numOfDays, fileName);

                    // sends a notification email to the applicant
                    SendMail(model, emp);

                    // sets the notification message to be displayed to the applicant
                    TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
                }
                else
                {
                    ViewBag.ErrorMessage = "You do not have enough balance.";
                }
            }
        }

        private void LeaveAppUnpaid(sLeaveModel model, sEmployeeModel emp, HttpPostedFileBase file, int numOfDays)
        {
            if (ModelState.IsValid)
            {
                // uploads the file to App_Data/Documentation
                string fileName = UploadFile(file);

                // inserts the data to the database
                ApplyLeave(model, numOfDays, fileName);

                // sends a notification email to the applicant
                SendMail(model, emp);

                // sets the notification message to be displayed to the applicant
                TempData["SuccessMessage"] = "Your " + model.leaveTypeName + " leave application for <b>" + numOfDays + " day(s)</b> has been submitted successfully.<br/>";
            }
        }

        private void SetViewData(sEmployeeModel emp, int leaveID)
        {
            var leaveTypes = GetAvailableLeaveTypes(emp);
            ViewData["LeaveTypes"] = leaveTypes;
            ViewData["SelectedLeaveTypeID"] = leaveID;
            ViewData["SelectedLeaveTypeName"] = leaveTypes[leaveID];
        }

        private Dictionary<int, string> GetAvailableLeaveTypes(sEmployeeModel emp)
        {
            var leaveTypes = AddDefaultToDictionary(DBLeaveTypeList(), 0, "- Select Leave Type -");

            // employees on probation can only apply for DIL leave, while off probation employees can apply for any.
            if (emp.onProbation)
            {
                var tempLeaveTypes = new Dictionary<int, string>(leaveTypes);
                foreach (var entity in tempLeaveTypes)
                {
                    if (!entity.Value.Equals("DIL") && entity.Key != 0)
                    {
                        leaveTypes.Remove(entity.Key);
                    }
                }
            }
            else
            {
                if (emp.gender != 'F')
                {
                    int maternityID = leaveTypes.FirstOrDefault(obj => obj.Value == "Maternity").Key;
                    leaveTypes.Remove(maternityID);
                }

                if (!emp.religionID.Equals("Muslim"))
                {
                    int pilgrimageID = leaveTypes.FirstOrDefault(obj => obj.Value == "Pilgrimage").Key;
                    leaveTypes.Remove(pilgrimageID);
                }

                leaveTypes.Remove(leaveTypes.FirstOrDefault(obj => obj.Value == "DIL").Key);
            }

            return leaveTypes;
        }

        private void SendMail(sLeaveModel lm, sEmployeeModel emp)
        {
            string message = "";
            if (lm.shortStartTime == null && lm.shortEndTime == null)
                message = "Your " + lm.leaveTypeName + " leave application from " + lm.startDate + " to " + lm.returnDate + " has been sent to your line manager for approval.";
            else
                message = "Your " + lm.leaveTypeName + " leave application for " + lm.startDate + " from " + lm.shortStartTime + " to " + lm.shortEndTime + " has been sent to your line manager for approval.";

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");
            mail.To.Add(new MailAddress(emp.email));
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
            Response.Write("<script> alert('Your leave application has been submitted.');location.href='Index'</script>");
        }

        private void ApplyLeave(sLeaveModel lm, int numOfDays=0, string fName="")
        {
            string queryString = "INSERT INTO dbo.Leave (Employee_ID, Documentation, Start_Date, Reporting_Back_Date, Start_Hrs, End_Hrs, Leave_ID, " +
                "Contact_Outside_UAE, Comment, Flight_Ticket, Total_Leave, Leave_Status_ID) " +
                "VALUES ('" + GetLoggedInID() + "','" + fName + "','" + lm.startDate.ToString("yyyy-MM-dd") + "','" + lm.endDate.ToString("yyyy-MM-dd") + 
                "','" + lm.shortStartTime.ToString() + "','" + lm.shortEndTime.ToString() + "','" + lm.leaveTypeID + "','" + lm.contactDetails + "','" + 
                lm.comments + "','" + lm.bookAirTicket + "','" + numOfDays + "','0');";
            DBExecuteQuery(queryString);
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

                    // store the file inside ~/App_Data/Documentation folder
                    var path = Path.Combine(Server.MapPath("~/App_Data/Documentation"), fName);
                    file.SaveAs(path);
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "ERROR:" + ex.Message.ToString();
                }
            }
            return fileName;
        }

        private int GetNextApplicationID()
        {
            // this is used for naming the uploaded file
            int nextApplicationID = 0;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT Leave_Application_ID FROM dbo.Leave";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((int)reader["Leave_Application_ID"] > nextApplicationID)
                            nextApplicationID = (int)reader["Leave_Application_ID"];
                    }
                }
                connection.Close();
            }

            return nextApplicationID+1;
        }

        private void CompareDates(DateTime sDate, DateTime rDate)
        {
            int result = DateTime.Compare(sDate, rDate);
            if (result > 0)
                ModelState.AddModelError("endDate", "Reporting Back date cannot be earlier than Start Date.");
            else if (result == 0)
                ModelState.AddModelError("endDate", "Start and Reporting Back dates cannot be the same.");
        }

        private void CompareHours(TimeSpan sTime, TimeSpan eTime)
        {
            int result = TimeSpan.Compare(sTime, eTime);
            if (result > 0)
                ModelState.AddModelError("shortEndTime", "End Time cannot be earlier than Start Time.");
            else if (result == 0)
                ModelState.AddModelError("shortEndTime", "Start and End Time cannot be the same.");
        }

        private int GetNumOfDays(DateTime sDate, DateTime eDate)
        {
            // @TODO: Test for all cases
            bool isWeekend = false;
            bool isPublicHoliday = false;
            TimeSpan diff = eDate - sDate;
            int numOfDays = diff.Days;
            int fullNumOfDays = numOfDays;

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
                            if (sDate.AddDays(i).Equals(day))
                            {
                                numOfDays--;
                                isPublicHoliday = true;
                            }
                        }
                    }
                }
                connection.Close();
            }

            if (numOfDays == 0 && isWeekend)
            {
                ViewBag.WarningMessage = "The selected date(s) is/are weekend(s).";
                ModelState.AddModelError("startDate", " ");
            }


            if (numOfDays == 0 && isPublicHoliday)
            {
                ViewBag.WarningMessage = "The selected date(s) is/are public holiday(s).";
                ModelState.AddModelError("startDate", " ");
            }

            return numOfDays;
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
    }
}