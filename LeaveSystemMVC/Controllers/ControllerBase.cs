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

        protected string GetLoggedInID()
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var identity = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            string loggedInID = identity.ToString();
            return loggedInID.Substring(loggedInID.Length - 5);
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
                            lv.annual = (decimal)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Maternity"))
                        {
                            lv.maternityID = (int)reader["Leave_ID"];
                            lv.maternity = (decimal)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Sick"))
                        {
                            lv.sickID = (int)reader["Leave_ID"];
                            lv.sick = (decimal)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("DIL"))
                        {
                            lv.daysInLieueID = (int)reader["Leave_ID"];
                            lv.daysInLieue = (decimal)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Compassionate"))
                        {
                            lv.compassionateID = (int)reader["Leave_ID"];
                            lv.compassionate = (decimal)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Short_Hours"))
                        {
                            lv.shortID = (int)reader["Leave_ID"];
                            lv.shortLeaveHours = (decimal)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Pilgrimage"))
                        {
                            lv.pilgrimageID = (int)reader["Leave_ID"];
                            lv.pilgrimage = (decimal)reader["Duration"];
                        }
                    }
                }
                connection.Close();
            }
            return lv;
        }



        protected void DBInsertRole(int employeeID, int roleID)
        {
            string insertQuery = "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                    "VALUES ('" + employeeID + "', '" + roleID + "') ";
            DBExecuteQuery(insertQuery);
        }

        protected void DBInsertBalance(int employeeID, int leaveID, decimal balance)
        {
            string insertQuery = "Insert into dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) " +
                "Values('" + employeeID + "','" + leaveID + "','" + balance + "')";
            DBExecuteQuery(insertQuery);
        }

        protected void DBUpdateBalance(int employeeID, int leaveID, decimal duration)
        {
            string updateQuery = "Update dbo.Leave_Balance SET Balance='" + duration + "' WHERE Leave_ID='" + leaveID + "' And Employee_ID='" + employeeID + "'";
            DBExecuteQuery(updateQuery);
        }

        protected void DBExecuteQuery(string query)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    connection.Close();
            }
        }



        public Dictionary<int, string> DBLineManagerList()
        {
            int lmRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "LM").Key;
            Dictionary<int, string> lineManagers = new Dictionary<int, string>();

            var queryString = "SELECT Employee.Employee_ID, First_Name, Last_Name " +
                "FROM dbo.Employee, dbo.Employee_Role " +
                "WHERE Employee.Employee_ID = Employee_Role.Employee_ID AND Employee_Role.Role_ID = " + lmRoleID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fullName = (string)reader["First_Name"] + " " + (string)reader["Last_Name"];
                        if (!lineManagers.ContainsKey((int)reader["Employee_ID"]))
                        {
                            lineManagers.Add((int)reader["Employee_ID"], fullName);
                        }
                    }
                }
                connection.Close();
            }
            return lineManagers;
        }

        public Dictionary<int, string> DBLeaveTypeList()
        {
            var queryString = "SELECT Leave_ID, Leave_Name FROM dbo.Leave_Type";
            return DBListing(queryString, "Leave_ID", "Leave_Name");
        }

        public Dictionary<int, string> DBNationalityList()
        {
            var queryString = "SELECT Nationality_ID, Nationality_Name FROM dbo.Nationality";
            return DBListing(queryString, "Nationality_ID", "Nationality_Name");
        }

        public Dictionary<int, string> DBDepartmentList()
        {
            var queryString = "Select Department_ID, Department_Name FROM dbo.Department";
            return DBListing(queryString, "Department_ID", "Department_Name");
        }

        public Dictionary<int, string> DBRoleList()
        {
            var queryString = "SELECT Role_ID, Role_Name FROM dbo.Role";
            return DBListing(queryString, "Role_ID", "Role_Name");
        }

        public Dictionary<int, string> DBReligionList()
        {
            var queryString = "SELECT Religion_ID, Religion_Name FROM dbo.Religion";
            return DBListing(queryString, "Religion_ID", "Religion_Name");
        }

        private Dictionary<int, string> DBListing(string query, string id, string name)
        {
            Dictionary<int, string> list = new Dictionary<int, string>();
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((int)reader[id], reader[name].ToString());
                    }
                }
                connection.Close();
            }
            return list;
        }
    }
}