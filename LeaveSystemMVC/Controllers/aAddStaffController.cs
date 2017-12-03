﻿/*
 * Author: M Hamza Rahimy
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using LeaveSystemMVC.Models;
using System.Globalization;

namespace LeaveSystemMVC.Controllers
{
    public class aAddStaffController : Controller
    {
        // GET: aAddStaff
        [HttpGet]
        public ActionResult Index()
        {
            /*The employee model object that will be passed into the view*/
            sEmployeeModel EmptyEmployee = new sEmployeeModel();
            EmptyEmployee.deptId = 0;
            Dictionary<int, string> nonDisplayRoleOptions = new Dictionary<int, string>();
            //Intermediary staff roles/types selection list

            //Get list of available roles
            var connectionString = 
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            bool notDisplayHrResponsible = true;
            var queryString = "SELECT Employee_ID " +
                "FROM dbo.Employee_Role " +
                "FULL JOIN dbo.Role " +
                "ON dbo.Role.Role_ID = dbo.Employee_Role.Role_ID " +
                "WHERE dbo.Role.Role_Name = 'HR_Responsible' " + 
                "AND dbo.Employee_Role.Employee_ID IS NOT NULL";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    notDisplayHrResponsible = reader.HasRows;
                }
                connection.Close();
            }

            queryString = "SELECT Role_ID, Role_Name FROM dbo.Role";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        string readRole = (string)reader[1];
                        switch(readRole)
                        {
                            case "Admin":
                                nonDisplayRoleOptions.Add((int)reader[0], readRole);
                                break;
                            case "HR_Responsible":
                                if (notDisplayHrResponsible)
                                    nonDisplayRoleOptions.Add((int)reader[0], readRole);
                                else
                                    EmptyEmployee.staffTypeSelectionOptions.Add((int)reader[0], readRole);
                                break;
                            case "HR":
                                if(notDisplayHrResponsible)
                                    EmptyEmployee.staffTypeSelectionOptions.Add((int)reader[0], readRole);
                                else
                                    nonDisplayRoleOptions.Add((int)reader[0], readRole);
                                break;
                            case "Staff":
                                nonDisplayRoleOptions.Add((int)reader[0], readRole);
                                break;
                            default:
                                EmptyEmployee.staffTypeSelectionOptions.Add((int)reader[0], readRole);
                                break;
                        }
                    }
                }
                connection.Close();
            }

            //We should have all role types from the database now
            //end get roles

 

            //Get all departments for selection
            queryString = "SELECT Department_ID, Department_Name FROM dbo.Department";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        EmptyEmployee.departmentList.Add((int)reader[0], (string)reader[1]);
                    }
                }
                connection.Close();
            }
            //End get departments

            //No longer required
            //Get a list of names and ids of line manager employees
            //This will be used to select a secondary line manager for an employee
            /*queryString = "SELECT Employee.Employee_ID, First_Name, Last_Name " +
                              "FROM dbo.Employee " +
                              "FULL JOIN dbo.Employee_Role " +
                              "ON dbo.Employee.Employee_ID = dbo.Employee_Role.Employee_ID " +
                              "FULL JOIN dbo.Role " +
                              "ON dbo.Role.Role_ID = dbo.Employee_Role.Role_ID " +
                              "WHERE dbo.Role.Role_Name = 'LM' " +
                              "AND dbo.Employee.First_Name IS NOT NULL " +
                              "AND dbo.Employee.Last_Name IS NOT NULL " +
                              "AND dbo.Employee.Employee_ID IS NOT NULL";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        string fullName = (string)reader[1] + " " + (string)reader[2];
                        EmptyEmployee.SecondLMSelectionOptions.Add((int)reader[0], fullName);
                    }
                }
                connection.Close();
            }*/

            //Get all line managers for "Reports To"
            queryString = "SELECT dbo.Employee.Employee_ID, dbo.Employee.First_Name, dbo.Employee.Last_Name " +
                "FROM dbo.Employee, dbo.Employee_Role " +
                "WHERE dbo.Employee.Employee_ID = dbo.Employee_Role.Employee_ID AND dbo.Employee_Role.Role_ID = 4";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string fullName = (string)reader[1] + " " + (string)reader[2];
                            if (!EmptyEmployee.lineManagerSelectionOptions.ContainsKey((int)reader[0]))
                            {
                                EmptyEmployee.lineManagerSelectionOptions.Add((int)reader[0], fullName);
                            }
                        }
                    }
                }
                connection.Close();
            }
            //End get all line managers

            TempData["EmptyEmployee"] = EmptyEmployee;
            TempData["nonDisplayRoleOptions"] = nonDisplayRoleOptions;
            return View(EmptyEmployee);
        }

        
        /*The bind attribute simply excludes validation errors for the given fields in the model.
         Needed to add that because some the given non-required fields were giving "required" 
         validation errors.*/
        [HttpPost]
        public ActionResult Index(sEmployeeModel SE)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "";

            /*Redundant: yes but the rest of the implementation requires references to the staff ID
             and there was no effective way to enforce that the staff ID be 5 digits as an int*/
            int actualStaffID;
            int.TryParse(SE.staffIDInString, out actualStaffID);
            SE.staffID = actualStaffID;
            /////////////////////////////

            //Validations
            bool hasValidationErrors = false;
            //Check if username already exists
            queryString = "SELECT Employee_ID, User_Name FROM dbo.Employee WHERE dbo.Employee.User_Name = '" + SE.userName + "' OR dbo.Employee.Employee_ID = '" + SE.staffID + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        int id = (int)reader[0];
                        if(id == SE.staffID)
                        {
                            ModelState.AddModelError("staffID", "Staff ID already exists.");
                            hasValidationErrors = true;
                        }
                        string userName = (string)reader[1];
                        if(userName.Equals(SE.userName))
                        {
                            ModelState.AddModelError("userName", "Username already exists.");
                            hasValidationErrors = true;
                        }
                    }
                }
                connection.Close();
            }
            //End check if username exists.
            if(hasValidationErrors)
            {
                sEmployeeModel EmptyEmployee = (sEmployeeModel)TempData["EmptyEmployee"];
                SE.staffTypeSelectionOptions = EmptyEmployee.staffTypeSelectionOptions;
                SE.departmentList = EmptyEmployee.departmentList;
                //SE.SecondLMSelectionOptions = EmptyEmployee.SecondLMSelectionOptions;
                TempData["EmptyEmployee"] = EmptyEmployee;
                TempData["nonDisplayRoleOptions"] = TempData["nonDisplayRoleOptions"];
                return View(SE);
            }
            // End validations

            //Table insertions
            SE.password = RandomPassword.Generate(7, 7);
            
            //string dateTimeFormat = "d/MM/yyyy";
            string startDateString = SE.empStartDate.ToString("yyyy-MM-dd");
            //DateTime convertedStartDate = DateTime.ParseExact(startDateString, dateTimeFormat, new CultureInfo("en-CA"));

            /*Had to use deptname to store the actual department ID because for some 
             reason the view wouldn't store the value of the dropdown for department
             selection in the deptID int*/
            if (SE.deptName == null)
            {
                
                queryString = "INSERT INTO dbo.Employee (Employee_ID, First_Name, " +
                    "Last_Name, User_Name, Password, Designation, Email, Gender, PH_No, " +
                    "Account_Status) VALUES('" + SE.staffID +
                    "', '" + SE.firstName + "', '" + SE.lastName + "', '" + SE.userName +
                    "', '" + SE.password + "', '" + SE.designation + "', '" + SE.email +
                    "', '" + SE.gender + "', '" + SE.phoneNo + "', '" + "True)";
            }
            else
            {
                queryString = "INSERT INTO dbo.Employee (Employee_ID, First_Name, " +
                    "Last_Name, User_Name, Password, Designation, Email, Gender, PH_No, " +
                    "Account_Status, Department_ID) VALUES('" + SE.staffID +
                    "', '" + SE.firstName + "', '" + SE.lastName + "', '" + SE.userName +
                    "', '" + SE.password + "', '" + SE.designation + "', '" + SE.email +
                    "', '" + SE.gender + "', '" + SE.phoneNo + "', '" + "True" + "', '" + SE.deptName + "')";
            }
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }

            //Add employee's reporting line manager
            queryString = "INSERT INTO dbo.Reporting (Employee_ID, Reporting_ID, Start_Date) VALUES('" + SE.staffID +
                    "', '" + SE.reportsToLineManagerString + "', '" + SE.empStartDate.ToString("yyyy-MM-dd") + "')";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }

            queryString = "UPDATE dbo.Employee SET Reporting_ID = '" + SE.reportsToLineManagerString + "' WHERE Employee_ID = '" + SE.staffID + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }

            //Insert into the Employment_Period table
            queryString = "INSERT INTO dbo.Employment_Period (Employee_ID, Emp_Start_Date) VALUES('" + SE.staffID +
                   "', '" + SE.empStartDate.ToString("yyyy-MM-dd") + "')";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }

            /*We are now assuming that all roles except for admin are also a 
             staff member, so the staff member role will be hard coded */
            Dictionary<int, string> nonDisplayRoleOptions = (Dictionary<int, string>)TempData["nonDisplayRoleOptions"];
            if (SE.isAdmin)
            {
                int adminID = nonDisplayRoleOptions.FirstOrDefault(obj => obj.Value == "Admin").Key;
                queryString = "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                "VALUES ('" + SE.staffID + "', '" + adminID + "') ";
            }
            else
            {
                int staffRoleID = nonDisplayRoleOptions.FirstOrDefault(obj => obj.Value == "Staff").Key;
                bool toAddStaffType = true;
                bool toAddOptionalType = true;
                if (SE.staffType == null)
                    toAddStaffType = false;
                if (SE.optionalStaffType == null)
                    toAddOptionalType = false;
                if (SE.staffType != null && SE.staffType.Equals(SE.optionalStaffType))
                {
                    queryString = CreateRolesQuery(toAddStaffType, false, SE, staffRoleID);
                }
                else
                {
                    queryString = CreateRolesQuery(toAddStaffType, toAddOptionalType, SE, staffRoleID);
                }

            }

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }

            //Todo: calculate and credit the leave balances instead of routing to hrEditBalance below.

            //End table insertions

            ViewBag.SuccessMessage = SE.firstName + " " + SE.lastName + " has been successfully added to the database.";
            ModelState.Clear();
            /*Redirect the user to the hr edit balances page.*/
            return RedirectToAction("Index", "hrEditBalance");
            //return Index();
        }

        /*Construct a query string for inserting roles into the db
         *Will always insert the staff role, and staff role ID must be provided
         *Will insertion query to add the roles provided in the staffType and 
          optionalStaffType object if their corresponding bools are true.
         */
        private string CreateRolesQuery(bool toAddStaffType, bool toAddOptionalType, sEmployeeModel employeeObject, int staffRoleID)
        {
            string queryString = "";
            if(toAddStaffType)
            {
                queryString += "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                    "VALUES ('" + employeeObject.staffID + "', '" + employeeObject.staffType + "') ";
            }
            if(toAddOptionalType)
            {
                queryString += "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                    "VALUES ('" + employeeObject.staffID + "', '" + employeeObject.optionalStaffType + "') ";
            }
            queryString += "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                "VALUES ('" + employeeObject.staffID + "', '" + staffRoleID + "') ";
            return queryString;
        }

        /*Function referred to from @ http://nimblegecko.com/using-simple-drop-down-lists-in-ASP-NET-MVC/
         * Essentially encapsulates the creation of a list of SelectItem. Or a SelectList object.
         * Please note that this function is not being used because a better solution has replaced 
          it's specific use for creating dropdown lists out of dictionary objects. However, this might
          become useful later(possibly in other projects). This (especially the link) will be kept here
          essentially for future reference because it helped clear up a lot of confusions.*/
        private IEnumerable<SelectListItem> GetDropdownListItems(Dictionary<int, string> elements)
        {
            var selectList = new List<SelectListItem>();

            foreach(var element in elements)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.Key.ToString(),
                    Text = element.Value
                });
            }

            return selectList;
        }
    }
}
