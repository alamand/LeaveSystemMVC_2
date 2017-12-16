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

            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;

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