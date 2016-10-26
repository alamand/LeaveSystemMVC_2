using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class LApplicationController : Controller
    {
        // GET: LApplication
        public ActionResult Index()
        {
            return View();
        }
    }
}