using LeaveSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class RandomPasswordController : Controller
    {
        // GET: RandomPassword
        [HttpGet]
        public ActionResult RandomPassword()
        {
            return View();
        }

        // POST: RandomPassword
        [HttpPost]
        public ActionResult RandomPassword(LeaveSystemMVC.Models.RandomPassword model)
        {
            System.Diagnostics.Debug.WriteLine(LeaveSystemMVC.Models.RandomPassword.Generate(7, 7));
            return View();
        }
    }
}