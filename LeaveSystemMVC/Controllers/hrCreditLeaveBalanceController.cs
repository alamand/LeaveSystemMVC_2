using System;
using System.Collections.Generic;
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
        public ActionResult Credit(string value)
        {
            if (ModelState.IsValid)
            {
                ModelState.AddModelError("Save", "Save Button Clicked");
                return RedirectToAction("Index");
            }
            return View("Index");
        }
        
    }
}