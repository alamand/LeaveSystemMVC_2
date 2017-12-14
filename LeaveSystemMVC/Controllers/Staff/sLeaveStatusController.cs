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
    public class sLeaveStatusController : ControllerBase
    {
        // GET: sLeaveStatus
        public ActionResult Index()
        {
            var model = new List<sLeaveModel>();
            List<sLeaveModel> leaveList = GetLeaveModel("Employee.Employee_ID", GetLoggedInID());
            foreach (var leave in leaveList)
            {
                if (leave.leaveStatusName.Equals("Pending_HR") || leave.leaveStatusName.Equals("Pending_LM"))
                    model.Add(leave);
            }

            return View(model);
        }

        public ActionResult Delete(int id) {

            string queryString = "UPDATE Leave SET Leave_Status_ID= '5' WHERE Leave_Application_ID = '"+id+"'";
            DBExecuteQuery(queryString);
            Response.Write("<script> alert('Your leave application has been cancelled.')</script>");

            return RedirectToAction("Index");
        }
    }
}