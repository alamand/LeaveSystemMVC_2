/*
 * Author: M Hamza Rahimy
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class aEditStaffController : Controller
    {
        // GET: aEditStaff
        [HttpGet]
        public ActionResult Index()
        {
            sEmployeeModel EmptyEmployee = new sEmployeeModel();
            List<minDepartment> tempDeps = new List<minDepartment>
            {
                new minDepartment {departmentID = 1, departmentName = "newDep" },
                new minDepartment {departmentID = 2, departmentName = "newDep1" }
            };
            ViewData["DepartmentList"] = tempDeps;
            //all these lists should not be hardcoded

            List<string> sid = new List<string>();
            sid.Add("None");
            sid.Add("1234567");
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
            return View(tempDeps);
        }

        [HttpPost]
        public ActionResult Index(sEmployeeModel SE)
        {
            return View(SE);
        }
    }
}