using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrEmployeeLeaveHistoryController : ControllerBase
    {
        // GET: hrEmployeeLeaveHistory
        public ActionResult Index()
        {
            // @TODO: ADD FILTERS BY DEPARTMENT, EMPLOYEE and DATE (START-END).

            var model = new List<sLeaveModel>();
            List<sLeaveModel> leaveList = GetLeaveModel();

            foreach (var leave in leaveList)
            {
                if (!leave.leaveStatusName.Equals("Pending_LM") && !leave.leaveStatusName.Equals("Pending_HR"))
                    model.Add(leave);
            }
            
            return View(model);
        }
    }
}