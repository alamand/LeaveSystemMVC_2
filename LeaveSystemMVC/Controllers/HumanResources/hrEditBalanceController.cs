using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
                        var empBal = new Models.hrEmpLeaveBalModel();
                        empBal.employee = new Models.sEmployeeModel();
                        empBal.leaveBalance = new Models.sleaveBalanceModel();
                        empBal.employee.firstName = (string)reader["First_Name"];
                        empBal.employee.lastName = (string)reader["Last_Name"];
                        empBal.employee.staffID = (int)reader["Employee_ID"];
                        empBal.employee.gender = Convert.ToChar(reader["Gender"]);
                        empBal.employee.religionID = (!DBNull.Value.Equals(reader["Religion_ID"])) ? (int)reader["Religion_ID"] : 0;    // @TODO: remove DBNULL checker when the DB is filled accuretly
                        empBal.religionString = GetReligion((int)empBal.employee.staffID);

                        string queryString2 = "SELECT Balance,Leave_Name FROM dbo.Leave_Balance, dbo.Leave_Type " +
                            "WHERE Leave_Balance.Employee_ID = '" + empBal.employee.staffID + "' AND Leave_Balance.Leave_ID = Leave_Type.Leave_ID";

                        using (var connection2 = new SqlConnection(connectionString))
                        {
                            var command2 = new SqlCommand(queryString2, connection2);
                            connection2.Open();
                            using (var reader2 = command2.ExecuteReader())
                            {
                                while (reader2.Read())
                                {
                                    switch ((string)reader2["Leave_Name"])
                                    {
                                        case "Annual":
                                            empBal.leaveBalance.annual = (decimal)reader2["Balance"];
                                            break;
                                        case "Sick":
                                            empBal.leaveBalance.sick = (decimal)reader2["Balance"];
                                            break;
                                        case "Compassionate":
                                            empBal.leaveBalance.compassionate = (decimal)reader2["Balance"];
                                            break;
                                        case "Maternity":
                                            empBal.leaveBalance.maternity = (decimal)reader2["Balance"];
                                            break;
                                        case "Short_Hours":
                                            empBal.leaveBalance.shortLeaveHours = (decimal)reader2["Balance"];
                                            break;
                                        case "Unpaid":
                                            empBal.leaveBalance.unpaidTotal = (decimal)reader2["Balance"];
                                            break;
                                        case "DIL":
                                            empBal.leaveBalance.daysInLieue = (decimal)reader2["Balance"];
                                            break;
                                        case "Pilgrimage":
                                            empBal.leaveBalance.pilgrimage = (decimal)reader2["Balance"];
                                            break;
                                        default:
                                            break; ;
                                    }
                                }
                            }
                            connection2.Close();
                        }
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
            string search = form["EnteredSearch"];
            return RedirectToAction("Index", new { filterDepartmentID = deptID, filterAccStatus = accStatID, filterSearch = search });
        }

        private string GetFilteredQuery(int deptID, int accStat, string search)
        {
            string queryString = "SELECT Employee_ID, First_Name, Last_Name, Gender, Religion_ID FROM dbo.Employee WHERE Probation='False'";

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
            var lv = GetLeaveBalanceModel();

            ViewBag.sid = empID;                                             // pass the staff id to view
            ViewBag.name = GetFullName(empID);
            ViewBag.gender = GetGender(empID);                               // retrieve staff's gender and pass it to view
            ViewBag.religion = GetReligion(empID);                           // retrieve staff's religion and pass it to view
            ViewBag.annual = GetBalance(empID, lv.annualID);                 // retrieve staff's annual balance and pass it to view
            ViewBag.maternity = GetBalance(empID, lv.maternityID);           // retrieve staff's maternity balance and pass it to view
            ViewBag.sick = GetBalance(empID, lv.sickID);                     // retrieve staff's sick balance and pass it to view
            ViewBag.daysInLieue = GetBalance(empID, lv.daysInLieueID);       // retrieve staff's days in lieu balance and pass it to view
            ViewBag.compassionate = GetBalance(empID, lv.compassionateID);   // retrieve staff's compassionate balance and pass it to view
            ViewBag.shortLeaveHours = GetBalance(empID, lv.shortID);         // retrieve staff's short leave hours balance and pass it to view
            ViewBag.pilgrimage = GetBalance(empID, lv.pilgrimageID);         // retrieve staff's short leave hours balance and pass it to view
            return View(lv);
        }

        [HttpPost]
        public ActionResult Edit(Models.sleaveBalanceModel m)
        {
            if (ModelState.IsValid)
            {
                DBUpdateBalance(m.empId, m.annualID, m.annual);
                DBUpdateBalance(m.empId, m.maternityID, m.maternity);
                DBUpdateBalance(m.empId, m.sickID, m.sick);
                DBUpdateBalance(m.empId, m.compassionateID, m.compassionate);
                DBUpdateBalance(m.empId, m.daysInLieueID, m.daysInLieue);
                DBUpdateBalance(m.empId, m.shortID, m.shortLeaveHours);
                Response.Write("<script> alert('Success. The information has been updated.');</script>");
            }
            else
            {
                ModelState.AddModelError("errmsg", "Failed: An error occured. Please check your input and try again.");
            }
            return Edit(m.empId);
        }

        private string GetFullName(int empID)
        {
            string name = "";

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select First_Name, Last_Name FROM dbo.Employee WHERE Employee_ID = " + empID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        name = (string)reader["First_Name"] + " " + (string)reader["Last_Name"];
                    }
                }
                connection.Close();
            }

            return name;
        }

        private string GetReligion(int empID)
        {
            string religion = "";

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Religion_Name FROM dbo.Employee, dbo.Religion WHERE Employee_ID = " + empID + " AND Employee.Religion_ID = Religion.Religion_ID";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        religion = (string)reader["Religion_Name"];
                    }
                }
                connection.Close();
            }

            return religion;
        }

        private char GetGender(int empID)
        {
            char gender = '\0';
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Gender FROM dbo.Employee WHERE Employee_ID = '" + empID + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        gender = Convert.ToChar(reader["Gender"]);
                    }
                }
                connection.Close();
            }

            return gender;
        }

        private decimal GetBalance(int employeeID, int leaveID)
        {
            decimal balance = 0;                    // actual balance
            Boolean existingBalance = false;        // in case's where the record does not exist

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Balance FROM dbo.Leave_Balance Where Employee_ID ='" + employeeID + "' And Leave_ID= '" + leaveID + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        balance = (decimal)reader["Balance"];
                        existingBalance = true;
                    }
                }
                connection.Close();

                if (!existingBalance)           // was the balance found?
                {
                    InsertBalance(employeeID, leaveID, 0);     // if not, create a new one and set it to 0 by default
                }
            }

            return balance;
        }

        private void InsertBalance(int employeeID, int leaveID, decimal balance)
        {
            string insertQuery = "Insert into dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) Values('" + employeeID + "','" + leaveID + "','" + balance + "')";
            DBExecuteQuery(insertQuery);
        }

        private void DBUpdateBalance(int employeeID, int leaveID, decimal duration)
        {
            string updateQuery = "Update dbo.Leave_Balance SET Balance='" + duration + "' WHERE Leave_ID='" + leaveID + "' And Employee_ID='" + employeeID + "'";
            DBExecuteQuery(updateQuery);
        }

    }
}