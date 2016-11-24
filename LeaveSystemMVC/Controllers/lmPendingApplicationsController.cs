using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class lmPendingApplicationsController : Controller
    {
        // GET: lmPendingApplications
        public ActionResult Index()
        {
            List<sLeaveModel> RetrievedApplications = new List<sLeaveModel>
            {
                new sLeaveModel { leaveID = "5013" ,leaveType = "Annual" },
                new sLeaveModel {leaveID = "5024" ,leaveType = "Maternity" }
            };

            return View(RetrievedApplications);
        }
    }
}