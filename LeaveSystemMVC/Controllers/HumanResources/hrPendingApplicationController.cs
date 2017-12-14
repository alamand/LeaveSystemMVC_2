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
            List<sLeaveModel> RetrievedApplications = new List<sLeaveModel>();

            foreach (var leave in GetLeaveModel())
            {
                if (leave.leaveStatusName.Equals("Pending_HR"))
                    RetrievedApplications.Add(leave);
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

            var balanceModel = GetLeaveBalanceModel(passingLeave.employeeID);
            var leaveModel = new List<sLeaveModel>();
            var leaveHistory = GetLeaveModel("Employee.Employee_ID", passingLeave.employeeID);

            foreach (var leave in leaveHistory)
            {
                if (!leave.leaveStatusName.Equals("Pending_LM") && !leave.leaveStatusName.Equals("Pending_HR"))
                    leaveModel.Add(leave);
            }

            ViewData["LeaveHistory"] = leaveModel;
            ViewData["Balances"] = balanceModel;

            var employee = GetEmployeeModel(passingLeave.employeeID);
            ViewData["Gender"] = employee.gender;
            ViewData["Religion"] = DBReligionList()[employee.religionID];

            TempData["Employee"] = employee;

            return View(passingLeave);
        }

        [HttpPost]
        public ActionResult Select(sLeaveModel leave, string submit)
        {
            var statusList = DBLeaveStatusList();
            int approvedID = statusList.FirstOrDefault(obj => obj.Value == "Approved").Key;
            int rejectedID = statusList.FirstOrDefault(obj => obj.Value == "Rejected_HR").Key;

            string queryString = "";
            string message = "";
            switch (submit)
            {
                case "Approve":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '" + approvedID + "', " +
                        "HR_Comment = '" + leave.hrComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + leave.leaveAppID + "' ";
                    message = "Your " + leave.leaveTypeName + " leave application " + "from " + leave.startDate + " to " + leave.returnDate + " has been fully approved.";
                    break;
                case "Reject":
                    queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '" + rejectedID + "', " +
                        "HR_Comment = '" + leave.hrComment + "' " +
                        "WHERE dbo.Leave.Leave_Application_ID = '" + leave.leaveAppID + "' ";
                    message = "Your " + leave.leaveTypeName + " leave application " + "from " + leave.startDate + " to " + leave.returnDate + " has been rejected by Human Resources.";
                    break;

                default:
                    break; ;
            }

            DBExecuteQuery(queryString);
            SendMail(message);

            return RedirectToAction("Index");
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

    }
}