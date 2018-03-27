/*
 * Author: M Hamza Rahimy
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class aEditStaffController : ControllerBase
    {
        // GET: aEditStaff
        public ActionResult Index(int selectedEmployee = 0)
        {
            // sets ViewData for dropdowns
            SetViewDatas((int)selectedEmployee);

            if (selectedEmployee != 0)
            {
                // selected employee model that will be passed into the view
                sEmployeeModel emp = GetEmployeeModel(selectedEmployee);
                return View(emp);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(sEmployeeModel emp)
        {
            UpdateEmployee(emp);        // updates data in dbo.Employee table
            UpdateEmployeeRole(emp);    // updates data in dbo.Employee_Role table

            // sets ViewData for dropdowns
            SetViewDatas((int)emp.staffID);

            /*Construct notification e-mail only if the username has been changed*/

            ViewBag.SuccessMessage = emp.firstName + " " + emp.lastName + " has been successfully updated.";
            ModelState.Clear();

            return View(emp);
        }

        [HttpPost]
        public ActionResult Select(FormCollection form)
        {
            // gets the selected employee ID and passes it to Index to display the information
            int id = Convert.ToInt32(form["selectedEmployee"]);
            return RedirectToAction("Index", new { selectedEmployee = id });
        }

        private void SetViewDatas(int selectedEmployee)
        {
            // gets and stores to the ViewData all available Departments from the DB and adds a default key of 0 for de-selecting 
            ViewData["DepartmentList"] = AddDefaultToDictionary(DBDepartmentList(), 0, "- Select Department -");

            // gets and stores to the ViewData all available Line Managers from the DB and adds a default key of 0 for de-selecting 
            ViewData["LineManagerList"] = AddDefaultToDictionary(DBLineManagerList(), 0, "- Select Line Manager -");

            // gets and stores to the ViewData all available Nationalities from the DB
            ViewData["NationalityList"] = DBNationalityList();

            // gets and stores to the ViewData all available Religions from the DB
            ViewData["ReligionList"] = DBReligionList();

            // gets True or False if the DB contains a staff with a HRR role
            ViewData["HRRExists"] = IsHRResponsibleExist();

            // gets True or False if the DB contains 2 staffs with an Admin role
            ViewData["AdminExists"] = IsAdminExist(2);

            // gets and stores to the ViewData all available Employees from the DB and adds a default key of 0 for de-selecting 
            ViewData["EmployeeList"] = AddDefaultToDictionary(DBEmployeeList(), 0, "- Select Employee -");

            // this sets the default selection, or the user selection from the dropdown
            ViewData["selectedEmployee"] = selectedEmployee;
        }

        private void UpdateEmployee(sEmployeeModel emp)
        {
            // basic information update
            var queryString = "UPDATE dbo.Employee SET " +
                "First_Name = '" + emp.firstName + "' , " +
                "Last_Name = '" + emp.lastName + "' , " +
                "User_Name = '" + emp.userName + "' , " +
                "Password = '" + emp.firstName.ToLower() + "' , " +
                "Designation = '" + emp.designation + "' , " +
                "Email = '" + emp.email + "' , " +
                "Gender = '" + emp.gender + "' , " +
                "PH_No = '" + emp.phoneNo + "' , " +
                "Account_Status = '" + emp.accountStatus + "' , " +
                "Date_Of_Birth = '" + emp.dateOfBirth.ToString("yyyy-MM-dd") + "' , " +
                "Nationality_ID = '" + emp.nationalityID + "' , " +
                "Religion_ID = '" + emp.religionID + "' " +
                "WHERE Employee_ID = " + emp.staffID;
            DBExecuteQuery(queryString);

            // updates the employee's record if a department was selected or de-selected
            if (emp.deptID != 0 && emp.deptID != null)
                queryString = "UPDATE dbo.Employee SET Department_ID = " + emp.deptID + " WHERE Employee_ID = " + emp.staffID;
            else
                queryString = "UPDATE dbo.Employee SET Department_ID = NULL WHERE Employee_ID = " + emp.staffID;
            DBExecuteQuery(queryString);

            // updates the employee's record if a reporting to line manager was selected or de-selected
            if (!IsReportingToExist(emp))
            {
                queryString = "UPDATE dbo.Reporting SET End_Date = SYSDATETIME() WHERE End_Date IS NULL AND Employee_ID = " + emp.staffID;
                DBExecuteQuery(queryString);
                if (emp.reportsToLineManagerID != 0)
                {
                    queryString = "INSERT INTO dbo.Reporting VALUES (" + emp.reportsToLineManagerID + "," + emp.staffID + ",SYSDATETIME(),NULL,NULL)";
                    DBExecuteQuery(queryString);
                }
            } 
            else
            {
                queryString = "INSERT INTO dbo.Reporting VALUES (" + emp.reportsToLineManagerID + "," + emp.staffID + ",SYSDATETIME(),NULL,NULL)";
                DBExecuteQuery(queryString);
            }
        }

        private void UpdateEmployeeRole(sEmployeeModel emp)
        {
            // updates roles based on the role(s) checkboxes 
            UpdateRole(emp.isStaff, (int)emp.staffID, "Staff");
            UpdateRole(emp.isLM, (int)emp.staffID, "LM");
            UpdateRole(emp.isHR, (int)emp.staffID, "HR");
            UpdateRole(emp.isHRResponsible, (int)emp.staffID, "HR_Responsible");
            UpdateRole(emp.isAdmin, (int)emp.staffID, "Admin");
        }

        private void UpdateRole(bool isChecked, int empID, string role)
        {
            // gets the role ID based on the role name
            int roleID = DBRoleList().FirstOrDefault(obj => obj.Value == role).Key;

            if (isChecked && !IsEmployeeRoleExist(empID, roleID))
                DBInsertRole(empID, roleID);
            else if (!isChecked && IsEmployeeRoleExist(empID, roleID))
                DBDeleteRole(empID, roleID);
        }

        private bool IsReportingToExist(sEmployeeModel emp)
        {
            bool isExist = false;
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var queryString = "SELECT Employee_ID FROM dbo.Reporting WHERE Employee_ID = " + emp.staffID + " AND Report_To_ID = " + emp.reportsToLineManagerID + " AND " +
                "Start_Date <= SYSDATETIME() AND End_Date > SYSDATETIME()";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        isExist = true;
                    }
                }
                connection.Close();
            }

            return isExist;
        }

        public bool IsEmployeeRoleExist(int empID, int roleID)
        {
            bool isExist = false;
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var queryString = "SELECT * FROM dbo.Employee_Role WHERE Employee_ID = " + empID + " AND Role_ID = " + roleID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        isExist = true;
                    }
                }
                connection.Close();
            }
            return isExist;
        }

        protected void DBDeleteRole(int empID, int roleID)
        {
            string deleteQuery = "DELETE FROM dbo.Employee_Role WHERE Employee_ID = " + empID + " AND Role_ID = " + roleID;
            DBExecuteQuery(deleteQuery);
        }
    }
}