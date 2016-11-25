using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Security.Claims;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class lmPendingApplicationsController : Controller
    {
        [HttpGet]
        // GET: lmPendingApplications
        public ActionResult Index()
        {
            string userID = "";
            List<sLeaveModel> RetrievedApplications = new List<sLeaveModel>();

            //to get the id of the person logged in 
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (c != null)
                {
                    userID = c.Value;
                }
            }

            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            string queryString = "SELECT dbo.Leave.Leave_Application_ID, dbo.Leave.Employee_ID, " + 
                "dbo.Leave.Start_Date, dbo.Leave.End_Date, dbo.Leave.Reporting_Back_Date, " +
                "dbo.Leave.Leave_ID" + 
                "FROM dbo.Leave " +
                "FULL JOIN dbo.Employee " +
                "ON dbo.Leave.Employee_ID = dbo.Employee.Employee_ID " +
                "FULL JOIN dbo.Department " +
                "ON dbo.Employee.Department_ID = dbo.Department.Department_ID " +
                "WHERE dbo.Department.Line_Manager_ID = '" + userID + "' " +
                "AND dbo.Leave.Leave_ID IS NOT NULL ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            int lid = (int)reader[0];
                            int eid = (int)reader[1];
                            RetrievedApplications.Add(new sLeaveModel
                            {
                                leaveID = lid.ToString(), //from reader0
                                employeeID = eid.ToString(), //from reader1
                                startDate = (DateTime)reader[2], //from reader2
                                endDate = (DateTime)reader[3], //from reader3

                            });
                        }
                    }
                }
            }
            

            /*Get the list of applications due for the line manager to approve*/

            return View(RetrievedApplications);
        }

        private string GetLeaveStatus(int statusID)
        {
            string statusInString = "";
            switch(statusID)
            {
                case 0:
                    statusInString = "PendingLM";
                    break;
                case 1:
                    statusInString = "PendingHR";
                    break;
                case 2:
                    statusInString = "Approved";
                    break;
                case 3:
                    statusInString = "RejectedLM";
                    break;
                case 4:
                    statusInString = "RejectedHR";
                    break;
                case 5:
                    statusInString = "Cancelled";
                    break;
                case 6:
                    statusInString = "secondLMPending";
                    break;
                case 7:
                    statusInString = "secondLMRejected";
                    break;
            }

            return statusInString;
        }

        [HttpPost]
        public ActionResult Index(string Id)
        {
            return Index();
        }

        [HttpGet]
        public ActionResult Approve(string Id)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Reject(string Id)
        {
            return View();
        }

    }
}