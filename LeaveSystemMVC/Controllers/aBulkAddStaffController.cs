using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class aBulkAddStaffController : Controller
    {
        sEmployeeModel tempEmp = new sEmployeeModel();
        
        [HttpGet]
        // GET: aBulkAddStaff
        public ActionResult Index()
        {
            tempEmp.firstName = "File name here...";
            return View(tempEmp);
        }
        

        public ActionResult Upload(HttpPostedFileBase file)
        {
            return RedirectToAction("Index");
        }
    }
}