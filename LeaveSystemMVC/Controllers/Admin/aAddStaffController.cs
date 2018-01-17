using System.Linq;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;
using System.Collections.Generic;

namespace LeaveSystemMVC.Controllers
{
    public class aAddStaffController : ControllerBase
    {
        // GET: aAddStaff
        [HttpGet]
        public ActionResult Index()
        {
            // employee model that will be passed into the view
            sEmployeeModel EmptyEmployee = new sEmployeeModel();

            // sets ViewData for dropdowns
            SetViewDatas();

            return View(EmptyEmployee);
        }

        [HttpPost]
        public ActionResult Index(sEmployeeModel emp)
        {
            // checks if the entered staff ID or Username already exists
            if(IsStaffExist(emp))
            {
                // sets ViewData for dropdowns
                SetViewDatas();

                return View(emp);
            }

            AddEmployee(emp);               // adds data to dbo.Employee table
            AddReportingTo(emp);            // adds data to dbo.Reporting table
            AddEmploymentPeriod(emp);       // adds data to dbo.Employment_Period table
            AddEmployeeRole(emp);           // adds data to dbo.Employee_Role table

            //Todo: calculate and credit the leave balances instead of routing to hrEditBalance below.

            ViewBag.SuccessMessage = emp.firstName + " " + emp.lastName + " has been successfully added.";
            ModelState.Clear();

            return Index();
        }

        private void SetViewDatas()
        {
            // gets and stores to the ViewData all available Departments from the DB and adds a default key of 0 for de-selecting 
            ViewData["DepartmentList"] = AddDefaultToDictionary(DBDepartmentList(), 0, "- Select Department -");

            // gets and stores to the ViewData all available Line Managers from the DB and adds a default key of 0 for de-selecting 
            ViewData["LineManagerList"] = AddDefaultToDictionary(DBLineManagerList(), 0, "- Select Line Manager -");

            // gets and stores to the ViewData all available Nationalities from the DB
            ViewData["NationalityList"] = DBNationalityList();

            // gets and stores to the ViewData all available Religions from the DB
            ViewData["ReligionList"] = DBReligionList();

            // gets True or False if the DB contains a staff with an HRR role
            ViewData["HRRExists"] = IsHRResponsibleExist();
        }

        private void AddEmployee(sEmployeeModel emp)
        {
            // basic information insertion
            var queryString = "INSERT INTO dbo.Employee (Employee_ID, First_Name, Last_Name, User_Name, Password, " +
                "Designation, Email, Gender, PH_No, Account_Status, Nationality_ID, Religion_ID, Date_Of_Birth, Probation) " +
                "VALUES('" + emp.staffID + "', '" + emp.firstName + "', '" + emp.lastName + "', '" + emp.userName.ToLower() + "', '" + emp.firstName.ToLower() + "', '" + 
                emp.designation + "', '" + emp.email + "', '" + emp.gender + "', '" + emp.phoneNo  + "', 'True', '" + emp.nationalityID + "', '" + 
                emp.religionID + "', '" + emp.dateOfBirth.ToString("yyyy-MM-dd") + "', 'True')";
            DBExecuteQuery(queryString);

            // updates the employee's record if a department was selected or de-selected
            if (emp.deptID != 0 && emp.deptID != null)
                queryString = "UPDATE dbo.Employee SET Department_ID = " + emp.deptID + " WHERE Employee_ID = " + emp.staffID;
            else
                queryString = "UPDATE dbo.Employee SET Department_ID = NULL WHERE Employee_ID = " + emp.staffID;
            DBExecuteQuery(queryString);

            // updates the employee's record if a reporting to line manager was selected or de-selected
            if (emp.reportsToLineManagerID != 0 && emp.reportsToLineManagerID != null)
                queryString = "UPDATE dbo.Employee SET Reporting_ID = " + emp.reportsToLineManagerID + " WHERE Employee_ID = " + emp.staffID;
            else
                queryString = "UPDATE dbo.Employee SET Reporting_ID = NULL WHERE Employee_ID = " + emp.staffID;
            DBExecuteQuery(queryString);

        }

        private void AddReportingTo(sEmployeeModel emp)
        {
            // @TODO: need to fix the dates
            var queryString = "INSERT INTO dbo.Reporting (Employee_ID, Reporting_ID, Start_Date) " +
                "VALUES('" + emp.staffID + "', '" + emp.reportsToLineManagerID + "', '" + emp.empStartDate.ToString("yyyy-MM-dd") + "')";
            DBExecuteQuery(queryString);
        }

        private void AddEmploymentPeriod(sEmployeeModel emp)
        {
            var queryString = "INSERT INTO dbo.Employment_Period (Employee_ID, Emp_Start_Date) " +
                "VALUES('" + emp.staffID + "', '" + emp.empStartDate.ToString("yyyy-MM-dd") + "')";
            DBExecuteQuery(queryString);
        }

        private void AddEmployeeRole(sEmployeeModel emp)
        {
            if (emp.isStaff)
            {
                int staffRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "Staff").Key;
                DBInsertRole((int)emp.staffID, staffRoleID);
            }

            if (emp.isLM)
            {
                int lmRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "LM").Key;
                DBInsertRole((int)emp.staffID, lmRoleID);
            }

            if (emp.isHR)
            {
                int hrRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "HR").Key;
                DBInsertRole((int)emp.staffID, hrRoleID);
            }

            if (emp.isHRResponsible)
            {
                int hrrRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "HR_Responsible").Key;
                DBInsertRole((int)emp.staffID, hrrRoleID);
            }

            if (emp.isAdmin)
            {
                int adminRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "Admin").Key;
                DBInsertRole((int)emp.staffID, adminRoleID);
            }
        }

        private bool IsStaffExist(sEmployeeModel emp)
        {
            bool isExist = false;
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            var queryString = "SELECT Employee_ID, User_Name " +
                "FROM dbo.Employee " +
                "WHERE User_Name = '" + emp.userName + "' OR Employee_ID = '" + emp.staffID + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((int)reader["Employee_ID"] == emp.staffID)
                        {
                            ModelState.AddModelError("staffIDInString", "Staff ID already exists.");
                            isExist = true;
                        }
                        if (reader["User_Name"].ToString().Equals(emp.userName))
                        {
                            ModelState.AddModelError("userName", "Username already exists.");
                            isExist = true;
                        }
                    }
                }
                connection.Close();
            }
            return isExist;
        }

    }
}
