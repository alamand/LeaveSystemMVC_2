using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrProbationAdministrationController : BaseController
    {
        // GET: hrProbationAdministration
        public ActionResult Index()
        {
            // holds the list of employees who are on probation
            var onProbation = new List<Employee>();

            // select all employees who are on probation
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT e.Employee_ID, First_Name, Last_Name, Department_Name, DATEDIFF(day,Emp_Start_Date,GETDATE()) as Dif_Date, Emp_Start_Date, Emp_End_Date, Probation " +
                "FROM dbo.Employee e, dbo.Employment_Period p, dbo.Department d " +
                "WHERE e.Employee_ID = p.Employee_ID AND e.Department_ID = d.Department_ID AND (Probation=1 OR Probation IS NULL) AND Emp_Start_Date <= GETDATE()" +
                "ORDER BY Emp_Start_Date";

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                var employee = new Employee();
                employee.staffID = (int)row["Employee_ID"];
                employee.firstName = (string)row["First_Name"];
                employee.lastName = (string)row["Last_Name"];
                ViewData["Department" + employee.staffID.ToString()] = (string)row["Department_Name"];
                employee.empStartDate = (DateTime)row["Emp_Start_Date"];
                ViewData["Duration" + employee.staffID.ToString()] = (int)row["Dif_Date"];
                onProbation.Add(employee);
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewData["OnProbation"] = onProbation;

            return View();
        }

        // called when a user confirms a probation for a specific staff
        public ActionResult SetProbation(string id = null)
        {
            if (id.Equals(null))
                return RedirectToAction("Index");

            int empID = int.Parse(id);

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.CommandText = "UPDATE dbo.Employee SET Probation = 0 WHERE Employee_ID = @empID";

            DataBase db = new DataBase();
            db.Execute(cmd);

            // calculate balance
            SetBalance(empID);

            Employee emp = GetEmployeeModel(empID);
            TempData["SuccessMessage"] = "<b>" + emp.firstName + " " + emp.lastName + "</b> is now off probation.";

            return RedirectToAction("Index");
        }

        // sets the default balances for staffs who are confirmed off probation
        private void SetBalance(int empID)
        {
            // holds default durations for all leave types
            Balance leaveTypes = GetLeaveBalanceModel();
            // hold this staff member's details
            Employee employee = GetEmployeeModel(empID);

            double annualBalance = 0;
            
            // calculate annual leave balance where each month the staff recieves 1.833 annual credit
            // gets the date when the staff started
            DateTime startDate = GetStartDate(empID);

            if (startDate.Year == DateTime.Now.Year)
            {
                // gets the last date of the year
                DateTime endYearDate = new DateTime(startDate.Year, 12, 31);

                // ((Total days of employement duration + remaining days of the years) / days in a month) * 1.833
                // and round it, e.g.: 1.0=1.0, 1.2=1.0, 1.3=1.5, 1.6=1.5, 1.8=2.0, 2.2=2.0 and so on... 
                annualBalance = Math.Round((endYearDate.Subtract(startDate).TotalDays / 30) * ((double)leaveTypes.annual/12), MidpointRounding.AwayFromZero);

                // can not exceed more than 22 days
                annualBalance = (annualBalance > (double)leaveTypes.annual) ? (double)leaveTypes.annual : annualBalance;
            }
            else
            {   
                // as it is a new year, and the staff is employeed the year before
                // he or she should get all the credit for this year
                annualBalance = (double)leaveTypes.annual;
            }

            UpdateLeaveBalance(empID, leaveTypes.annualID, (decimal)annualBalance);
            UpdateLeaveBalance(empID, leaveTypes.sickID, leaveTypes.sick);
            UpdateLeaveBalance(empID, leaveTypes.compassionateID, 0);

            Dictionary dic = new Dictionary();

            if (dic.GetReligion()[employee.religionID].Equals("Muslim"))
                UpdateLeaveBalance(empID, leaveTypes.pilgrimageID, leaveTypes.pilgrimage);

            if (employee.gender == 'F')
                UpdateLeaveBalance(empID, leaveTypes.maternityID, leaveTypes.maternity);

            UpdateLeaveBalance(empID, leaveTypes.shortHoursID, leaveTypes.shortHours);
        }

        private void UpdateLeaveBalance(int empID, int leaveTypeID, decimal balance)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@leaveTypeID", SqlDbType.Int).Value = leaveTypeID;
            cmd.Parameters.Add("@balance", SqlDbType.Decimal).Value = balance;

            if (IsLeaveBalanceExist(empID, leaveTypeID))
                cmd.CommandText = "UPDATE dbo.Leave_Balance SET Balance = @balance WHERE Employee_ID = @empID AND Leave_Type_ID = @leaveTypeID";
            else
                cmd.CommandText = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_Type_ID, Balance) Values(@empID, @leaveTypeID, @balance)";

            DataBase db = new DataBase();
            db.Execute(cmd);
        }

        // gets the staffs start date from the database
        // note that it does not check for returning staff (assuming returning staffs will never be on probation)
        private DateTime GetStartDate(int empID)
        {
            DateTime date = new DateTime();

            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.CommandText = "SELECT Employee.Employee_ID, Emp_Start_Date FROM dbo.Employee, dbo.Employment_Period " +
                "WHERE Employee.Employee_ID = Employment_Period.Employee_ID AND Employee.Employee_ID = @empID";

            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                date = (DateTime)row["Emp_Start_Date"];
            }

            return date;
        }
    }
}