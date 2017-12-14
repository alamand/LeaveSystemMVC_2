using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class lmSubordinateLeaveHistoryController : ControllerBase
    {
        // GET: lmSubordinateLeaveHistory
        public ActionResult Index()
        {
            var model = new List<sLeaveModel>();
            List<sLeaveModel> leaveList = GetLeaveModel("Reporting.Reporting_ID", GetLoggedInID());

            foreach (var leave in leaveList)
            {
                if (!leave.leaveStatusName.Equals("Pending_LM") && !leave.leaveStatusName.Equals("Pending_HR"))
                    model.Add(leave);
            }
            
            return View(model);
        }

    }
}