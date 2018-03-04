using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class lmDaysInLieuController : ControllerBase
    {
        // GET: lmDaysInLieu
        public ActionResult Index(int selectedEmployee = 0)
        {
            Dictionary<int, string> lmSubordinates = new Dictionary<int, string>();
            lmSubordinates.Add(0, "- Select Employee -");
            var allEmployees = GetEmployeeModel();

            foreach (var emp in allEmployees)
            {
                if (emp.reportsToLineManagerID == GetLoggedInID())
                    lmSubordinates.Add((int)emp.staffID, emp.firstName + " " + emp.lastName);
            }

            ViewData["EmployeeList"] = lmSubordinates;
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

            if (selectedEmployee != 0)
            {
                hrDaysInLieu daysInLieu = new hrDaysInLieu();
                daysInLieu.employeeID = selectedEmployee;
                ViewData["selectedEmployee"] = selectedEmployee;

                return View(daysInLieu);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(hrDaysInLieu dil)
        {
            if (Request.Form["submit"] != null)
            {
                string queryString = "INSERT INTO dbo.Days_In_Lieu VALUES ('" + dil.employeeID + "' , '" + dil.date.ToString("yyyy-MM-dd") + "' , '" + dil.numOfDays + "' , '" + dil.comment + "')";
                DBExecuteQuery(queryString);

                //Check if DIL leave type exists for this employee.
                int dilID = DBLeaveTypeList().FirstOrDefault(obj => obj.Value == "DIL").Key;
                if (IsLeaveBalanceExists(dil.employeeID, dilID))
                {
                    queryString = "UPDATE dbo.Leave_Balance SET Balance = Balance + '" + dil.numOfDays + "' WHERE Employee_ID = '" + dil.employeeID + "' AND Leave_Type_ID = " + dilID;
                }
                else
                {
                    queryString = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_Type_ID, Balance) VALUES ('" + dil.employeeID + "' , '" + dilID + "' , '" + dil.numOfDays + "')";
                }
                DBExecuteQuery(queryString);

                sEmployeeModel emp = GetEmployeeModel(dil.employeeID);
                TempData["SuccessMessage"] = "<b>" + emp.firstName + " " + emp.lastName + "</b> has been credited successfully.";

                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Select(FormCollection form)
        {
            int id = Convert.ToInt32(form["selectedEmployee"]);
            return RedirectToAction("Index", new { selectedEmployee = id });
        }
    }
}