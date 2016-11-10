using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class aEditDepartmentController : Controller
    {
        // GET: aEditDepartment
        public ActionResult Index()
        {
            //this should not be hardcoded 
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

            List<string> plm = new List<string>();
            plm.Add("None");
            plm.Add("Sukhpreet Singh Sidhu");
            plm.Add("Bisha Sen");
            plm.Add("Mandy Northover");
            plm.Add("Dan Adkin");
            ViewBag.plm = plm;

            List<string> department = new List<string>();
            department.Add("None");
            department.Add("IT");
            department.Add("Academic Services");
            department.Add("HR");
            department.Add("Student Services");
            ViewBag.department = department;
            return View();
        }
    }
}