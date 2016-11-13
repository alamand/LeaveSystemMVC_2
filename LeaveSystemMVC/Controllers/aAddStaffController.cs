using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class aAddStaffController : Controller
    {
        // GET: aAddStaff
        public ActionResult Index()
        {
            //all these lists should not be hardcoded

            List<string> sid = new List<string>();
            sid.Add("None");
            sid.Add("12345678");
            sid.Add("22345678");
            sid.Add("33345678");
            sid.Add("44445678");
            ViewBag.sid = sid;

            List<string> slm = new List<string>();
            slm.Add("None");
            slm.Add("Sukhpreet Singh Sidhu");
            slm.Add("Bidisha Sen");
            slm.Add("Mandy Northover");
            slm.Add("Dan Adkins");
            ViewBag.slm = slm;

            List<string> department = new List<string>();
            department.Add("None");
            department.Add("IT");
            department.Add("Academics");
            department.Add("HR");
            department.Add("Student Services");
            ViewBag.department = department;

            List<string> staffType = new List<string>();
            staffType.Add("None");
            staffType.Add("Admin");
            staffType.Add("Line  Manager");
            staffType.Add("HR");
            staffType.Add("Staff Member");
            ViewBag.staffType = staffType;
            return View();
        }

  
    }
}