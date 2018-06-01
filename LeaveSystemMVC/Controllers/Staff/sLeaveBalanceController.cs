using System;
using System.Web.Mvc;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class sLeaveBalanceController : BaseController
    {
        // GET: sLeaveBalance
        public ActionResult Index()
        {
            int loginID = GetLoggedInID();
            var model = GetLeaveBalanceModel(loginID);
            var emp = GetEmployeeModel(loginID);
            var pilgrimageAllowed = IsPilgrimageAllowed(loginID);

            ViewData["Gender"] = emp.gender;
            ViewData["Pilgrimage"] = pilgrimageAllowed;

            return View(model);
        }

        private bool IsPilgrimageAllowed(int empID)
        {
            Employee emp = GetEmployeeModel(empID);
            List<Employee> employmentList = GetEmploymentPeriod(empID);

            // gets the latest employment period.
            Employee latestEmployment = employmentList[employmentList.Count - 1];
            TimeSpan diff = DateTime.Today - latestEmployment.empStartDate;
            double years = diff.TotalDays / 365.25;

            Dictionary dic = new Dictionary();
            if (dic.GetReligion()[emp.religionID].Equals("Muslim") && years >= 5)
                return true;
            else
                return false;
        }

    }
}