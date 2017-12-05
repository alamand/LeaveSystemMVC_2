using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class lmSubordinateBalanceController : ControllerBase
    {
        // GET: lmSubordinateBalance
        public ActionResult Index(int filterDepartmentID = 0)
        {
            var model = new List<Models.hrEmpLeaveBalModel>();

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Employee_ID, First_Name, Last_Name, Gender FROM dbo.Employee WHERE Reporting_ID = " + GetLoggedInID();

            // adds a filter query if a department is selected from the dropdown, note that 0 represents All Departments
            if (filterDepartmentID > 0)
            {
                queryString += " AND Department_ID = " + filterDepartmentID;
            }

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

                        string queryString2 = "Select Balance,Leave_Name FROM dbo.Leave_Balance, dbo.Leave_Type where Leave_Balance.Employee_ID = '" + empBal.employee.staffID + "' AND Leave_Balance.Leave_ID = Leave_Type.Leave_ID";

                        using (var connection2 = new SqlConnection(connectionString))
                        {
                            var command2 = new SqlCommand(queryString2, connection2);
                            connection2.Open();
                            using (var reader2 = command2.ExecuteReader())
                            {
                                while (reader2.Read())
                                {
                                    string leave = (string)reader2["Leave_Name"];
                                    if (leave.Equals("Annual"))
                                        empBal.leaveBalance.annual = (decimal)reader2["Balance"];

                                    if (leave.Equals("Sick"))
                                        empBal.leaveBalance.sick = (decimal)reader2["Balance"];

                                    if (leave.Equals("Compassionate"))
                                        empBal.leaveBalance.compassionate = (decimal)reader2["Balance"];

                                    if (leave.Equals("Maternity"))
                                        empBal.leaveBalance.maternity = (decimal)reader2["Balance"];

                                    if (leave.Equals("Short_Hours"))
                                        empBal.leaveBalance.shortLeaveHours = (decimal)reader2["Balance"];

                                    if (leave.Equals("Unpaid"))
                                        empBal.leaveBalance.unpaidTotal = (decimal)reader2["Balance"];

                                    if (leave.Equals("DIL"))
                                        empBal.leaveBalance.daysInLieue = (decimal)reader2["Balance"];
                                }
                            }

                            connection2.Close();
                        }

                        model.Add(empBal);
                    }
                }
                connection.Close();
            }

            ViewData["DepartmentList"] = DepartmentList();
            ViewData["SelectedDepartment"] = filterDepartmentID;

            return View(model);
        }

        [HttpPost]
        public ActionResult FilterListByDepartment(FormCollection form)
        {
            int id = Convert.ToInt32(form["selectedDepartment"]);
            return RedirectToAction("Index", new { filterDepartmentID = id });
        }
    }
}