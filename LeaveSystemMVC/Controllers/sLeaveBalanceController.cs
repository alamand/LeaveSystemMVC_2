using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class sLeaveBalanceController : Controller
    {
        // GET: sLeaveBalance
        public ActionResult Index()
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);   
            ViewBag.claim = c;
            string a = c.ToString();
           // string s = a.Substring();
            return View();
        }
    }
}