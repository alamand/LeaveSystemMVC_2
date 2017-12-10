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

        protected sleaveBalanceModel GetLeaveBalanceModel()
        {
            var lv = new sleaveBalanceModel();
            string queryString = "Select * FROM dbo.Leave_Type";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);      

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["Leave_Name"].Equals("Annual"))
                        {
                            lv.annualID = (int)reader["Leave_ID"];
                            lv.annual = (int)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Maternity"))
                        {
                            lv.maternityID = (int)reader["Leave_ID"];
                            lv.maternity = (int)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Sick"))
                        {
                            lv.sickID = (int)reader["Leave_ID"];
                            lv.sick = (int)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("DIL"))
                        {
                            lv.daysInLieueID = (int)reader["Leave_ID"];
                            lv.daysInLieue = (int)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Compassionate"))
                        {
                            lv.compassionateID = (int)reader["Leave_ID"];
                            lv.compassionate = (int)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Short_Hours"))
                        {
                            lv.shortID = (int)reader["Leave_ID"];
                            lv.shortLeaveHours = (int)reader["Duration"];
                        }
                        else if (reader["Leave_Name"].Equals("Pilgrimage"))
                        {
                            lv.pilgrimageID = (int)reader["Leave_ID"];
                            lv.pilgrimage = (int)reader["Duration"];
                        }
                    }
                }
                connection.Close();
            }
            return lv;
        }

        protected sEmployeeModel GetEmployeeModel(int empID)
        {
            sEmployeeModel employeeModel = new sEmployeeModel();
            var queryString = "SELECT * FROM dbo.Employee WHERE Employee_ID = " + empID + " AND Employee_ID IS NOT NULL";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        employeeModel.staffID = (int)reader["Employee_ID"];
                        employeeModel.firstName = (string)reader["First_Name"];
                        employeeModel.lastName = (string)reader["Last_Name"];
                        employeeModel.userName = (string)reader["User_Name"];
                        employeeModel.designation = (string)reader["Designation"];
                        employeeModel.email = (string)reader["Email"];
                        employeeModel.gender = ((string)reader["Gender"])[0];
                        employeeModel.deptID = (reader["Department_ID"] != DBNull.Value) ? (int)reader["Department_ID"] : (int?)null;
                        employeeModel.phoneNo = (reader["Ph_No"] != DBNull.Value) ? (string)reader["Ph_No"] : "";
                        employeeModel.accountStatus = (bool)reader["Account_Status"];
                        employeeModel.reportsToLineManagerID = (reader["Reporting_ID"] != DBNull.Value) ? (int)reader["Reporting_ID"] : (int?)null;
                        employeeModel.religionID = (int)reader["Religion_ID"];
                        employeeModel.dateOfBirth = (!DBNull.Value.Equals(reader["Date_Of_Birth"])) ? ((DateTime)reader["Date_Of_Birth"]) : new DateTime();
                        employeeModel.nationalityID = (int)reader["Nationality_ID"];
                        employeeModel.onProbation = (reader["Probation"] != DBNull.Value) ? ((bool)reader["Probation"]) : false;
                    }
                }
                connection.Close();
            }

            int staffRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "Staff").Key;
            int lmRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "LM").Key;
            int hrRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "HR").Key;
            int hrrRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "HR_Responsible").Key;
            int adminRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "Admin").Key;

            var queryString2 = "SELECT Role_ID FROM dbo.Employee_Role WHERE Employee_ID = " + empID;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if ((int)reader["Role_ID"] == staffRoleID)
                            employeeModel.isStaff = true;

                        if ((int)reader["Role_ID"] == lmRoleID )
                            employeeModel.isLM = true;

                        if ((int)reader["Role_ID"] == hrRoleID)
                            employeeModel.isHR = true;

                        if ((int)reader["Role_ID"] == hrrRoleID)
                            employeeModel.isHRResponsible = true;

                        if ((int)reader["Role_ID"] == adminRoleID)
                            employeeModel.isAdmin = true;
                    }
                }
                connection.Close();
            }

            return employeeModel;
        }

        public bool IsHRResponsibleExist()
        {
            bool isExist = false;
            int hrrRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "HR_Responsible").Key;

            var queryString = "SELECT Employee_ID, Role_ID FROM dbo.Employee_Role WHERE Role_ID = " + hrrRoleID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        isExist = true;
                    }
                }
                connection.Close();
            }
            return isExist;
        }

        protected void DBInsertRole(int employeeID, int roleID)
        {
            string insertQuery = "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) VALUES ('" + employeeID + "', '" + roleID + "') ";
            DBExecuteQuery(insertQuery);
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

        protected Dictionary<int, string> DBEmployeeList()
        {
            Dictionary<int, string> list = new Dictionary<int, string>();
            var queryString = "SELECT Employee_ID, First_Name, Last_Name FROM dbo.Employee";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fullName = (string)reader["First_Name"] + " " + (string)reader["Last_Name"];
                        list.Add((int)reader["Employee_ID"], fullName);
                    }
                }
                connection.Close();
            }
            return list;

        }

        protected Dictionary<int, string> DBLineManagerList()
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

        protected Dictionary<int, string> DBLeaveTypeList()
        {
            var queryString = "SELECT Leave_ID, Leave_Name FROM dbo.Leave_Type";
            return DBListing(queryString, "Leave_ID", "Leave_Name");
        }

        protected Dictionary<int, string> DBNationalityList()
        {
            var queryString = "SELECT Nationality_ID, Nationality_Name FROM dbo.Nationality";
            return DBListing(queryString, "Nationality_ID", "Nationality_Name");
        }

        protected Dictionary<int, string> DBDepartmentList()
        {
            var queryString = "Select Department_ID, Department_Name FROM dbo.Department";
            return DBListing(queryString, "Department_ID", "Department_Name");
        }

        protected Dictionary<int, string> DBRoleList()
        {
            var queryString = "SELECT Role_ID, Role_Name FROM dbo.Role";
            return DBListing(queryString, "Role_ID", "Role_Name");
        }

        protected Dictionary<int, string> DBReligionList()
        {
            var queryString = "SELECT Religion_ID, Religion_Name FROM dbo.Religion";
            return DBListing(queryString, "Religion_ID", "Religion_Name");
        }

        protected Dictionary<int, string> AddDefaultToDictionary(Dictionary<int, string> dictionary, int key, string value)
        {
            Dictionary<int, string> newDictionary = new Dictionary<int, string>();
            newDictionary.Add(key, value);
            foreach (KeyValuePair<int, string> entry in dictionary)
            {
                newDictionary.Add(entry.Key, entry.Value);
            }

            return newDictionary;
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