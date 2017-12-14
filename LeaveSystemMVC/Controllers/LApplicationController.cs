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

namespace LeaveSystemMVC.Controllers
{
    public class LApplicationController : ControllerBase
    {
        // GET: LApplication
        [HttpGet]
        public ActionResult Index(int leaveTypeID = 0)
        {
            sEmployeeModel emp = GetEmployeeModel(GetLoggedInID());
            SetViewDatas(emp, leaveTypeID);

            return View();
        }

        [HttpPost]
        public ActionResult Select(FormCollection form)
        {
            int leaveID = Convert.ToInt32(form["selectedLeaveTypeID"]);
            return RedirectToAction("Index", new { leaveTypeID = leaveID });
        }

        [HttpPost]
        public ActionResult Index(sLeaveModel model)
        {
            sEmployeeModel emp = GetEmployeeModel(GetLoggedInID());
            sleaveBalanceModel leaveBalance = GetLeaveBalanceModel(GetLoggedInID());
            string leaveType = DBLeaveTypeList()[model.leaveID];
            ModelState.Clear();

            switch (leaveType)
            {
                case "Annual":
                    CompareDates(model.startDate, model.endDate);

                    int numOfDays = GetNumOfDays(model.startDate, model.endDate);

                    if (leaveBalance.annual < numOfDays)
                        ModelState.AddModelError("endDate", "You do not have enough annual balance to apply for this duration.");

                    if (ModelState.IsValid)
                    {
                        string fileName = UploadFile(model.supportingDocs); // does not upload
                        ApplyAnnualLeave(model, numOfDays, fileName);   
                        SendMail(model, emp, leaveType);                    // not sure if it sends
                    }
                    break;
                
                    // need to add the rest of the leave types
                default:
                    break; ;

            }


            SetViewDatas(emp, model.leaveID);
            return View(model);
        }

        private void SetViewDatas(sEmployeeModel emp, int leaveID)
        {
            var leaveTypes = GetAvailableLeaveTypes(emp);
            ViewData["LeaveTypes"] = leaveTypes;
            ViewData["SelectedLeaveTypeID"] = leaveID;
            ViewData["SelectedLeaveTypeName"] = leaveTypes[leaveID];
        }

        private Dictionary<int, string> GetAvailableLeaveTypes(sEmployeeModel emp)
        {
            var leaveTypes = AddDefaultToDictionary(DBLeaveTypeList(), 0, "- Select Leave Type -");

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

            return leaveTypes;
        }

        private void SendMail(sLeaveModel lm, sEmployeeModel emp, string leaveType)
        {
            string message = "Your " + leaveType + " leave application from " + lm.startDate + " to " + lm.returnDate + " has been sent to your line manager for approval.";

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

        private void ApplyAnnualLeave(sLeaveModel lm, int numOfDays, string fName)
        {
            string queryString = "INSERT INTO dbo.Leave (Employee_ID, Document, Start_Date, End_Date, Leave_ID, " +
                "Contact_Outside_UAE, Comment, Flight_Ticket, Total_Leave_Days, Leave_Status_ID) " +
                "VALUES ('" + GetLoggedInID() + "','" + fName + "','" + lm.startDate.ToString("yyyy-MM-dd") + "','" + lm.endDate.ToString("yyyy-MM-dd") +
                "','" + lm.leaveID + "','" + lm.contactDetails + "','" + lm.comments + "','" + lm.bookAirTicket + "','" + numOfDays + "','0');";
            DBExecuteQuery(queryString);
            }

        private string UploadFile(HttpPostedFileBase file)
        {
            string fName = "";
            System.Diagnostics.Debug.WriteLine("UPLOADING...");
            var fileName = Path.GetFileName(file.FileName);
            fName = GetNextApplicationID() + "-" + fileName;
            System.Diagnostics.Debug.WriteLine("File Name: " + fName);
            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                // extract only the filename
                System.Diagnostics.Debug.WriteLine("NOT NULL...");


                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("~/App_Data/Application_Documents"), fName);
                file.SaveAs(path);
                System.Diagnostics.Debug.WriteLine("DONE");

            }

            return fName;
        }

        private int GetNextApplicationID()
        {
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

            return nextApplicationID;
        }

        private void CompareDates(DateTime sDate, DateTime rDate)
        {
            int result = DateTime.Compare(sDate, rDate);
            if (result > 0)
                ModelState.AddModelError("endDate", "Reporting Back date cannot be earlier than Start Date.");
            else if (result == 0)
                ModelState.AddModelError("endDate", "Start and Reporting Back dates cannot be the same.");
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
                ModelState.AddModelError("startDate", "This date is a weekend.");

            if (numOfDays == 0 && isPublicHoliday)
                ModelState.AddModelError("startDate", "This date is a public holiday.");

            if (numOfDays > 30)
                ModelState.AddModelError("endDate", "You cannot apply for more than 30 days duration.");

            return numOfDays;
        }

    }
}