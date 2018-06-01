using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class aEditStaffController : BaseController
    {
        // GET: aEditStaff
        public ActionResult Index(int selectedEmployee = 0)
        {
            // sets ViewData for dropdowns
            SetViewDatas(selectedEmployee);

            if (selectedEmployee != 0)
            {
                // selected employee model that will be passed into the view
                Employee emp = GetEmployeeModel(selectedEmployee);
                return View(emp);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(Employee emp)
        {
            if (IsUserNameExist(emp.staffID, emp.userName))
            {
                ModelState.AddModelError("userName", "Username already exists.");
                SetViewDatas((int)emp.staffID);
                return View(emp);
            }

            UpdateEmployee(emp);        // updates data in dbo.Employee table
            UpdateReportingTo(emp.staffID, emp.reportsToLineManagerID);     // updates data in dbo.Reporting table
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
            Dictionary dic = new Dictionary();

            // gets and stores to the ViewData all available Departments from the DB and adds a default key of 0 for de-selecting 
            ViewData["DepartmentList"] = dic.AddDefaultToDictionary(dic.GetDepartment(), 0, "- Select Department -");               //@TODO: add Display_Name to dbo.Department and change dic.GetDepartmentName()

            // gets and stores to the ViewData all available Line Managers from the DB and adds a default key of 0 for de-selecting 
            ViewData["LineManagerList"] = dic.AddDefaultToDictionary(dic.GetLineManager(), 0, "- Select Line Manager -");

            // gets and stores to the ViewData all available Nationalities from the DB
            ViewData["NationalityList"] = dic.GetNationalityName();

            // gets and stores to the ViewData all available Religions from the DB
            ViewData["ReligionList"] = dic.GetReligionName();

            // gets True or False if the DB contains a staff with a HRR role
            ViewData["HRRExists"] = IsHRResponsibleExist();

            // gets True or False if the DB contains 2 staffs with an Admin role
            ViewData["AdminExists"] = IsAdminExist(2);

            // gets and stores to the ViewData all available Employees from the DB and adds a default key of 0 for de-selecting 
            ViewData["EmployeeList"] = dic.AddDefaultToDictionary(dic.GetEmployee(), 0, "- Select Employee -");

            // this sets the default selection, or the user selection from the dropdown
            ViewData["selectedEmployee"] = selectedEmployee;
        }

        private void UpdateEmployee(Employee emp)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@firstName", SqlDbType.NChar).Value = emp.firstName;
            cmd.Parameters.Add("@lastName", SqlDbType.NChar).Value = emp.lastName;
            cmd.Parameters.Add("@userName", SqlDbType.NChar).Value = emp.userName.ToLower();
            cmd.Parameters.Add("@designation", SqlDbType.NChar).Value = emp.designation;
            cmd.Parameters.Add("@email", SqlDbType.NChar).Value = emp.email;
            cmd.Parameters.Add("@gender", SqlDbType.NChar).Value = emp.gender;
            cmd.Parameters.Add("@phoneNo", SqlDbType.NChar).Value = emp.phoneNo;
            cmd.Parameters.Add("@nationalityID", SqlDbType.Int).Value = emp.nationalityID;
            cmd.Parameters.Add("@religionID", SqlDbType.Int).Value = emp.religionID;
            cmd.Parameters.Add("@dateOfBirth", SqlDbType.NChar).Value = emp.dateOfBirth.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@status", SqlDbType.Bit).Value = emp.accountStatus;
            cmd.CommandText = "UPDATE dbo.Employee SET First_Name = @firstName, Last_Name = @lastName, User_Name = @userName, " +
                "Designation =  @designation, Email = @email, Gender = @gender, PH_No = @phoneNo, Nationality_ID = @nationalityID , " +
                "Religion_ID = @religionID, Date_Of_Birth = @dateOfBirth, Account_Status = @status WHERE Employee_ID = " + emp.staffID;

            DataBase db = new DataBase();
            db.Execute(cmd);

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

            db.Execute(cmd);
        }

        private void UpdateReportingTo(int? empID, int? reportToID)
        {
            // updates the employee's record if a reporting to line manager was selected or de-selected
            if (!IsReportingToExist(empID, reportToID))
            {
                RemovePreviousReportingTo(empID);

                if (reportToID != 0)
                    AddNewReportingTo(empID, reportToID);
            }
        }

        private void UpdateEmployeeRole(Employee emp)
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
            Dictionary dic = new Dictionary();

            // gets the role ID based on the role name
            int roleID = dic.GetRole().FirstOrDefault(obj => obj.Value == role).Key;

            if (isChecked && !IsEmployeeRoleExist(empID, roleID))
                InsertRole(empID, roleID);
            else if (!isChecked && IsEmployeeRoleExist(empID, roleID))
                DeleteRole(empID, roleID);
        }

        private bool IsUserNameExist(int? empID, String userName)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (empID != null) ? empID : 0;
            cmd.Parameters.Add("@userName", SqlDbType.NChar).Value = userName;
            cmd.CommandText = "SELECT COUNT(*) FROM dbo.Employee WHERE User_Name = @userName AND Employee_ID != @empID";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        private bool IsReportingToExist(int? empID, int? reportToID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (empID != null) ? empID : 0;
            cmd.Parameters.Add("@reportToID", SqlDbType.Int).Value = (reportToID != null) ? reportToID : 0;
            cmd.CommandText = "SELECT * FROM dbo.Reporting WHERE Employee_ID = @empID AND Report_To_ID = @reportToID";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        private bool IsEmployeeRoleExist(int? empID, int roleID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (empID != null) ? empID : 0;
            cmd.Parameters.Add("@roleID", SqlDbType.Int).Value = roleID;
            cmd.CommandText = "SELECT * FROM dbo.Employee_Role WHERE Employee_ID = @empID AND Role_ID = @roleID";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        private bool IsHRResponsibleExist()
        {
            Dictionary dic = new Dictionary();
            int hrrRoleID = dic.GetRole().FirstOrDefault(obj => obj.Value == "HR_Responsible").Key;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@hrrRoleID", SqlDbType.Int).Value = hrrRoleID;
            cmd.CommandText = "SELECT * FROM dbo.Employee_Role WHERE Role_ID = @hrrRoleID";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        private bool IsAdminExist(int numOfAdmins = 1)
        {
            Dictionary dic = new Dictionary();
            int adminRoleID = dic.GetRole().FirstOrDefault(obj => obj.Value == "Admin").Key;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@adminRoleID", SqlDbType.Int).Value = adminRoleID;
            cmd.CommandText = "SELECT * FROM dbo.Employee_Role WHERE Role_ID = @adminRoleID";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);
            return (dataTable.Rows.Count >= numOfAdmins);
        }

        private void DeleteRole(int empID, int roleID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@roleID", SqlDbType.Int).Value = roleID;
            cmd.CommandText = "DELETE FROM dbo.Employee_Role WHERE Employee_ID = @empID AND Role_ID = @roleID";
            DataBase db = new DataBase();
            db.Execute(cmd);
        }

        private void InsertRole(int empID, int roleID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@roleID", SqlDbType.Int).Value = roleID;
            cmd.CommandText = "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) VALUES (@empID, @roleID) ";
            DataBase db = new DataBase();
            db.Execute(cmd);
        }

        private void RemovePreviousReportingTo(int? empID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;
            cmd.CommandText = "DELETE FROM dbo.Reporting WHERE Employee_ID = @empID";
            DataBase db = new DataBase();
            db.Execute(cmd);
        }

        private void AddNewReportingTo(int? empID, int? reportToID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = (int)empID;
            cmd.Parameters.Add("@reportToID", SqlDbType.Int).Value = (int)reportToID;
            cmd.CommandText = "INSERT INTO dbo.Reporting (Employee_ID, Report_To_ID) VALUES (@empID, @reportToID)";
            DataBase db = new DataBase();
            db.Execute(cmd);
        }
    }
}