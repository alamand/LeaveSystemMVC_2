using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;


namespace LeaveSystemMVC.Controllers
{
    public class hrCreditLeaveBalanceController : ControllerBase
    {
        // GET: hrCreditLeaveBalance
        public ActionResult Index()
        {
            var ActiveEmployees = DBEmployeeList(1);
            List<sleaveBalanceModel> empBalance = new List<sleaveBalanceModel>();
            foreach (int empID in ActiveEmployees.Keys)
            {
                empBalance.Add(GetLeaveBalanceModel(empID));
            }

            ViewBag.employeeList = ActiveEmployees;
            ViewBag.defaultBalance = GetLeaveBalanceModel();
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            return View(empBalance);
        }

        [HttpPost]
        public ActionResult Index(List<sleaveBalanceModel> model)
        {
            sleaveBalanceModel defaultBal = GetLeaveBalanceModel();

            foreach (var empBal in model)
            {
                UpdateLeaveBalance(empBal.empId, empBal.daysInLieuID, empBal.daysInLieu);
                UpdateLeaveBalance(empBal.empId, empBal.annualID, empBal.annual);
                UpdateLeaveBalance(empBal.empId, defaultBal.compassionateID, defaultBal.compassionate);
                UpdateLeaveBalance(empBal.empId, defaultBal.sickID, defaultBal.sick);

                if (GetEmployeeModel(empBal.empId).gender == 'F')
                    UpdateLeaveBalance(empBal.empId, defaultBal.maternityID, defaultBal.maternity);
            }

            TempData["SuccessMessage"] = "Leave balances has been updated.";

            return RedirectToAction("Index");
        }

        private void UpdateLeaveBalance(int empID, int leaveID, decimal balance)
        {
            string queryString = "";
            if (IsLeaveBalanceExists(empID, leaveID))
                queryString = "UPDATE dbo.Leave_Balance SET Balance = " + balance + ", Last_Edit_Comment = 'Leave quota per annum' WHERE Employee_ID = " + empID + " AND Leave_ID = " + leaveID;
            else
                queryString = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_ID, Balance, Last_Edit_Comment) VALUES(" + empID + "," + leaveID + "," + balance + ",'Leave quota per annum')";
            DBExecuteQuery(queryString);
        }
        
    }
}