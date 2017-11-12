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
    public class hrSelectSubstituteController : Controller
    {
        // GET: hrSelectSubstitute
        public ActionResult Index()
        {

            string userID = " ";

            //to get the id of the person logged in as 
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (c != null)
                {
                    userID = c.Value;
                }

            }

            int tempDept=0;
            int tempID = 0;
            selectSubstitute substitute = new selectSubstitute();

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            //Include the employees who work in the HR department
            string fullName = "";
            string searchString = "Select Employee_ID, First_Name, Last_Name From dbo.Employee Where Department_ID = 5 AND Employee_ID !='" + userID + "'AND Account_Status != 'False'";
            //@todo: remove hardcoding of department_id
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(searchString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fullName = (string)reader[1] + " " + (string)reader[2];
                        substitute.substituteListOptions.Add((int)reader[0], fullName);
                    }
                }
            }

            // Include the employees who are line managers                      
            string queryString = "Select dbo.Employee.Employee_ID, First_Name, Last_Name FROM dbo.Employee_Role, dbo.Employee WHERE " +
                "dbo.Employee_Role.Employee_ID = dbo.Employee.Employee_ID" +
                " AND Role_ID = 4 AND dbo.Employee.Employee_ID !='" + userID + "'AND Account_Status != 'False'";
            //@todo: remove hardcoding of role_id
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        fullName = (string)reader[1] + " " + (string)reader[2];
                        System.Diagnostics.Debug.WriteLine("name is: " + fullName);
                        if (!substitute.substituteListOptions.ContainsKey((int)reader[0]))
                        {
                            substitute.substituteListOptions.Add((int)reader[0], fullName);
                        }
                    }
                }
                connection.Close();
            }
            return View(substitute);
        }

        [HttpPost]
        public ActionResult Index(Models.selectSubstitute newSubstitute)
        {
            int tempSubstituteID = 0;
            string userID = " ";

            //to get the id of the person logged in as 
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (c != null)
                {
                    userID = c.Value;
                }

            }

            //to get the id of the selected person 
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Employee.Employee_ID From dbo.Employee Where Employee_Id='" + newSubstitute.substituteStaffID + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                    {
                        tempSubstituteID = (int)reader[0];
                    }
                connection.Close();
            }

            //The current HR Responsible becomes just HR 
            string insertString = "Update dbo.Employee_Role SET dbo.Employee_Role.Role_ID ='3' Where dbo.Employee_Role.Employee_ID ='" + userID + "' AND dbo.Employee_Role.Role_ID = '2'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(insertString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                connection.Close();
            }

            //The selected substitute becomes HR Responsible 
            insertString = "Update dbo.Employee_Role SET Role_ID ='2' Where Employee_ID ='" + tempSubstituteID + "' AND dbo.Employee_Role.Role_ID = '3'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(insertString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                connection.Close();
                Response.Write("<script> alert ('Successfully changed HR Responsible role.')</script>");
            }

            return Index();
        }
    }
}