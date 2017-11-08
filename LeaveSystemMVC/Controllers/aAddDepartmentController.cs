
//Adding a Department Controller 

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
    public class aAddDepartmentController : Controller
    {
        // GET: aAddDepartment
        public ActionResult Index()
        {
            
            aDepartment EmptyDepartment = new aDepartment();  //object created to make a list of LMs with their IDs and Names stored in it
            List<string> departmentNames = new List<string>(); //list to display department names 
            int temproleID= 0 ;

            //displays a list of existing departments for the user's reference 
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Department_Name FROM dbo.Department "; 
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        departmentNames.Add((string)reader[0]);
                    }
                }

                //for the next query to get the role id of line manager 
                queryString = "Select Role_Id FROM dbo.Role Where Role_Name= 'LM'";
                command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        temproleID= (int)reader[0];
                    }
                }
                System.Diagnostics.Debug.WriteLine("This is the LM role ID - " + temproleID);

                //displays a list of employees who fall under the staff type category Line Manager 
                queryString = "Select Employee.Employee_ID, First_Name,Last_Name FROM dbo.Employee Full Join dbo.Employee_Role On dbo.Employee_Role.Employee_ID = dbo.Employee.Employee_ID WHERE Employee_Role.Role_ID ='"+temproleID +"' AND Account_Status != 'False'"; //display list of lms 
                command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fullName = (string)reader[1] + " " + (string)reader[2];
                        EmptyDepartment.linemanagerSelectionListOptions.Add((int)reader[0], fullName); 
                    }
                }
                connection.Close();
                ViewBag.departmentNames = departmentNames; //sends the list of departments to the view 
            }

            return View(EmptyDepartment); //sends the list of line managers to the view 
        }

        [HttpPost]
        public ActionResult Index(Models.aDepartment newDepartmentName)
        {
            int tempID = 0; //to temporarily store LM ID before adding to the database 

            System.Diagnostics.Debug.WriteLine("The new department name is: " + newDepartmentName.departmentName);
            //checks if new department name is entered
            if (newDepartmentName.departmentName == "" || newDepartmentName.departmentName == null)
            {
                System.Diagnostics.Debug.WriteLine("The new department name is null");
                ModelState.AddModelError("departmentName", "Please enter a department name.");
                return Index();
            }
            else
            {
                //checks if department name already exists 
                List<string> departmentNames = new List<string>();
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                string queryString = "Select Department_Name FROM dbo.Department ";
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(queryString, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departmentNames.Add((string)reader[0]);
                        }
                    }
                    connection.Close();
                }
                foreach (var department in departmentNames)
                {
                    if (newDepartmentName.departmentName.ToLower() == department.ToLower())
                    {
                        ModelState.AddModelError("departmentName", "Department name already exists");

                    }
                }

                //checks if department name has more than 30 charecters
                if (newDepartmentName.departmentName.Length > 30)
                {
                    ModelState.AddModelError("departmentName", "Department name too long");
                }

                //checks if department name has integers in it
                foreach (char c in newDepartmentName.departmentName)
                {
                    if (char.IsDigit(c))
                    {
                        ModelState.AddModelError("departmentName", "Department name cannot contain digits");
                    }
                }

                //if above checks are passed
                if (ModelState.IsValid)
                {
                    //selecting the lm's id 
                    connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    string getIDString = "Select Employee_ID From dbo.Employee Where Employee_ID='" + newDepartmentName.primaryLMID + "'";

                    using (var connection = new SqlConnection(connectionString))
                    {
                        var command = new SqlCommand(getIDString, connection);
                        connection.Open();
                        using (var reader = command.ExecuteReader())

                            while (reader.Read())
                            {
                                tempID = (int)reader[0];
                            }

                        connection.Close();
                        //System.Diagnostics.Debug.WriteLine("This the LM ID - " + tempID);
                    }

                    //adding the slected lm id and the entered department name into the database 
                    newDepartmentName.departmentName = System.Text.RegularExpressions.Regex.Replace(newDepartmentName.departmentName, @"'", "");
                    string insertString = "Insert Into dbo.Department (Line_Manager_ID, Department_Name) VALUES ('" + tempID + "','" + newDepartmentName.departmentName + "')";
                    System.Diagnostics.Debug.WriteLine("insertString:", newDepartmentName.departmentName);
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var command = new SqlCommand(insertString, connection);
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                            connection.Close();
                    }
                    Response.Write("<script> alert ('Successfully added a new department')</script>");
                }
                return Index();
            }
        }
    }
}