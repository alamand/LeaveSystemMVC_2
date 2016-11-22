
//Editing a Department controller 

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
    public class aEditDepartmentController : Controller
    { 
        // GET: aEditDepartment
        public ActionResult Index()
        {
            aDepartment EmptyDepartment = new aDepartment(); //object created to make a list of LMs with their IDs and Names stored in it
            List<string> departmentNames = new List<string>(); //list to display department names 
            int temproleID = 0;

            string tempLMName = "bla";
            ViewBag.tempLMName = tempLMName;

            //displays a list of departments in the system for the user to select one to edit 
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

                queryString = "Select Role_Id FROM dbo.Role Where Role_Name= 'LM'";
                command = new SqlCommand(queryString, connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        temproleID = (int)reader[0];
                    }
                }
                //System.Diagnostics.Debug.WriteLine("This the LM role ID - " + temproleID);

                //display a list of LM's for the user to select a new one for a particular department 
                queryString = "Select Employee.Employee_ID, First_Name,Last_Name FROM dbo.Employee Full Join dbo.Employee_Role On dbo.Employee_Role.Employee_ID = dbo.Employee.Employee_ID WHERE Employee_Role.Role_ID ='" + temproleID + "'";
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
        public ActionResult Index(Models.aDepartment editDepartmentName)
        {
            int tempID = 0; //to temporarily store LM ID before adding to the database 

            //start
            string tempLMName = "bla";
            int tempLMID = 0; 
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString="Select Line_Manager_ID From dbo.Department Where Department_Name= '" + editDepartmentName.departmentName + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())

                    while (reader.Read())
                    {
                        tempLMID = (int)reader[0];
                    }
            }

            queryString = "Select First_Name, Last_Name from dbo.Employee Where Employee_Id ='" +tempLMID +"'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())

                    while (reader.Read())
                    {
                        tempLMName = (string)reader[0] + " " + (string)reader[1];
                    }
                System.Diagnostics.Debug.WriteLine("This the LM Name "+tempLMName);
                connection.Close();
            }

            ViewBag.tempLMName = tempLMName; //to display current lm in the view 
            //end



            if (ModelState.IsValid)
                {
                    // get slected lm's ID 
                    //var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    string getIDString = "Select Employee_ID From dbo.Employee Where Employee_ID= '" + editDepartmentName.primaryLMID + "'";
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var command = new SqlCommand(getIDString, connection);
                        connection.Open();
                        using (var reader = command.ExecuteReader())

                            while (reader.Read())
                            {
                                tempID = (int)reader[0];

                            }
                        //System.Diagnostics.Debug.WriteLine("This the LM ID"+tempID);
                        connection.Close();
                    }

                    //update the selected lm id along on to the selected department in the database 
                    string insertString = "UPDATE dbo.Department SET Line_Manager_ID = '" + tempID + "' WHERE Department_Name = '" + editDepartmentName.departmentName + "'";
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var command = new SqlCommand(insertString, connection);
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                            connection.Close();
                    }
                    Response.Write("<script> alert ('Successfully edited the department');location='Index'</script>");
                }
            return Index();
        }
    }
}


