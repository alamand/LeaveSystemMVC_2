﻿using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrAdministerEmploymentController : BaseController
    {
        // GET: hrAdministerEmployment
        public ActionResult Index(int selectedEmployee = 0)
        {
            Dictionary dic = new Dictionary();

            SetMessageViewBags();

            // gets and stores to the ViewData all available Employees from the DB and adds a default key of 0 for de-selecting 
            ViewData["EmployeeList"] = dic.AddDefaultToDictionary(dic.GetStaff(), 0, "- Select Employee -");

            // this sets the default selection, or the user selection from the dropdown
            ViewData["selectedEmployee"] = selectedEmployee;

            // did the user select an employee from the dropdown list?
            if (selectedEmployee != 0)
            {
                // create a list of employment periods (note that this is with the same employee/staff ID)
                List<Employee> employmentList = GetEmploymentPeriod(selectedEmployee);

                // does these employment periods ends with an End Date, in other words, does the last employment period has an End Date?
                // if yes, then create a new empty employment period for the user to fill in the Start Date
                // else, the user will have to fill in the End Date for the latest Start Date
                if (IsEndDateExist(employmentList))
                {
                    Employee emp = new Employee();
                    emp.staffID = selectedEmployee;
                    employmentList.Add(emp);
                }

                // note that the user is allowed to enter only one date at a time, either a start date or end date.
                // this will be detrmined in the view code.
                // if the latest employment period has a start date and not an end date, then the user will be able to enter an end date for that employment period.
                // else if the latest employment period already has both start and end date, then the user will be able to enter a new start date.
                return View(employmentList);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(List<Employee> employmentList)
        {
            Dictionary dic = new Dictionary();
            DataBase db = new DataBase();

            // gets the latest employment period.
            Employee latestEmployment = employmentList[employmentList.Count - 1];

            // checks if the start date for this employment period already exist in DB.
            // true if the DB has the Start Date and not the End Date
            // false if the DB does not have any record for this staff ID with this Start Date
            bool startDateExist = IsStartDateExist(latestEmployment.staffID, latestEmployment.empStartDate);

            // if the DB does not have a record for this Start Date, and the Start Date is entered by the user.
            if (!startDateExist && !latestEmployment.empStartDate.ToShortDateString().Equals("01/01/0001"))
            {
                // is the entered start date at least one day after the previous end date?
                if (latestEmployment.empStartDate > employmentList[employmentList.Count-2].empEndDate)
                {
                    if (InsertEmploymentStartDate(latestEmployment.staffID, latestEmployment.empStartDate))
                    {

                        db.AddEmploymentPeriodSchedule((int)latestEmployment.staffID, latestEmployment.empStartDate, true);
                        TempData["SuccessMessage"] = "You have successfully updated the employment period for <b>" + dic.GetEmployee()[(int)latestEmployment.staffID] + "</b>.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Something went wrong. Please try again or contact technical support.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to set an employment period. The selected Start Date must be <b>after</b> the previous End Date. <br/>" +
                        "The previous <b>End Date</b> was on <b>" + employmentList[employmentList.Count - 2].empEndDate.ToShortDateString() + "</b>, where the selected <b>Start Date</b> is on <b>" + latestEmployment.empStartDate.ToShortDateString() + "</b>.";
                }

                return RedirectToAction("Index", new { selectedEmployee = (int)latestEmployment.staffID });
            }
            // if the DB does have a record for this End Date, and the EnD Date is entered by the user.
            else if (startDateExist && !latestEmployment.empEndDate.ToShortDateString().Equals("01/01/0001"))
            {
                // is the entered end date at least one day after the start date?
                if (latestEmployment.empEndDate > latestEmployment.empStartDate)
                {
                    if (UpdateEmploymentEndDate(latestEmployment.staffID, latestEmployment.empStartDate, latestEmployment.empEndDate))
                    {
                        db.AddEmploymentPeriodSchedule((int)latestEmployment.staffID, latestEmployment.empEndDate, false);
                        TempData["SuccessMessage"] = "You have successfully updated employment period for <b>" + dic.GetEmployee()[(int)latestEmployment.staffID] + "</b>.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Something went wrong. Please try again or contact technical support.";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to set an employment period. The selected End Date must be <b>after</b> the Start Date. <br/>" +
                        "The <b>Start Date</b> was on <b>" + latestEmployment.empStartDate.ToShortDateString() + "</b>, where the selected <b>End Date</b> is on <b>" + latestEmployment.empStartDate.ToShortDateString() + "<b/>.";
                }

                return RedirectToAction("Index", new { selectedEmployee = (int)latestEmployment.staffID });
            }
            else
            {
                // gets and stores to the ViewData all available Employees from the DB and adds a default key of 0 for de-selecting 
                ViewData["EmployeeList"] = dic.AddDefaultToDictionary(dic.GetEmployee(), 0, "- Select Employee -");

                // this sets the default selection, or the user selection from the dropdown
                ViewData["selectedEmployee"] = latestEmployment.staffID;
            }

            return View(employmentList);
        }

        [HttpPost]
        public ActionResult Select(FormCollection form)
        {
            // gets the selected employee ID and passes it to Index to display the information
            int id = Convert.ToInt32(form["selectedEmployee"]);
            return RedirectToAction("Index", new { selectedEmployee = id });
        }

        private bool InsertEmploymentStartDate(int? empID, DateTime startDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@staffID", SqlDbType.Int).Value = (int)empID;
            cmd.Parameters.Add("@startDate", SqlDbType.NChar).Value = startDate.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Employment_Period (Employee_ID, Emp_Start_Date) VALUES (@empID, @startDate)";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool UpdateEmploymentEndDate(int? empID, DateTime startDate, DateTime endDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;
            cmd.Parameters.Add("@startDate", SqlDbType.NChar).Value = startDate.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@endDate", SqlDbType.NChar).Value = endDate.ToString("yyyy-MM-dd");
            cmd.CommandText = "UPDATE dbo.Employment_Period SET Emp_End_Date = @endDate WHERE Employee_ID = @empID AND Emp_Start_Date = @startDate";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool IsStartDateExist(int? empID, DateTime startDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;
            cmd.Parameters.Add("@startDate", SqlDbType.NChar).Value = startDate.ToString("yyyy-MM-dd");
            cmd.CommandText = "SELECT COUNT(*) FROM dbo.Employment_Period WHERE Employee_ID = @empID AND Emp_Start_Date = @startDate AND Emp_End_Date IS NULL";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        private bool IsEndDateExist(List<Employee> employmentList)
        {
            bool isExist = false;
            DateTime defaultDate = new DateTime(0001, 01, 01);

            foreach (var employment in employmentList)
            {
                isExist = false;
                isExist = (employment.empEndDate != defaultDate) ? true : false;
            }

            return isExist;
        }
    }
}