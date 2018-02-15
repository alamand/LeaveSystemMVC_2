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

        protected void Output(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        protected int GetLoggedInID()
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var identity = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            string loggedInID = identity.ToString();
            loggedInID = loggedInID.Substring(loggedInID.Length - 5);
            return Convert.ToInt32(loggedInID);
        }

        protected sleaveBalanceModel GetLeaveBalanceModel(int empID = 0)
        {
            var lb = new sleaveBalanceModel();

            // this query will be used to get the default durations on all leave types
            string queryString = "Select * FROM dbo.Leave_Type";        

            // the word Duration is the column name in dbo.Leave_Types
            string balanceColName = "Duration";

            // change the query, gets balances on all leave types for a specific employee
            if (empID > 0)
            {
                // recalls this method to get the leave IDs and default balances
                lb = GetLeaveBalanceModel();
                lb.empId = empID;

                // sets all balances to 0, so that if the database does not containt a record for the employee, it won't use the default balance
                lb.annual = lb.compassionate = lb.daysInLieu = lb.maternity = lb.sick = lb.unpaid = lb.pilgrimage = 0;

                // the word Balance is the column name in dbo.Leave_Balance
                balanceColName = "Balance";     

                queryString = "SELECT Leave_Type.Leave_ID, Leave_Name, Balance " +
                "FROM dbo.Leave_Balance, dbo.Leave_Type " +
                "Where Leave_Type.Leave_ID = Leave_Balance.Leave_ID AND Employee_ID =" + empID;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);      

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        switch ((string)reader["Leave_Name"])
                        {
                            case "Annual":
                                lb.annualID = (int)reader["Leave_ID"];
                                lb.annual = (decimal)reader[balanceColName];
                                break;

                            case "Maternity":
                                lb.maternityID = (int)reader["Leave_ID"];
                                lb.maternity = (decimal)reader[balanceColName];
                                break;

                            case "Sick":
                                lb.sickID = (int)reader["Leave_ID"];
                                lb.sick = (decimal)reader[balanceColName];
                                break;

                            case "DIL":
                                lb.daysInLieuID = (int)reader["Leave_ID"];
                                lb.daysInLieu = (decimal)reader[balanceColName];
                                break;

                            case "Compassionate":
                                lb.compassionateID = (int)reader["Leave_ID"];
                                lb.compassionate = (decimal)reader[balanceColName];
                                break;

                            case "Short_Hours_Per_Month":
                                lb.shortHoursID = (int)reader["Leave_ID"];
                                lb.shortHours = (decimal)reader[balanceColName];
                                break;

                            case "Pilgrimage":
                                lb.pilgrimageID = (int)reader["Leave_ID"];
                                lb.pilgrimage = (decimal)reader[balanceColName];
                                break;

                            case "Unpaid":
                                lb.unpaidID = (int)reader["Leave_ID"];
                                lb.unpaid = (decimal)reader[balanceColName];
                                break;

                            default:
                                break; ;
                        }
                    }
                }
                connection.Close();
            }
            return lb;
        }

        protected sEmployeeModel GetEmployeeModel(int empID)
        {
            sEmployeeModel employeeModel = new sEmployeeModel();

            var queryString = "";

            // true if this employee has a record in the Reporting Table
            Boolean reportingIDExist = IsReportingExist(empID);  

            if (reportingIDExist)
                queryString += "SELECT * FROM dbo.Employee, dbo.Reporting, dbo.Employment_Period WHERE Employee.Employee_ID = Employment_Period.Employee_ID AND " +
                    "Employee.Employee_ID = Reporting.Employee_ID AND Employee.Employee_ID = " + empID + " AND " +
                    "(Reporting.Start_Date <= SYSDATETIME() AND (Reporting.End_Date > SYSDATETIME() OR Reporting.End_Date IS NULL))";
            else
                queryString += "SELECT * FROM dbo.Employee WHERE Employee.Employee_ID = " + empID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // @TODO: fix the DBNull checking after the DB has been updated
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
                        if (reportingIDExist)
                            employeeModel.reportsToLineManagerID = (reader["Reporting_ID"] != DBNull.Value) ? (int)reader["Reporting_ID"] : (int?)null;
                        employeeModel.religionID = (reader["Religion_ID"] != DBNull.Value) ? (int)reader["Religion_ID"] : 0;
                        employeeModel.dateOfBirth = (!DBNull.Value.Equals(reader["Date_Of_Birth"])) ? (DateTime)reader["Date_Of_Birth"] : new DateTime();
                        employeeModel.nationalityID = (reader["Nationality_ID"] != DBNull.Value) ? (int)reader["Nationality_ID"] : 0;
                        employeeModel.onProbation = (reader["Probation"] != DBNull.Value) ? (bool)reader["Probation"] : false;
                        employeeModel.empStartDate = (reader["Emp_Start_Date"] != DBNull.Value) ? (DateTime)reader["Emp_Start_Date"] : new DateTime();
                    }
                }
                connection.Close();
            }

            var roleDictionary = DBRoleList();
            var queryString2 = "SELECT Role_ID FROM dbo.Employee_Role WHERE Employee_ID = " + empID;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (roleDictionary[(int)reader["Role_ID"]].Equals("Staff"))
                            employeeModel.isStaff = true;

                        if (roleDictionary[(int)reader["Role_ID"]].Equals("LM"))
                            employeeModel.isLM = true;

                        if (roleDictionary[(int)reader["Role_ID"]].Equals("HR"))
                            employeeModel.isHR = true;

                        if (roleDictionary[(int)reader["Role_ID"]].Equals("HR_Responsible"))
                            employeeModel.isHRResponsible = true;

                        if (roleDictionary[(int)reader["Role_ID"]].Equals("Admin"))
                            employeeModel.isAdmin = true;
                    }
                }
                connection.Close();
            }

            return employeeModel;
        }

        protected List<sEmployeeModel> GetEmployeeModel()
        {
            List<sEmployeeModel> empList = new List<sEmployeeModel>();
            var queryString = "SELECT Employee_ID FROM dbo.Employee";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        empList.Add(GetEmployeeModel((int)reader["Employee_ID"]));
                    }
                }
                connection.Close();
            }
            
            return empList;
        }

        protected Boolean IsReportingExist(int empID)
        {
            bool isExist = false;

            var queryString = "SELECT COUNT(*) FROM dbo.Employee, dbo.Reporting WHERE Employee.Employee_ID = Reporting.Employee_ID AND Employee.Employee_ID = " + empID + " AND " +
                            "(Reporting.Start_Date <= SYSDATETIME() AND (Reporting.End_Date > SYSDATETIME() OR Reporting.End_Date IS NULL))";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                if ((int)command.ExecuteScalar() > 0)
                    isExist = true;
                connection.Close();
            }
            return isExist;
        }

        protected List<sLeaveModel> GetLeaveModel(string listFor="", int id=0)
        {
            var leaveList = new List<sLeaveModel>();

            var queryString = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, Leave.Reporting_Back_Date, Leave.Leave_ID, Leave_Name, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, HR_Comment, LM_Comment " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_ID = Leave_Type.Leave_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND Employee.Employee_ID = Reporting.Employee_ID";

            if (!listFor.Equals("") && id != 0)
            {
                 queryString += " AND " + listFor + " = " + id;
            }

            if (!listFor.Equals("") && id == 0)
            {
                queryString = listFor;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var leave = new sLeaveModel
                        {
                            leaveAppID = (int)reader["Leave_Application_ID"],
                            employeeID = (int)reader["Employee_ID"],
                            employeeName = (string)reader["First_Name"] + " " + (string)reader["Last_Name"],
                            startDate = (!DBNull.Value.Equals(reader["Start_Date"])) ? (DateTime)reader["Start_Date"] : new DateTime(0, 0, 0),
                            returnDate = (!DBNull.Value.Equals(reader["Reporting_Back_Date"])) ? (DateTime)reader["Reporting_Back_Date"] : new DateTime(0,0,0),
                            leaveTypeID = (int)reader["Leave_ID"],
                            leaveTypeName = (string)reader["Leave_Name"],
                            contactDetails = (!DBNull.Value.Equals(reader["Contact_Outside_UAE"])) ? (string)reader["Contact_Outside_UAE"] : "",
                            comments = (!DBNull.Value.Equals(reader["Comment"])) ? (string)reader["Comment"] : "",
                            documentation = (!DBNull.Value.Equals(reader["Documentation"])) ? (string)reader["Documentation"] : "",
                            bookAirTicket = (!DBNull.Value.Equals(reader["Flight_Ticket"])) ? (bool)reader["Flight_Ticket"] : false,
                            leaveDuration = (decimal)reader["Total_Leave"],
                            shortStartTime = (!DBNull.Value.Equals(reader["Start_Hrs"])) ? (TimeSpan)reader["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                            shortEndTime = (!DBNull.Value.Equals(reader["End_Hrs"])) ? (TimeSpan)reader["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                            leaveStatusID = (int)reader["Leave_Status_ID"],
                            leaveStatusName = (string)reader["Status_Name"],
                            hrComment = (!DBNull.Value.Equals(reader["HR_Comment"])) ? (string)reader["HR_Comment"] : "",
                            lmComment = (!DBNull.Value.Equals(reader["LM_Comment"])) ? (string)reader["LM_Comment"] : ""
                        };
                        leaveList.Add(leave);
                    }
                }
                connection.Close();
            }

            return leaveList;
        }

        protected bool IsLeaveBalanceExists(int empID, int leaveID)
        {
            bool isExist = false;

            var queryString = "SELECT COUNT(*) FROM dbo.Leave_Balance WHERE Employee_ID = " + empID + " AND Leave_ID = " + leaveID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                if ((int)command.ExecuteScalar() > 0)
                    isExist = true;
                connection.Close();
            }
            return isExist;
        }

        protected int GetSubstituteID(int empID)
        {
            string queryString2 = "SELECT dbo.Employee.Substitute_ID FROM dbo.Employee WHERE dbo.Employee.Employee_ID = '" + empID + "'";
            int subsID = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (reader["Substitute_ID"] != DBNull.Value)
                            {
                                subsID = (int)reader["Substitute_ID"];
                            }
                        }
                    }
                }
            }
            return subsID;
        }

        protected bool IsHRResponsibleExist()
        {
            bool isExist = false;
            int hrrRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "HR_Responsible").Key;

            var queryString = "SELECT COUNT(*) FROM dbo.Employee_Role WHERE Role_ID = " + hrrRoleID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                if ((int)command.ExecuteScalar() > 0)
                    isExist = true;
                connection.Close();
            }
            return isExist;
        }

        protected bool IsAdminExist(int numOfAdmins = 1)
        {
            bool isExist = false;
            int adminRole = DBRoleList().FirstOrDefault(obj => obj.Value == "Admin").Key;

            var queryString = "SELECT COUNT(*) FROM dbo.Employee_Role WHERE Role_ID = " + adminRole;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                if ((int)command.ExecuteScalar() >= numOfAdmins)
                    isExist = true;
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

        protected Dictionary<int, string> AccountStatusList()
        {
            Dictionary<int, string> accStatus = new Dictionary<int, string>
            {
                { -1, "Active/Inactive" },
                { 1, "Active Only" },
                { 0, "Inactive Only" }
            };
            return accStatus;
        }

        protected Dictionary<int, string> DBLeaveStatusList()
        {
            var queryString = "SELECT Leave_Status_ID, Status_Name FROM dbo.Leave_Status";
            return DBListing(queryString, "Leave_Status_ID", "Status_Name");
        }

        protected Dictionary<int, string> DBEmployeeList(int accountStatus = -1)
        {
            Dictionary<int, string> list = new Dictionary<int, string>();
            var queryString = "SELECT Employee_ID, First_Name, Last_Name FROM dbo.Employee";

            if (accountStatus == 1 || accountStatus == 0)
                queryString += " WHERE Account_Status = " + accountStatus;

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
            var queryString = "SELECT Department_ID, Department_Name FROM dbo.Department";
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