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

            List<string> leaves = new List<string>() ;
            leaves.Add("Annual");
            leaves.Add("Sick");
            leaves.Add("Compassionate");
            leaves.Add("Maternity");
            leaves.Add("Short");
            ViewBag.leave = leaves;
            return View();
        }
    }
}