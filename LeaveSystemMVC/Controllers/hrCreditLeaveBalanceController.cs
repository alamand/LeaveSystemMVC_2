using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class hrCreditLeaveBalanceController : Controller
    {
        // GET: hrCreditLeaveBalance
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(int? value)
        {
            if (ModelState.IsValid)
            {
             //Something
            }
            return View();
        }

    }
}