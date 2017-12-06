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

            ViewData["DepartmentList"] = DBDepartmentList();
            ViewData["RoleList"] = AvailableRoles();
            ViewData["LineManagerList"] = DBLineManagerList();
            ViewData["NationalityList"] = DBNationalityList();
            ViewData["ReligionList"] = DBReligionList();

            return View(EmptyEmployee);
        }

        [HttpPost]
        public ActionResult Index(sEmployeeModel emp)
        {
            int actualStaffID;
            int.TryParse(emp.staffIDInString, out actualStaffID);
            emp.staffID = actualStaffID;

            if(IsStaffExist(emp))
            {
                ViewData["DepartmentList"] = DBDepartmentList();
                ViewData["RoleList"] = AvailableRoles();
                ViewData["LineManagerList"] = DBLineManagerList();
                ViewData["NationalityList"] = DBNationalityList();
                ViewData["ReligionList"] = DBReligionList();
                return View(emp);
            }

            AddEmployee(emp);
            AddReportingTo(emp);
            AddEmploymentPeriod(emp);
            AddEmployeeRole(emp);

            //Todo: calculate and credit the leave balances instead of routing to hrEditBalance below.

            ViewBag.SuccessMessage = emp.firstName + " " + emp.lastName + " has been successfully added.";
            ModelState.Clear();

            return Index();
        }

        private bool IsStaffExist(sEmployeeModel emp)
        {
            bool isExist = false;

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var queryString = "SELECT Employee_ID, User_Name " +
                "FROM dbo.Employee " +
                "WHERE Employee.User_Name = '" + emp.userName + "' OR Employee.Employee_ID = '" + emp.staffID + "'";

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

        private void AddEmployee(sEmployeeModel emp)
        {
            var queryString = "INSERT INTO dbo.Employee (Employee_ID, First_Name, Last_Name, User_Name, " +
                "Password, Designation, Email, Gender, PH_No, Account_Status, Date_Of_Birth, Probation) " +
                "VALUES('" + emp.staffID + "', '" + emp.firstName + "', '" + emp.lastName + "', '" + emp.userName +
                "', '" + emp.password + "', '" + emp.designation + "', '" + emp.email + "', '" + emp.gender + 
                "', '" + emp.phoneNo + "', 'True', '" + emp.empDateOfBirth.ToString("yyyy-MM-dd") + "', 'True')";

            DBExecuteQuery(queryString);

            if (emp.deptName != null)
            {
                queryString = "UPDATE dbo.Employee SET Department_ID = " + emp.deptName + " WHERE Employee_ID = " + emp.staffID;
                DBExecuteQuery(queryString);
            }

            if (emp.reportsToLineManagerString != null)
            {
                queryString = "UPDATE dbo.Employee SET Reporting_ID = " + emp.reportsToLineManagerString + " WHERE Employee_ID = " + emp.staffID;
                DBExecuteQuery(queryString);
            }

            if (emp.religionString != null)
            {
                queryString = "UPDATE dbo.Employee SET Religion_ID = " + emp.religionString + " WHERE Employee_ID = " + emp.staffID;
                DBExecuteQuery(queryString);
            }

            if (emp.nationalityString != null)
            {
                queryString = "UPDATE dbo.Employee SET Nationality_ID = " + emp.nationalityString + " WHERE Employee_ID = " + emp.staffID;
                DBExecuteQuery(queryString);
            }
        }

        private void AddReportingTo(sEmployeeModel emp)
        {
            var queryString = "INSERT INTO dbo.Reporting (Employee_ID, Reporting_ID, Start_Date) " +
                "VALUES('" + emp.staffID + "', '" + emp.reportsToLineManagerString + "', '" + emp.empStartDate.ToString("yyyy-MM-dd") + "')";
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
            if (emp.isAdmin)
            {
                int adminRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "Admin").Key;
                DBInsertRole(emp.staffID, adminRoleID);
            }
            else
            {
                int staffRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "Staff").Key;
                DBInsertRole(emp.staffID, staffRoleID);

                bool isStaffType = (emp.staffType != null) ? true : false;
                bool isOptionalType = (emp.optionalStaffType != null) ? true : false;

                if (isStaffType)
                {
                    staffRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == emp.staffType).Key;
                    DBInsertRole(emp.staffID, staffRoleID);
                }

                if (isOptionalType && !emp.staffType.Equals(emp.optionalStaffType))
                {
                    staffRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == emp.optionalStaffType).Key;
                    DBInsertRole(emp.staffID, staffRoleID);
                }
            }
        }

        private Dictionary<int, string> AvailableRoles()
        {
            Dictionary<int, string> roleList = new Dictionary<int, string>();
            foreach (var entry in DBRoleList())
            {
                if (entry.Value.Equals("LM") || entry.Value.Equals("HR") || (!HRResponsibleExists() && entry.Value.Equals("HR_Responsible")))
                {
                    roleList.Add(entry.Key, entry.Value);
                }
            }

            return roleList;
        }

        private bool HRResponsibleExists()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            bool isExist = true;
            var queryString = "SELECT Employee_ID" +
                " FROM dbo.Employee_Role, dbo.Role" +
                " WHERE Role.Role_ID = Employee_Role.Role_ID" +
                " AND Role.Role_Name = 'HR_Responsible'" +
                " AND Employee_Role.Employee_ID IS NOT NULL";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    isExist = reader.HasRows;
                }
                connection.Close();
            }

            return isExist;
        }
    }
}
