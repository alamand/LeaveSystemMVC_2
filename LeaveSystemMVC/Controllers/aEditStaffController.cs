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
    public class aEditStaffController : Controller
    {
        /*
         * Look into @ http://stackoverflow.com/questions/32949510/add-items-to-select-list-on-the-client-side-in-mvc-5-asp
         */
        // GET: aEditStaff
        [HttpGet]
        public ActionResult Index()
        {
            string empID = TempData["EmpID"] as string;
            sEmployeeModel EmptyEmployee = new sEmployeeModel();
            /*Get employee details*/

            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            
            //In case an employee has multiple employment dates, get the one with the latest start date from the Employment_Period table.
            string queryString = "SELECT dbo.Employment_Period.Emp_Start_Date " +
               "FROM dbo.Employment_Period " +
               "WHERE dbo.Employment_Period.Employee_ID = '" + empID + "'";
            DateTime tempStartDate = DateTime.MinValue;

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
                            if((DateTime)reader[0] > tempStartDate)
                            {
                                tempStartDate = (DateTime)reader[0];
                            }
                        }
                    }
                }
                connection.Close();
            }
            
            queryString = "SELECT dbo.Employment_Period.Emp_Start_Date, dbo.Employment_Period.Emp_End_Date " +
               "FROM dbo.Employment_Period " +
               "WHERE dbo.Employment_Period.Employee_ID = '" + empID + "' AND dbo.Employment_Period.Emp_Start_Date = '"
               + tempStartDate.ToString("yyyy-MM-dd") + "'";

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
                            EmptyEmployee.empStartDate = (DateTime)reader[0];
                            System.Diagnostics.Debug.WriteLine("Setting empStartDate to: " + (DateTime)reader[0]);
                            EmptyEmployee.empOldStartDate = (DateTime)reader[0]; //For detecting changes to the start date.
                            if (reader[1] != DBNull.Value)
                            {
                                System.Diagnostics.Debug.WriteLine("Setting empEndDate to: " + (DateTime)reader[1]);
                                EmptyEmployee.empEndDate = (DateTime)reader[1];
                                EmptyEmployee.empOldEndDate = (DateTime)reader[1]; //For detecting changes to the end date.
                            }                    
                        }
                    }
                }
                connection.Close();
            }

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


            queryString = "SELECT Employee_ID, First_Name, Last_Name, " +
                "Gender, Ph_No, Email, User_Name, Designation, dbo.Employee.Department_ID, " +
                "Account_Status, Reporting_ID " + 
                "FROM dbo.Employee " + 
                "WHERE dbo.Employee.Employee_ID = '" + empID + "' " +
                "AND dbo.Employee.Employee_ID IS NOT NULL";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            EmptyEmployee.staffID = (int)reader[0];
                            EmptyEmployee.staffIDInString = EmptyEmployee.staffID.ToString();
                            EmptyEmployee.firstName = (string)reader[1];
                            EmptyEmployee.lastName = (string)reader[2];
                            string gender = (string)reader[3];
                            EmptyEmployee.gender = gender[0];
                            if (reader[4] != DBNull.Value)
                            {
                                EmptyEmployee.phoneNo = (string)reader[4];
                            }
                            EmptyEmployee.email = (string)reader[5];
                            EmptyEmployee.userName = (string)reader[6];
                            EmptyEmployee.designation = (string)reader[7];
                            if (reader[8] != DBNull.Value)
                            {
                                var read = reader[8];
                                EmptyEmployee.deptId = (int)reader[8];
                                EmptyEmployee.deptName = EmptyEmployee.deptId.ToString();
                            }
                            else
                                EmptyEmployee.deptName = "";
                            EmptyEmployee.accountStatus = (bool)reader[9];
                            if (reader[10] != DBNull.Value)
                            {                                
                                EmptyEmployee.reportsToLineManagerString = ((int)reader[10]).ToString();
                            }
                        }
                    }
                }
                connection.Close();
            }

            Dictionary<int, string> nonDisplayRoleOptions = new Dictionary<int, string>();
            //Intermediary staff roles/types selection list

            //Get list of available roles
            bool notDisplayHrResponsible = true;
            queryString = "SELECT Employee_ID " +
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

            queryString = "SELECT dbo.Role.Role_ID, Role_Name " +
                "FROM dbo.Role " +
                "FULL JOIN dbo.Employee_Role " +
                "ON dbo.Role.Role_ID = dbo.Employee_Role.Role_ID " +
                "WHERE dbo.Employee_Role.Employee_ID = '" + EmptyEmployee.staffIDInString + "' " +
                "AND dbo.Employee_Role.Employee_ID IS NOT NULL";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                int itr = 0;
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        while(reader.Read())
                        {
                            string rn = (string)reader[1];
                            if(rn.Equals("Admin"))
                            {
                                EmptyEmployee.isAdmin = true;
                                break;
                            }
                            if(itr == 0)
                            {
                                int rv = (int)reader[0];
                                EmptyEmployee.staffType = rv.ToString();
                                itr++;
                                continue;
                            }
                            if(itr == 1)
                            {
                                int rv = (int)reader[0];
                                EmptyEmployee.optionalStaffType = rv.ToString();
                                break;
                            }
                        }
                    }
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
                    while (reader.Read())
                    {
                        string readRole = (string)reader[1];
                        switch (readRole)
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
                                if (notDisplayHrResponsible)
                                    EmptyEmployee.staffTypeSelectionOptions.Add((int)reader[0], readRole);
                                else
                                    nonDisplayRoleOptions.Add((int)reader[0], readRole);
                                break;
                            case "Staff":
                                nonDisplayRoleOptions.Add((int)reader[0], readRole);
                                break;
                            case "LM":
                                nonDisplayRoleOptions.Add((int)reader[0], readRole);
                                EmptyEmployee.staffTypeSelectionOptions.Add((int)reader[0], readRole);
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
                    while (reader.Read())
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
                              "AND dbo.Employee.Employee_ID != '" + EmptyEmployee.staffIDInString + "' " +
                              "AND dbo.Employee.First_Name IS NOT NULL " +
                              "AND dbo.Employee.Last_Name IS NOT NULL " +
                              "AND dbo.Employee.Employee_ID IS NOT NULL";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    
                    while (reader.Read())
                    {
                        string fullName = (string)reader[1] + " " + (string)reader[2];
                        EmptyEmployee.SecondLMSelectionOptions.Add((int)reader[0], fullName);
                    }
                }
                connection.Close();
            }*/

            //TempData["Gender"] = EmptyEmployee.gender;
            TempData["EmptyEmployee"] = EmptyEmployee;
            TempData["nonDisplayRoleOptions"] = nonDisplayRoleOptions;
            return View(EmptyEmployee);
        }

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
                    sEmployeeModel tempEmp = (sEmployeeModel)TempData["EmptyEmployee"];
                    while (reader.Read())
                    {
                        int id = (int)reader[0];
                        if (tempEmp.staffID != id && id == SE.staffID)
                        {
                            ModelState.AddModelError("staffID", "Staff ID already exists.");
                            hasValidationErrors = true;
                        }
                        string userName = (string)reader[1];
                        
                        if (!userName.Equals(tempEmp.userName) && userName.Equals(SE.userName))
                        {
                            ModelState.AddModelError("userName", "Username already exists.");
                            hasValidationErrors = true;
                        }
                    }
                }
                //End check if username exists.


                /*Check if the employee is the line manager of a department
                 *Here, I can easily retrieve the department ID and use that to
                 check if the department ID is assigned to at least one staff member.
                 But I have not implemented this because it's not clear what I would
                 or would not do if or if not there is a staff member associated with the
                 department*/
                queryString = "SELECT Line_Manager_ID " +
                    "FROM dbo.Department " +
                    "WHERE dbo.Department.Line_Manager_ID = '" + SE.staffID +"' " + 
                    "AND dbo.Department.Line_Manager_ID IS NOT NULL";
                command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                {
                    if(reader.HasRows)
                    {
                        if(SE.accountStatus == false)
                        {
                            ModelState.AddModelError("accountStatus", "Cannot deactivate a department's Line Manager.\r\n Please replace the Line Manager first.");
                            hasValidationErrors = true;
                        }
                        //Not required
                        /*if(SE.isAdmin)
                        {
                            ModelState.AddModelError("isAdmin", "This employee is an active Line Manager of a department. Administrator users do not have the functionalities of a Line Manager.");
                            hasValidationErrors = true;
                        }*/
                        /*Check if user tried to change the LM Role of an active LM of a department
                         *Normally it's okay to deactivate a person with the LM role if they are not 
                         the active LM of a department.*/
                        Dictionary<int, string> lmCheckRoleOptions = (Dictionary<int, string>)TempData["nonDisplayRoleOptions"];
                        int lmID = lmCheckRoleOptions.FirstOrDefault(obj => obj.Value == "LM").Key;
                        sEmployeeModel lmCheckModel = (sEmployeeModel)TempData["EmptyEmployee"];
                        string lsr;
                        string rsr;
                        string losr;
                        string rosr;
                        if (SE.staffType == null)
                            lsr = "";
                        else
                            lsr = SE.staffType;
                        if (lmCheckModel.staffType == null)
                            rsr = "";
                        else
                            rsr = lmCheckModel.staffType;
                        if (SE.optionalStaffType == null)
                            losr = "";
                        else
                            losr = SE.optionalStaffType;
                        if (lmCheckModel.optionalStaffType == null)
                            rosr = "";
                        else
                            rosr = lmCheckModel.optionalStaffType;

                        if (lmCheckModel.staffType == lmID.ToString())
                        {
                            if (!lsr.Equals(rsr) && !losr.Equals(rsr))
                            {
                                ModelState.AddModelError("staffType", "The employee is the active line manager of a department. \r\n Please replace the Line Manager first.");
                                hasValidationErrors = true;
                            }
                        }
                        if (lmCheckModel.optionalStaffType == lmID.ToString())
                        {
                            if(!losr.Equals(rosr) && !lsr.Equals(rosr))
                            {
                                ModelState.AddModelError("optionalStaffType", "The employee is the active line manager of a department. \r\n Please replace the Line Manager first.");
                                hasValidationErrors = true;
                            }
                        }
                    }
                }
                
                connection.Close();
            }

            int result = DateTime.Compare(SE.empEndDate, SE.empStartDate);
            if (result < 0)
            {
                ModelState.AddModelError("empEndDate", "The end date cannot be earlier than the start date.");
                hasValidationErrors = true;
            }

            /*Make sure the selection lists for departments, roles
             and the non-display role options are persisted. And then redirect to
             back to the view.*/
            if (hasValidationErrors)
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
            //string secondLMtext = "";
            //string secondLmValueText = "";

            //Update the Employment_Period table
            //Start date has been modified, so create a new entry in the Employment_Period table
            if (!SE.empStartDate.Equals(SE.empOldStartDate))
            {
                queryString = "INSERT INTO dbo.Employment_Period (Employee_ID, Emp_Start_Date, Emp_End_Date) VALUES('" + SE.staffID +
                   "', '" + SE.empStartDate.ToString("yyyy-MM-dd") + "', '" + SE.empEndDate.ToString("yyyy-MM-dd") + "')";

                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(queryString, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                        connection.Close();
                }
            }
            //An employment end date has been added or the employment end date has been modified and the start date is unmodified
            else if ((SE.empOldEndDate == DateTime.MinValue && SE.empEndDate != DateTime.MinValue) || !SE.empEndDate.Equals(SE.empOldEndDate))
            {
                queryString = "UPDATE dbo.Employment_Period SET Emp_End_Date = '" + SE.empEndDate.ToString("yyyy-MM-dd") +
                "' WHERE dbo.Employment_Period.Employee_ID = '" + SE.staffID +
                "' AND dbo.Employment_Period.Emp_Start_Date = '" + SE.empOldStartDate.ToString("yyyy-MM-dd") + "'";

                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(queryString, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                        connection.Close();
                }
            }

            //Update the Employee table
            if (SE.deptName == null)
            {
                queryString = "UPDATE dbo.Employee SET First_Name = '" + SE.firstName + "' , Last_Name = '" + SE.lastName +
                    "' , User_Name = '" + SE.userName + "' , Designation = '" + SE.designation + "' , Email = '" + SE.email + 
                    "' , Gender = '" + SE.gender + "' , Ph_No = '" + SE.phoneNo + "' , Account_Status = '" + SE.accountStatus + 
                    "' WHERE dbo.Employee.Employee_ID = '" + SE.staffID + "'";
            }
            else
            {
                queryString = "UPDATE dbo.Employee SET First_Name = '" + SE.firstName + "' , Last_Name = '" + SE.lastName + 
                    "' , User_Name = '" + SE.userName + "' , Designation = '" + SE.designation + "' , Email = '" + SE.email + 
                    "' , Gender = '" + SE.gender + "' , Ph_No = '" + SE.phoneNo + "' , Account_Status = '" + SE.accountStatus + 
                    "' , Department_ID = '" + SE.deptName + "' WHERE dbo.Employee.Employee_ID = '" + SE.staffID + "'";
            }
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }

            //Update employee's reporting line manager
            queryString = "UPDATE dbo.Reporting SET Reporting_ID = '" + SE.reportsToLineManagerString + "' WHERE Employee_ID = '" + SE.staffID + "'";

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

            /*Clear the employee's roles before adding the updated ones.
             Easiest way to make sure that only the updated roles remain.*/
            queryString = "DELETE FROM dbo.Employee_Role " +
                "WHERE dbo.Employee_Role.Employee_ID = '" + SE.staffID + "' ";
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

            //End Insertions

            /*Construct notification e-mail only if the username has been changed*/

            TempData["EmpID"] = SE.staffIDInString;            
            string successMessage = "The details of " + SE.firstName + " " + SE.lastName + " have been edited.";
            TempData["SuccessMessage"] = successMessage;
            return RedirectToAction("Index");
        }

        /*Construct a query string for inserting roles into the db
         *Will always insert the staff role, and staff role ID must be provided
         *Will insertion query to add the roles provided in the staffType and 
          optionalStaffType object if their corresponding bools are true.
         */
        private string CreateRolesQuery(bool toAddStaffType, bool toAddOptionalType, sEmployeeModel employeeObject, int staffRoleID)
        {
            string queryString = "";
            if (toAddStaffType)
            {
                queryString += "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                    "VALUES ('" + employeeObject.staffID + "', '" + employeeObject.staffType + "') ";
            }
            if (toAddOptionalType)
            {
                queryString += "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                    "VALUES ('" + employeeObject.staffID + "', '" + employeeObject.optionalStaffType + "') ";
            }
            queryString += "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                "VALUES ('" + employeeObject.staffID + "', '" + staffRoleID + "') ";
            return queryString;
        }

        [HttpGet]
        public ActionResult Select()
        {
            minStaff model = new minStaff();
            Dictionary<int, string> EmployeeList = new Dictionary<int, string>();
            var connectionString =
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            string queryString = "SELECT Employee_ID, First_Name, Last_Name " +
                "FROM dbo.Employee " +
                "WHERE dbo.Employee.First_Name IS NOT NULL ";
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
                            EmployeeList.Add((int)reader[0], fullName);
                        }
                    }
                }
                connection.Close();
            }
            ViewData["EmployeeList"] = EmployeeList;
            return View();
        }

        [HttpPost]
        public ActionResult Select(minStaff empMod)
        {
            if(empMod.empIDAsString != null)
            {
                TempData["EmpID"] = empMod.empIDAsString;
                return RedirectToAction("Index");
            }
            return Select();
        }

    }
}