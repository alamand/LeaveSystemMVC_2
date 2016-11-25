using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Security.Claims;

namespace LeaveSystemMVC.Controllers
{
    public class lmPendingApplicationsController : Controller
    {
        [HttpGet]
        // GET: lmPendingApplications
        public ActionResult Index()
        {
            List<sLeaveModel> RetrievedApplications = new List<sLeaveModel>
            {
                new sLeaveModel { leaveID = "5013" ,leaveType = "Annual" },
                new sLeaveModel {leaveID = "5024" ,leaveType = "Maternity" }
            };

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