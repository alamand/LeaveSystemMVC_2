using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class aAddStaffController : BaseController
    {
        // GET: aAddStaff
        [HttpGet]
        public ActionResult Index()
        {
            // employee model that will be passed into the view
            Employee EmptyEmployee = new Employee();

            // sets ViewData for dropdowns
            SetViewDatas();

            return View(EmptyEmployee);
        }

        [HttpPost]
        public ActionResult Index(Employee emp)
        {
            // checks if the entered staff ID or Username already exists
            if(IsStaffExist((int)emp.staffID, emp.userName))
            {
                SetViewDatas();
                return View(emp);
            }

            if (!AddEmployee(emp))
            {
                ViewBag.ErrorMessage = emp.firstName + " " + emp.lastName + " was NOT added.<br/> Something went wrong in Employee Information.";
                ClearEmployee(emp.staffID);
                SetViewDatas();
                return View(emp);
            }


            if (!AddEmployeeRole(emp))
            {
                ViewBag.ErrorMessage = emp.firstName + " " + emp.lastName + " was NOT added.<br/> Something went wrong in Employee Role.";
                ClearEmployee(emp.staffID);
                SetViewDatas();
                return View(emp);
            }

            if (!AddReportingTo(emp.staffID, emp.reportsToLineManagerID))
            {
                ViewBag.ErrorMessage = emp.firstName + " " + emp.lastName + " was NOT added.<br/> Something went wrong in Reporting To.";
                ClearEmployee(emp.staffID);
                SetViewDatas();
                return View(emp);
            }

            if (!AddEmploymentPeriod(emp.staffID, emp.empStartDate))
            {
                ViewBag.ErrorMessage = emp.firstName + " " + emp.lastName + " was NOT added.<br/> Something went wrong in Employment Period.";
                ClearEmployee(emp.staffID);
                SetViewDatas();
                return View(emp);
            }

            ViewBag.SuccessMessage = emp.firstName + " " + emp.lastName + " has been successfully added.";

            ModelState.Clear();
            return Index();
        }

        private void SetViewDatas()
        {
            Dictionary dic = new Dictionary();

            // gets and stores to the ViewData all available Departments from the DB and adds a default key of 0 for de-selecting 
            ViewData["DepartmentList"] = dic.AddDefaultToDictionary(dic.GetDepartment(), 0, "- Select Department -");               // @TODO: Change to GetDepartmentName()

            // gets and stores to the ViewData all available Line Managers from the DB and adds a default key of 0 for de-selecting 
            ViewData["LineManagerList"] = dic.AddDefaultToDictionary(dic.GetLineManager(), 0, "- Select Line Manager -");

            // gets and stores to the ViewData all available Nationalities from the DB
            ViewData["NationalityList"] = dic.GetNationalityName();

            // gets and stores to the ViewData all available Religions from the DB
            ViewData["ReligionList"] = dic.GetReligionName();

            // gets True or False if the DB contains a staff with an HRR role
            ViewData["HRRExists"] = IsHRResponsibleExist();
        }

        private bool AddEmployee(Employee emp)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@staffID", SqlDbType.Int).Value = (int)emp.staffID;
            cmd.Parameters.Add("@firstName", SqlDbType.NChar).Value = emp.firstName;
            cmd.Parameters.Add("@lastName", SqlDbType.NChar).Value = emp.lastName;
            cmd.Parameters.Add("@userName", SqlDbType.NChar).Value = emp.userName.ToLower();
            cmd.Parameters.Add("@password", SqlDbType.NChar).Value = emp.firstName.ToLower();
            cmd.Parameters.Add("@designation", SqlDbType.NChar).Value = emp.designation;
            cmd.Parameters.Add("@email", SqlDbType.NChar).Value = emp.email;
            cmd.Parameters.Add("@gender", SqlDbType.NChar).Value = emp.gender;
            cmd.Parameters.Add("@phoneNo", SqlDbType.NChar).Value = emp.phoneNo;
            cmd.Parameters.Add("@nationalityID", SqlDbType.Int).Value = emp.nationalityID;
            cmd.Parameters.Add("@religionID", SqlDbType.Int).Value = emp.religionID;
            cmd.Parameters.Add("@dateOfBirth", SqlDbType.NChar).Value = emp.dateOfBirth.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Employee (Employee_ID, First_Name, Last_Name, User_Name, Password, " +
                "Designation, Email, Gender, PH_No, Account_Status, Nationality_ID, Religion_ID, Date_Of_Birth, Probation) " +
                "VALUES(@staffID,@firstName,@lastName,@userName,@password,@designation,@email,@gender,@phoneNo,'True',@nationalityID, @religionID,@dateOfBirth,'True')";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);

            // updates the employee's record if a department was selected or de-selected
            cmd.Parameters.Clear();
            cmd.Parameters.Add("@staffID", SqlDbType.Int).Value = (int)emp.staffID;

            // true if the department is selected
            if (emp.deptID != 0 && emp.deptID != null)
            {
                cmd.Parameters.Add("@deptID", SqlDbType.Int).Value = (int)emp.deptID;
                cmd.CommandText = "UPDATE dbo.Employee SET Department_ID = @deptID WHERE Employee_ID = @staffID";
            }
            else
            {
                cmd.CommandText = "UPDATE dbo.Employee SET Department_ID = NULL WHERE Employee_ID = @staffID";
            }

            success = (success) ? db.Execute(cmd) : success;

            return success;
        }

        private bool AddReportingTo(int? empID, int? reportToID)
        {
            bool success = true;
            if (reportToID != 0 && reportToID != null)
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;
                cmd.Parameters.Add("@reportToID", SqlDbType.Int).Value = (int)reportToID;
                cmd.CommandText = "INSERT INTO dbo.Reporting (Employee_ID, Report_To_ID) VALUES(@empID, @reportToID)";
                DataBase db = new DataBase();
                success = db.Execute(cmd);
            }

            return success;
        }

        private bool AddEmploymentPeriod(int? empID, DateTime startDate)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (empID == null) ? null : empID;
            cmd.Parameters.Add("@startDate", SqlDbType.NChar).Value = startDate.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Employment_Period (Employee_ID, Emp_Start_Date) VALUES(@empID, @startDate)";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);
            return success;
        }

        private bool AddEmployeeRole(Employee emp)
        {
            Dictionary dic = new Dictionary();
            Dictionary<int, String> roleList = dic.GetRole();

            bool success = true;

            if (emp.isStaff)
            {
                int staffRoleID = roleList.FirstOrDefault(obj => obj.Value == "Staff").Key;
                success = (success) ? InsertRole(emp.staffID, staffRoleID) : success;
            }

            if (emp.isLM)
            {
                int lmRoleID = roleList.FirstOrDefault(obj => obj.Value == "LM").Key;
                success = (success) ? InsertRole(emp.staffID, lmRoleID) : success;
            }

            if (emp.isHR)
            {
                int hrRoleID = roleList.FirstOrDefault(obj => obj.Value == "HR").Key;
                success = (success) ? InsertRole(emp.staffID, hrRoleID) : success;
            }

            if (emp.isHRResponsible)
            {
                int hrrRoleID = roleList.FirstOrDefault(obj => obj.Value == "HR_Responsible").Key;
                success = (success) ? InsertRole(emp.staffID, hrrRoleID) : success;
            }

            if (emp.isAdmin)
            {
                int adminRoleID = roleList.FirstOrDefault(obj => obj.Value == "Admin").Key;
                success = (success) ? InsertRole(emp.staffID, adminRoleID) : success;
            }

            return success;
        }

        private bool InsertRole(int? empID, int roleID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;
            cmd.Parameters.Add("@roleID", SqlDbType.Int).Value = roleID;
            cmd.CommandText = "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) VALUES(@empID, @roleID)";
            DataBase db = new DataBase();
            bool success = db.Execute(cmd);

            return success;
        }
        
        private bool IsStaffExist(int? empID, String userName)
        {
            bool isExist = false;
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@userName", SqlDbType.NChar).Value = userName;
            cmd.Parameters.Add("@staffID", SqlDbType.Int).Value = (int)empID;
            cmd.CommandText = "SELECT Employee_ID, User_Name FROM dbo.Employee WHERE User_Name = @userName OR Employee_ID = @staffID";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                if ((int)row["Employee_ID"] == empID)
                {
                    ModelState.AddModelError("staffIDInString", "Staff ID already exists.");
                    isExist = true;
                }
                if (row["User_Name"].ToString().Equals(userName))
                {
                    ModelState.AddModelError("userName", "Username already exists.");
                    isExist = true;
                }
            }
                        
            return isExist;
        }

        private bool IsHRResponsibleExist()
        {
            Dictionary dic = new Dictionary();
            int hrrRoleID = dic.GetRole().FirstOrDefault(obj => obj.Value == "HR_Responsible").Key;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@hrrRoleID", SqlDbType.Int).Value = hrrRoleID;
            cmd.CommandText = "SELECT COUNT(*) FROM dbo.Employee_Role WHERE Role_ID = @hrrRoleID";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        private void ClearEmployee(int? empID)
        {
            if (empID != null && empID != 0)
            {
                DataBase db = new DataBase();
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;

                cmd.CommandText = "DELETE FROM dbo.Reporting WHERE Employee_ID = @empID";
                db.Execute(cmd);

                cmd.CommandText = "DELETE FROM dbo.Employment_Period WHERE Employee_ID = @empID";
                db.Execute(cmd);

                cmd.CommandText = "DELETE FROM dbo.Employee_Role WHERE Employee_ID = @empID";
                db.Execute(cmd);

                cmd.CommandText = "DELETE FROM dbo.Employee WHERE Employee_ID = @empID";
                db.Execute(cmd);
            }
        }
    }
}
