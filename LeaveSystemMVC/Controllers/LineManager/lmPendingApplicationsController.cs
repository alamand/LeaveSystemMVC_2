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
    public class lmPendingApplicationsController : ControllerBase
    {
        [HttpGet]
        // GET: lmPendingApplications
        public ActionResult Index()
        {
            List<sLeaveModel> RetrievedApplications = new List<sLeaveModel>();

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            //Check if this user has nominated a substitute.
            string queryString2 = "SELECT dbo.Employee.Substitute_ID FROM dbo.Employee WHERE dbo.Employee.Employee_ID = '" + GetLoggedInID() + "'";
            Boolean substituteExists = false;
            int subsID = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["Substitute_ID"] != DBNull.Value)
                            {
                                substituteExists = true;
                                subsID = (int)reader["Substitute_ID"];
                            }
                        }
                    }
                }
            }

            //No substitute nominated, so add the leave applications of staff members who report to this user
            if (!substituteExists)
            {
                var leaveList = GetLeaveModel("Reporting.Reporting_ID", GetLoggedInID());

                foreach (var leave in leaveList)
                {
                    if (leave.leaveStatusName.Equals("Pending_LM"))
                        RetrievedApplications.Add(leave);
                }
            }

            //Check if this user is a substitute.
            queryString2 = "SELECT dbo.Employee.Employee_ID FROM dbo.Employee WHERE dbo.Employee.Substitute_ID = '" + GetLoggedInID() + "'";
            Boolean isSubstitute = false;
            int empID = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["Employee_ID"] != DBNull.Value)
                            {
                                isSubstitute = true;
                                empID = (int)reader["Employee_ID"];
                            }
                        }
                    }
                }
            }
            //Adding the leave applications to the substitute instead
            if (isSubstitute)
            {
                var leaveList = GetLeaveModel("Reporting.Reporting_ID", empID);

                foreach (var leave in leaveList)
                {
                    if (leave.leaveStatusName.Equals("Pending_LM"))
                        RetrievedApplications.Add(leave);
                }
            }

            /*Get the list of applications due for the line manager to approve*/
            TempData["RetrievedApplications"] = RetrievedApplications;
            return View(RetrievedApplications);
        }

        [HttpGet]
        public ActionResult Select(int appID)
        {
            List<sLeaveModel> passedApplications = TempData["RetrievedApplications"] as List<sLeaveModel>;
            sLeaveModel passingLeave = passedApplications.First(leave => leave.leaveAppID == appID);

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
        public ActionResult Select(sLeaveModel model, string submit)
        {
            var statusList = DBLeaveStatusList();
            int approvedID = statusList.FirstOrDefault(obj => obj.Value == "Pending_HR").Key;
            int rejectedID = statusList.FirstOrDefault(obj => obj.Value == "Rejected_LM").Key;

            string queryString = "";
            string message = "";
            switch (submit)
            {
                case "Approve":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '" + approvedID + "', " +
                        "LM_Comment = '" + model.lmComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + model.leaveAppID + "' ";
                    message = "Your " + model.leaveTypeName + " leave application " + "from " + model.startDate + " to " + model.returnDate + " has been approved by your line manager. It is now awaiting review by Human Resources.";
                    break;
                case "Reject":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '" + rejectedID + "', " +
                        "LM_Comment = '" + model.lmComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + model.leaveAppID + "' ";
                    message = "Your " + model.leaveTypeName + " leave application " + "from " + model.startDate + " to " + model.returnDate + " has been rejected by your line manager.";
                    break;

                default:
                    break; ;
            }

            DBExecuteQuery(queryString);
            SendMail(message);

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

        private void SendMail(string message)
        {
            /*Construct an e-mail and send it.*/
            var employee = TempData["Employee"] as sEmployeeModel;

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");
            mail.To.Add(new MailAddress(employee.email));
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

        protected List<sLeaveModel> GetLeaveHistory(int empID)
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