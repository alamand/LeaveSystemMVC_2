using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;
using System.Net;
using System.Net.Mail;

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

        protected void SetMessageViewBags()
        {
            ViewBag.InfoMessage = TempData["InfoMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.WarningMessage = TempData["WarningMEssage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
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
                lb.annual = lb.compassionate = lb.daysInLieu = lb.maternity = lb.sick = lb.unpaid = lb.pilgrimage = lb.shortHours = 0;

                // the word Balance is the column name in dbo.Leave_Balance
                balanceColName = "Balance";     

                queryString = "SELECT Leave_Type.Leave_Type_ID, Leave_Name, Balance " +
                "FROM dbo.Leave_Balance, dbo.Leave_Type " +
                "Where Leave_Type.Leave_Type_ID = Leave_Balance.Leave_Type_ID AND Employee_ID =" + empID;
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
                                lb.annualID = (int)reader["Leave_Type_ID"];
                                lb.annual = (decimal)reader[balanceColName];
                                break;

                            case "Maternity":
                                lb.maternityID = (int)reader["Leave_Type_ID"];
                                lb.maternity = (decimal)reader[balanceColName];
                                break;

                            case "Sick":
                                lb.sickID = (int)reader["Leave_Type_ID"];
                                lb.sick = (decimal)reader[balanceColName];
                                break;

                            case "DIL":
                                lb.daysInLieuID = (int)reader["Leave_Type_ID"];
                                lb.daysInLieu = (decimal)reader[balanceColName];
                                break;

                            case "Compassionate":
                                lb.compassionateID = (int)reader["Leave_Type_ID"];
                                lb.compassionate = (decimal)reader[balanceColName];
                                break;

                            case "Short_Hours":
                                lb.shortHoursID = (int)reader["Leave_Type_ID"];
                                lb.shortHours = (decimal)reader[balanceColName];
                                break;

                            case "Pilgrimage":
                                lb.pilgrimageID = (int)reader["Leave_Type_ID"];
                                lb.pilgrimage = (decimal)reader[balanceColName];
                                break;

                            case "Unpaid":
                                lb.unpaidID = (int)reader["Leave_Type_ID"];
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

            bool reportingIDExist = IsReportingExist(empID);                
            bool startDateExist = IsStartDateExist(empID);                 


            if (reportingIDExist && startDateExist) // @TODO: fix this when line manager substitution and employment period feature is added
                queryString += "SELECT * FROM dbo.Employee, dbo.Reporting, dbo.Employment_Period WHERE Employee.Employee_ID = Employment_Period.Employee_ID AND " +
                    "Employee.Employee_ID = Reporting.Employee_ID AND Employee.Employee_ID = " + empID;
            else if (reportingIDExist)     
                queryString += "SELECT * FROM dbo.Employee, dbo.Reporting WHERE Employee.Employee_ID = Reporting.Employee_ID AND Employee.Employee_ID = " + empID;
            else if (startDateExist) 
                queryString += "SELECT * FROM dbo.Employee, dbo.Employment_Period WHERE Employee.Employee_ID = Employment_Period.Employee_ID AND Employee.Employee_ID = " + empID;
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
                            employeeModel.reportsToLineManagerID = (reader["Report_To_ID"] != DBNull.Value) ? (int)reader["Report_To_ID"] : (int?)null;
                        employeeModel.religionID = (reader["Religion_ID"] != DBNull.Value) ? (int)reader["Religion_ID"] : 0;
                        employeeModel.dateOfBirth = (!DBNull.Value.Equals(reader["Date_Of_Birth"])) ? (DateTime)reader["Date_Of_Birth"] : new DateTime();
                        employeeModel.nationalityID = (reader["Nationality_ID"] != DBNull.Value) ? (int)reader["Nationality_ID"] : 0;
                        employeeModel.onProbation = (reader["Probation"] != DBNull.Value) ? (bool)reader["Probation"] : false;
                        if (startDateExist)
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

        protected bool IsPilgrimageAllowed(int empID)
        {
            sEmployeeModel emp = GetEmployeeModel(empID);
            List<sEmployeeModel> employmentList = GetEmploymentPeriod(empID);

            // gets the latest employment period.
            sEmployeeModel latestEmployment = employmentList[employmentList.Count - 1];
            TimeSpan diff = DateTime.Today - latestEmployment.empStartDate;
            double years = diff.TotalDays / 365.25;
            if (DBReligionList()[emp.religionID].Equals("Muslim") && years >= 5)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected List<sEmployeeModel> GetEmploymentPeriod(int empID)
        {
            List<sEmployeeModel> employmentList = new List<sEmployeeModel>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT * FROM dbo.Employment_Period WHERE Employee_ID = '" + empID + "' ORDER BY Emp_Start_Date";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employment = new sEmployeeModel
                        {
                            staffID = (int)reader["Employee_ID"],
                            empStartDate = (reader["Emp_Start_Date"] != DBNull.Value) ? (DateTime)reader["Emp_Start_Date"] : new DateTime(),
                            empEndDate = (reader["Emp_End_Date"] != DBNull.Value) ? (DateTime)reader["Emp_End_Date"] : new DateTime()
                        };
                        employmentList.Add(employment);
                    }
                }
                connection.Close();
            }

            return employmentList;
        }

        protected bool IsReportingExist(int empID)
        {
            bool isExist = false;

            var queryString = "SELECT COUNT(*) FROM dbo.Reporting WHERE Employee_ID = " + empID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                isExist = ((int)command.ExecuteScalar() > 0) ? true : false;
                connection.Close();
            }
            return isExist;
        }

        protected bool IsStartDateExist(int empID)
        {
            bool isExist = false;

            var queryString = "SELECT COUNT(*) FROM dbo.Employment_Period WHERE Employee_ID = " + empID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                isExist = ((int)command.ExecuteScalar() > 0) ? true : false;
                connection.Close();
            }
            return isExist;
        }

        protected List<sLeaveModel> GetLeaveModel(string listFor = "", int id = 0)
        {
            var queryString = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, Leave.Reporting_Back_Date, Leave.Leave_Type_ID, Leave_Name, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, Leave_Status.Display_Name, HR_Comment, LM_Comment, Leave.Personal_Email, Is_Half_Start_Date, Is_Half_Reporting_Back_Date " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_Type_ID = Leave_Type.Leave_Type_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND Employee.Employee_ID = Reporting.Employee_ID";

            if (!listFor.Equals("") && id != 0)
            {
                queryString += " AND " + listFor + " = " + id;
            }

            return GetLeaveModel(queryString);
        }

        protected List<sLeaveModel> GetLeaveModel(string queryString)
        {
            var leaveList = new List<sLeaveModel>();

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
                            leaveTypeID = (int)reader["Leave_Type_ID"],
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
                            leaveStatusDisplayName = (string)reader["Display_Name"],
                            hrComment = (!DBNull.Value.Equals(reader["HR_Comment"])) ? (string)reader["HR_Comment"] : "",
                            lmComment = (!DBNull.Value.Equals(reader["LM_Comment"])) ? (string)reader["LM_Comment"] : "",
                            email = (!DBNull.Value.Equals(reader["Personal_Email"])) ? (string)reader["Personal_Email"] : "",
                            isStartDateHalfDay = (!DBNull.Value.Equals(reader["Is_Half_Start_Date"])) ? (bool)reader["Is_Half_Start_Date"] : false,
                            isReturnDateHalfDay = (!DBNull.Value.Equals(reader["Is_Half_Reporting_Back_Date"])) ? (bool)reader["Is_Half_Reporting_Back_Date"] : false
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

            var queryString = "SELECT COUNT(*) FROM dbo.Leave_Balance WHERE Employee_ID = " + empID + " AND Leave_Type_ID = " + leaveID;

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

        protected int GetEmpBalanceID(int empID, int leaveTypeID)
        {
            string queryString = "SELECT Leave_Balance_ID FROM dbo.Leave_Balance WHERE Employee_ID = '" + empID + "' AND Leave_Type_ID = '" + leaveTypeID + "'";
            int leaveBalID = 0;

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        leaveBalID = (int)reader["Leave_Balance_ID"];
                    }
                }
                connection.Close();
            }

            return leaveBalID;
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
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(query, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                        connection.Close();
                }
            }
            catch (Exception e) { Output(e.Message); }
        }

        protected Dictionary<int, decimal> DBGetAuditLeaveBalance(int leaveApp)
        {
            string queryString = "SELECT Leave_Balance_ID, Value_Before, Value_After FROM dbo.Audit_Leave_Balance WHERE Leave_Application_ID = '" + leaveApp + "' ORDER BY Modified_On";
            Dictionary<int, decimal> auditLeave = new Dictionary<int, decimal>();

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int balID = (int)reader["Leave_Balance_ID"];
                        string valBefore = (string)reader["Value_Before"];
                        string valAfter = (string)reader["Value_After"];
                        if (auditLeave.ContainsKey(balID))
                            auditLeave[balID] += decimal.Parse(valBefore) - decimal.Parse(valAfter);
                        else
                            auditLeave.Add(balID, decimal.Parse(valBefore) - decimal.Parse(valAfter));
                    }
                }
                connection.Close();
            }

            return auditLeave;
        }

        protected void DBRefundLeaveBalance(int appID)
        {
            Dictionary<int, decimal> audit = DBGetAuditLeaveBalance(appID);

            foreach (KeyValuePair<int, decimal> pair in audit)
            {
                int balID = pair.Key;
                decimal consumedBal = pair.Value;
                decimal previousBalance = DBGetLeaveBalance(balID);
                string queryString = "UPDATE dbo.Leave_Balance SET Balance = '" + (previousBalance+consumedBal) + "' WHERE Leave_Balance_ID = '" + balID + "'";
                DBExecuteQuery(queryString);
                DBUpdateAuditBalance(balID, appID, previousBalance, previousBalance + consumedBal, "Refund All Balances");
            }

        }

        public void SendMail(string email, string message)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");
            mail.To.Add(new MailAddress(email));
            mail.Subject = "Leave Application Update";
            mail.Body = message + Environment.NewLine;

            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("project_ict333@murdochdubai.ac.ae", "ict@333");
            try
            {
                client.Send(mail);
                System.Diagnostics.Debug.WriteLine("Mail Sent");
            }
            catch (Exception e) { }
        }

        protected decimal DBGetLeaveBalance(int balID) {
            var queryString = "SELECT Balance FROM dbo.Leave_Balance WHERE Leave_Balance_ID = '" + balID + "'";
            decimal balance = 0;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        balance = (decimal)reader["Balance"];
                    }
                }
                connection.Close();
            }

            return balance;
        }

        protected void DBUpdateAuditBalance(int leaveBalanceID, int applicationID, decimal valueBefore, decimal valueAfter, string comment)
        {
            string queryString = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                  "VALUES('" + leaveBalanceID + "','" + applicationID + "', 'Balance' ,'" + valueBefore + "','" + valueAfter + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "','" + comment + "')";
            DBExecuteQuery(queryString);
        }

        protected int DBLastIdentity(string colName, string tableName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            string queryString = "SELECT MAX(" + colName + ") AS LastID FROM " + tableName;
            int id = 0;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        id = (int)reader["LastID"];
                    }
                }
                connection.Close();
            }

            return id;
        }

        protected List<lmReporting> GetReportingList(int empID=0)
        {
            List<lmReporting> reportingList = new List<lmReporting>();

            string queryString = "SELECT Report_To_ID, Employee_ID, From_ID, To_ID, Substitution_Level, is_Active " +
                "FROM dbo.Reporting_Map " +
                "FULL OUTER JOIN dbo.Reporting ON dbo.Reporting_Map.Original_ID = Reporting.Report_To_ID";

            if (empID != 0)
                queryString += " WHERE Report_To_ID = '" + empID + "' OR To_ID = '" + empID + "' OR From_ID = '" + empID + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lmReporting reporting = new lmReporting
                        {
                            employeeID = (int)reader["Employee_ID"],
                            reportToID = (int)reader["Report_To_ID"],
                            fromID = (reader["From_ID"] != DBNull.Value) ? (int)reader["From_ID"] : (int?)null,
                            toID = (reader["To_ID"] != DBNull.Value) ? (int)reader["To_ID"] : (int?)null,
                            subLevel = (reader["Substitution_Level"] != DBNull.Value) ? (int)reader["Substitution_Level"] : (int?)null,
                            isActive = (reader["Is_Active"] != DBNull.Value) ? (Boolean)reader["is_Active"] : (Boolean?)null,
                            employeeName = DBEmployeeList()[(int)reader["Employee_ID"]]
                        };
                        reportingList.Add(reporting);
                    }
                }
                connection.Close();
            }

            return reportingList;
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

        protected Dictionary<int, string> DBStaffList(int accountStatus = -1)
        {
            int staffRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "Staff").Key;
            Dictionary<int, string> staffList = new Dictionary<int, string>();

            var queryString = "SELECT Employee.Employee_ID, First_Name, Last_Name " +
                "FROM dbo.Employee, dbo.Employee_Role " +
                "WHERE Employee.Employee_ID = Employee_Role.Employee_ID AND Employee_Role.Role_ID = " + staffRoleID;

            if (accountStatus == 1 || accountStatus == 0)
                queryString += " AND Employee.Account_Status = " + accountStatus;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fullName = (string)reader["First_Name"] + " " + (string)reader["Last_Name"];
                        staffList.Add((int)reader["Employee_ID"], fullName);
                    }
                }
                connection.Close();
            }
            return staffList;

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
            var queryString = "SELECT Leave_Type_ID, Leave_Name FROM dbo.Leave_Type";
            return DBListing(queryString, "Leave_Type_ID", "Leave_Name");
        }

        protected Dictionary<int, string> DBLeaveNameList()
        {
            var queryString = "SELECT Leave_Type_ID, Display_Name FROM dbo.Leave_Type";
            return DBListing(queryString, "Leave_Type_ID", "Display_Name");
        }

        protected Dictionary<int, string> DBNationalityList()
        {
            var queryString = "SELECT Nationality_ID, Display_Name FROM dbo.Nationality";
            return DBListing(queryString, "Nationality_ID", "Display_Name");
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
            var queryString = "SELECT Religion_ID, Display_Name FROM dbo.Religion";
            return DBListing(queryString, "Religion_ID", "Display_Name");
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

        protected bool IsPublicHoliday(DateTime date)
        {
            bool isPublicHoliday = false;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT * FROM dbo.Public_Holiday WHERE Date = '" + date.ToString("yyyy-MM-dd") + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime day = (DateTime)reader["Date"];
                        isPublicHoliday = date.Equals(day) ? true : false;
                    }
                }
                connection.Close();
            }

            return isPublicHoliday;
        }
    }
}