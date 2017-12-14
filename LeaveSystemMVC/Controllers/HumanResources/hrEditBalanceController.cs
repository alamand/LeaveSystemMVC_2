using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrEditBalanceController : ControllerBase
    {
        // GET: hrEditBalance
        public ActionResult Index(int filterDepartmentID = 0, int filterAccStatus = -1, string filterSearch = "")
        {
            var model = new List<Models.hrEmpLeaveBalModel>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Gets the employee's id and name
            string queryString = GetFilteredQuery(filterDepartmentID, filterAccStatus, filterSearch);

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);      // retrieve employee id, first and last name
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // iterate through all employees in the database and add them all to the list
                    while (reader.Read())
                    {
                        var empBal = new hrEmpLeaveBalModel();
                        empBal.employee = GetEmployeeModel((int)reader["Employee_ID"]);
                        empBal.leaveBalance = GetLeaveBalanceModel((int)reader["Employee_ID"]);
                        empBal.religionString = DBReligionList()[empBal.employee.religionID];

                        model.Add(empBal);
                    }
                }
                connection.Close();
            }

            ViewData["DepartmentList"] = AddDefaultToDictionary(DBDepartmentList(), 0, "All Departments");
            ViewData["AccountStatusList"] = AccountStatusList();
            ViewData["SelectedDepartment"] = filterDepartmentID;
            ViewData["SelectedAccStatus"] = filterAccStatus;
            ViewData["EnteredSearch"] = filterSearch;

            return View(model);
        }

        [HttpPost]
        public ActionResult Filter(FormCollection form)
        {
            int deptID = Convert.ToInt32(form["selectedDepartment"]);
            int accStatID = Convert.ToInt32(form["SelectedAccStatus"]);
            string search = form["enteredSearch"];
            return RedirectToAction("Index", new { filterDepartmentID = deptID, filterAccStatus = accStatID, filterSearch = search });
        }

        private string GetFilteredQuery(int deptID, int accStat, string search)
        {
            string queryString = "SELECT Employee_ID FROM dbo.Employee WHERE Probation='False'";

            // adds a filter query if a department is selected from the dropdown, note that 0 represents All Departments
            if (deptID > 0)
            {
                queryString += " AND Department_ID = " + deptID;
            }

            // adds a filter query if a account status is selected from the dropdown, note that -1 represents Active/InActive
            if (accStat >= 0)
            {
                queryString += " AND Account_Status = " + accStat;
            }

            // adds a filter query if search box contains character(s), note that 0 length means the search box is empty
            if (search.Length > 0)
            {
                queryString += " AND (Employee_ID LIKE '%" + search + "%' " +
                    "OR First_Name LIKE '%" + search + "%' " +
                    "OR Last_Name LIKE '%" + search + "%')";
            }

            return queryString;
        }

        public ActionResult Edit(int empID)
        {
            var emp = GetEmployeeModel(empID);
            ViewBag.name = emp.firstName + " " + emp.lastName;
            ViewBag.gender = emp.gender;
            ViewBag.religion = DBReligionList()[emp.religionID];

            var leaveBalance = GetLeaveBalanceModel(empID);

            return View(leaveBalance);
        }

        [HttpPost]
        public ActionResult Edit(sleaveBalanceModel lb)
        {
            if (ModelState.IsValid)
            {
                DBUpdateBalance(lb.empId, lb.annualID, lb.annual);
                DBUpdateBalance(lb.empId, lb.maternityID, lb.maternity);
                DBUpdateBalance(lb.empId, lb.sickID, lb.sick);
                DBUpdateBalance(lb.empId, lb.compassionateID, lb.compassionate);
                DBUpdateBalance(lb.empId, lb.daysInLieuID, lb.daysInLieu);
                DBUpdateBalance(lb.empId, lb.shortHoursID, lb.shortHours);
                DBUpdateBalance(lb.empId, lb.pilgrimageID, lb.pilgrimage);
                Response.Write("<script> alert('Success. The information has been updated.');</script>");
            }
            else
            {
                ModelState.AddModelError("errmsg", "Failed: An error occured. Please check your input and try again.");
            }

            return Edit(lb.empId);
        }

        private void DBUpdateBalance(int employeeID, int leaveID, decimal balance)
        {
            string insertQuery = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) VALUES('" + employeeID + "','" + leaveID + "','" + balance + "')";
            string updateQuery = "UPDATE dbo.Leave_Balance SET Balance = '" + balance + "' WHERE Leave_ID = '" + leaveID + "' AND Employee_ID = '" + employeeID + "'";
            string queryString = (!IsLeaveBalanceExists(employeeID, leaveID) && balance > 0) ? insertQuery : updateQuery;
            DBExecuteQuery(queryString);
        }
    }
}