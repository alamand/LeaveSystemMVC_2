using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.IO;
using LeaveSystemMVC.CustomLibraries;
using System.Globalization;
using System.Data.SqlClient;
using System.Configuration;

namespace LeaveSystemMVC.Controllers
{
    public class aBulkAddStaffController : Controller
    {
        sEmployeeModel tempEmp = new sEmployeeModel();
        [HttpGet]
        // GET: aBulkAddStaff
        public ActionResult Index()
        {
            
            return View(tempEmp);
        }
        
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase upload)
        {
            if(upload != null && upload.ContentLength > 0)
            {
                if(upload.FileName.EndsWith(".csv"))
                {
                    try
                    {
                        CsvFile csv = new CsvFile(Path.Combine(Server.MapPath("~/App_Data"), Path.GetFileName(upload.FileName)));
                        upload.SaveAs(csv.path);
                        var tableRows = csv.readFile();
                        var newEmployees = new List<employeeCons>();
                        foreach(string[] tableRow in tableRows)
                        {
                            int column = 0;
                            var newEmployee = new employeeCons();
                            bool addEmployee = true;
                            foreach (string tableColumn in tableRow)
                            {
                                
                                switch(column)
                                {
                                    case 0:
                                        if(tableColumn.Length > 5)
                                        //> 5 because some IDs might contain less than 5 digits
                                        {
                                            addEmployee = false;
                                            break;
                                        }
                                        int ID;
                                        int.TryParse(tableColumn, out ID);
                                        newEmployee.employeeObject.staffID = ID;
                                        break;
                                    case 1:
                                        newEmployee.employeeObject.firstName = tableColumn;
                                        break;
                                    case 2:
                                        newEmployee.employeeObject.lastName = tableColumn;
                                        break;
                                    case 3:
                                        //username cant be empty, it cant contain no .s, it cant contain spaces
                                        if(tableColumn == "" || !tableColumn.Contains(".") || tableColumn.Contains(" "))
                                        {
                                            addEmployee = false;
                                            break;
                                        }
                                        //if there's nothing after the dot, the username isn't valid
                                        string afterDot = tableColumn.Substring(tableColumn.LastIndexOf('.') + 1);
                                        if(afterDot == "")
                                        {
                                            addEmployee = false;
                                            break;
                                        }
                                        //usernames cannot have more than one .
                                        if(tableColumn.Count(x => x == '.') > 1)
                                        {
                                            addEmployee = false;
                                            break;
                                        }
                                        newEmployee.employeeObject.userName = tableColumn;
                                        break;
                                    case 4:
                                        newEmployee.employeeObject.designation = tableColumn;
                                        break;
                                    case 5:
                                        newEmployee.employeeObject.deptName = tableColumn;
                                        break;
                                    case 6:
                                        newEmployee.employeeObject.gender = tableColumn;
                                        break;
                                    case 7:
                                        /*
                                        DateTime startDate = DateTime.ParseExact(tableColumn, "MM/dd/yyyy",
                                            System.Globalization.CultureInfo.InvariantCulture);*/
                                        DateTime startDate = DateTime.Parse(tableColumn);
                                        newEmployee.employeeObject.empStartDate = startDate;
                                        break;
                                    case 8:
                                        newEmployee.employeeObject.email = tableColumn;
                                        break;
                                    case 9:
                                        newEmployee.roles.Add(tableColumn);
                                        break;
                                    case 10:
                                        newEmployee.roles.Add(tableColumn);
                                        break;
                                    case 11:
                                        newEmployee.roles.Add(tableColumn);
                                        break;

                                    case 12:
                                        newEmployee.employeeObject.phoneNo = tableColumn;
                                        break;
                                    case 13:
                                        int annual;
                                        int.TryParse(tableColumn, out annual);
                                        newEmployee.balances.annual = annual;
                                        break;
                                    case 14:
                                        int maternity;
                                        int.TryParse(tableColumn, out maternity);
                                        newEmployee.balances.maternity = maternity;
                                        break;
                                    case 15:
                                        int sick;
                                        int.TryParse(tableColumn, out sick);
                                        newEmployee.balances.sick = sick;
                                        break;
                                    case 16:
                                        int compassionate;
                                        int.TryParse(tableColumn, out compassionate);
                                        newEmployee.balances.compassionate = compassionate;
                                        break;
                                    case 17:
                                        int dil;
                                        int.TryParse(tableColumn, out dil);
                                        newEmployee.balances.daysInLieue = dil;
                                        break;
                                    case 18:
                                        int hours;
                                        int.TryParse(tableColumn, out hours);
                                        newEmployee.balances.shortLeaveHours = hours;
                                        break;

                                } // end of switch
                                if (!addEmployee)
                                    break;
                                newEmployee.employeeObject.password = RandomPassword.Generate(7, 7);
                                column++;
                            } // end of tablecolumn foreach
                            if(addEmployee)
                                newEmployees.Add(newEmployee);

                            column = 0;
                        } // end of tablerow foreach

                        foreach(var employee in newEmployees)
                        {
                            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                            //Check if employee with same ID and username already exists.
                            //If it does then move on to the next employee.
                            string queryString = "SELECT Employee_ID, User_Name FROM dbo.Employee WHERE Employee_ID = "
                                + employee.employeeObject.staffID + " OR User_Name = '" + employee.employeeObject.userName + "'";
                            using (var connection = new SqlConnection(connectionString))
                            {
                                var command = new SqlCommand(queryString, connection);
                                connection.Open();
                                using (var reader = command.ExecuteReader())
                                {
                                    bool toContinue = false;
                                    while(reader.Read())
                                    {
                                        toContinue = true;
                                        break;
                                    }
                                    if (toContinue)
                                        continue;
                                }
                                connection.Close();
                            }
                            //End check for existing employee

                            //Insert employee data into the database table
                            queryString = "INSERT INTO dbo.Employee (Employee_ID, First_Name, " +
                                "Last_Name, User_Name, Password, Designation, Email, Gender, PH_No, " +
                                "Emp_Start_Date, Account_Status) VALUES('" + employee.employeeObject.staffID +
                                "', '" + employee.employeeObject.firstName + "', '" + employee.employeeObject.lastName +
                                "', '" + employee.employeeObject.userName +
                                "', '" + employee.employeeObject.password + "', '" + employee.employeeObject.designation +
                                "', '" + employee.employeeObject.email + "', '" + employee.employeeObject.gender +
                                "', '" + employee.employeeObject.phoneNo + "', '" + employee.employeeObject.empStartDate +
                                "', '" + "True" + "')";
                            using (var connection = new SqlConnection(connectionString))
                            {
                                var command = new SqlCommand(queryString, connection);
                                connection.Open();
                                using (var reader = command.ExecuteReader())
                                    connection.Close();
                            }
                            //End employee insertion

                            //Employee Balances insertion
                            //lets assume that all the leave types and
                            //their appropriate max balances will be included
                            //in the database from the getgo
                            //
                            for(int i = 0; i <= 5; i++)
                            {
                                int balance = 0;
                                int employeeID = employee.employeeObject.staffID;
                                int leaveID = 0;
                                switch(i)
                                {
                                    case 0:
                                        balance = employee.balances.annual;
                                        leaveID = 1;
                                        break;
                                    case 1:
                                        balance = employee.balances.maternity;
                                        leaveID = 2;
                                        break;
                                    case 2:
                                        balance = employee.balances.sick;
                                        leaveID = 3;
                                        break;
                                    case 3:
                                        balance = employee.balances.compassionate;
                                        leaveID = 4;
                                        break;
                                    case 4:
                                        balance = employee.balances.daysInLieue;
                                        leaveID = 5;
                                        break;
                                    case 5:
                                        balance = employee.balances.shortLeaveHours;
                                        leaveID = 6;
                                        break;
                                } // end of switch

                                queryString = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) " +
                                "VALUES ('" + employeeID + "', '" + leaveID + "', '" + balance +  "')";

                                using (var connection = new SqlConnection(connectionString))
                                {
                                    var command = new SqlCommand(queryString, connection);
                                    connection.Open();
                                    using (var reader = command.ExecuteReader())
                                        connection.Close();
                                }
                            }
                            //end employee balances insertion

                            //Employee roles insertion
                            foreach(var role in employee.roles)
                            {
                                int roleID = 0;
                                queryString = "SELECT Role_ID FROM dbo.Role WHERE Role_Name = '" + role + "'";
                                using (var connection = new SqlConnection(connectionString))
                                {
                                    var command = new SqlCommand(queryString, connection);
                                    connection.Open();
                                    using (var reader = command.ExecuteReader())
                                    {
                                        while(reader.Read())
                                        {
                                            roleID = (int)reader[0];
                                        }
                                    }
                                    connection.Close();
                                }
                                if (roleID == 0)
                                    continue;

                                queryString = "INSERT INTO dbo.Employee_Role (Employee_ID, Role_ID) " +
                                    "VALUES ('" + employee.employeeObject.staffID + "', '" + roleID +"')";
                                using (var connection = new SqlConnection(connectionString))
                                {
                                    var command = new SqlCommand(queryString, connection);
                                    connection.Open();
                                    using (var reader = command.ExecuteReader())
                                        connection.Close();
                                }
                            }
                            //End employee roles insertion
                        }

                        /* I tried having only a single connection open for adding all of the entries
                         * gave an error saying i need to close the previous connection for new one
                        var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                        using (var connection = new SqlConnection(connectionString))
                        {
                            connection.Open();
                            foreach (var employee in newEmployees)
                            {
                                string queryString = "INSERT INTO dbo.Employee (Employee_ID, First_Name, " +
                                    "Last_Name, User_Name, Password, Designation, Email, Gender, PH_No, " +
                                    "Emp_Start_Date, Account_Status) VALUES('" + employee.employeeObject.staffID + 
                                    "', '" + employee.employeeObject.firstName + "', '" + employee.employeeObject.lastName +
                                    "', '" + employee.employeeObject.userName +
                                    "', '" + employee.employeeObject.password + "', '" + employee.employeeObject.designation +
                                    "', '" + employee.employeeObject.email + "', '" + employee.employeeObject.gender +
                                    "', '" + employee.employeeObject.phoneNo + "', '" + employee.employeeObject.empStartDate +
                                    "', '" + "True" + "')";

                                var command = new SqlCommand(queryString, connection);

                                command.ExecuteReader();
                            }
                            connection.Close();
                        }*/


                        ViewBag.Message = "File uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }
                }
                else
                {
                    ViewBag.Message = "File type not supported";
                    ModelState.AddModelError("upload", "The file type is not supported");
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file";
            }
            tempEmp.firstName = upload.FileName;
            return View(tempEmp);
        }
    }

    public class employeeCons // EmployeeConsolidated
    {
        public sEmployeeModel employeeObject;
        public List<string> roles;
        public sleaveBalanceModel balances;

        public employeeCons()
        {
            employeeObject = new sEmployeeModel();
            roles = new List<string>();
            balances = new sleaveBalanceModel();
        }

        public string autoUsername()
        {
            string username = "";

            return username;
        }
    }

}