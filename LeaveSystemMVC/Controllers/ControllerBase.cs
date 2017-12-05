using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class ControllerBase : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public Dictionary<int, string> DepartmentList()
        {
            Dictionary<int, string> departments = new Dictionary<int, string>();
            string queryString = "Select Department_ID, Department_Name FROM dbo.Department";

            // adds all available departments to the dictionary, including 0, as All Departments
            departments.Add(0, "All Departments");
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        departments.Add((int)reader["Department_ID"], (string)reader["Department_Name"]);
                    }
                }
                connection.Close();
            }
            return departments;
        }

        protected sleaveBalanceModel ConstructLeaveBalance()
        {
            var lv = new sleaveBalanceModel();
            string queryString = "Select * FROM dbo.Leave_Type";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);      // retrieve leave id and type

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // iterate through all leave types in the database and update sleaveModel 
                    while (reader.Read())
                    {
                        if (reader["Leave_Name"].Equals("Annual"))
                        {
                            lv.annualID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("Maternity"))
                        {
                            lv.maternityID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("Sick"))
                        {
                            lv.sickID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("DIL"))
                        {
                            lv.daysInLieueID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("Compassionate"))
                        {
                            lv.compassionateID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("Short_Hours"))
                        {
                            lv.shortID = (int)reader["Leave_ID"];
                        }
                    }
                }
                connection.Close();
            }
            return lv;
        }

        protected void InsertBalance(int employeeID, int leaveID, decimal balance)
        {
            string insertQuery = "Insert into dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) Values('" + employeeID + "','" + leaveID + "','" + balance + "')";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(insertQuery, connection);

                connection.Open();
                using (var reader = command.ExecuteReader()) { }
                connection.Close();
            }
        }

        protected void UpdateBalance(int employeeID, int leaveID, decimal duration)
        {
            string queryUpdate = "Update dbo.Leave_Balance SET Balance='" + duration + "' WHERE Leave_ID='" + leaveID + "' And Employee_ID='" + employeeID + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryUpdate, connection);
                connection.Open();
                using (var reader = command.ExecuteReader()) { }
                connection.Close();
            }
        }

        protected string GetLoggedInID()
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var identity = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            string loggedInID = identity.ToString();
            return loggedInID.Substring(loggedInID.Length - 5);
        }
    }
}