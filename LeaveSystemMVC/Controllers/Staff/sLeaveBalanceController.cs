using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class sLeaveBalanceController : ControllerBase
    {
        // GET: sLeaveBalance
        public ActionResult Index()
        {
            int loginID = GetLoggedInID();
            var model = GetLeaveBalanceModel(loginID);
            var emp = GetEmployeeModel(loginID);
            var pilgrimageAllowed = IsPilgrimageAllowed(loginID);

            ViewData["Gender"] = emp.gender;
            ViewData["Pilgrimage"] = pilgrimageAllowed;

            return View(model);
        }
    }
}