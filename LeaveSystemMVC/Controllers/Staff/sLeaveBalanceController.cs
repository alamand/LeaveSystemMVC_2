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
            var model = GetLeaveBalanceModel(GetLoggedInID());
            var emp = GetEmployeeModel(GetLoggedInID());

            ViewData["Gender"] = emp.gender;
            ViewData["Religion"] = DBReligionList()[emp.religionID];

            return View(model);
        }
    }
}