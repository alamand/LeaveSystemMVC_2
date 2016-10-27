using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class HolidaysCalendarController : Controller
    {
        // GET: HolidaysCalendar
        public ActionResult Index()
        {
            return View();
        }
    }
}