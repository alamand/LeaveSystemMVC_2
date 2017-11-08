using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;
using System.Security.Claims;

namespace LeaveSystemMVC.Controllers
{
    public class lmSelectSubstituteController : Controller
    {
        
        // GET: lmSubstitute
        public ActionResult Index()
        {
            int deptID = 0;
            string userID=" ";
            selectSubstitute substitute = new selectSubstitute();
            string fullName = "";

            //to get the id of the person logged in 
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            if (claimsIdentity != null)
            {
                var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (c != null)
                {
                    userID = c.Value;
                }

            }
            //display the list of employees that work under the current logged in line manager 
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Department_ID FROM dbo.Department Where Line_Manager_ID='" + userID + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deptID = (int)reader[0];
                        string searchString = "Select Employee.Employee_ID, First_Name, Last_Name FROM dbo.Employee Where Department_ID='" + deptID + "' AND Employee_ID !='" + userID + "'AND Account_Status != 'False'";
                        using (var connection1 = new SqlConnection(connectionString))
                        {
                            command = new SqlCommand(searchString, connection1);
                            connection1.Open();
                            using (var readerA = command.ExecuteReader())
                            {
                                while (readerA.Read())
                                {
                                    fullName = (string)readerA[1] + " " + (string)readerA[2];
                                    substitute.substituteListOptions.Add((int)readerA[0], fullName);
                                }
                            }
                            connection1.Close();
                        }
                    }
                }
                connection.Close();
            }
            return View(substitute);
        }
        
        //to add or remove substitute lm from db 
        [HttpPost]
        public ActionResult Index (Models.selectSubstitute newSubstitute)
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

            int tempSubstituteID = 0;
            
            if (newSubstitute.toTransferBack)
            {
                //to remove the substitute id from the database 
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                string insertString = "Update dbo.Department SET Substitute_LM_ID = NULL Where Line_Manager_ID= '"+userID+"'";
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(insertString, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                        connection.Close();
                }
                Response.Write("<script> alert ('Successfully transfered authority back')</script>");
            }
            else
            
            {
                //get the name selected in the list to appoint as substitute
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
                   //System.Diagnostics.Debug.WriteLine("This the LM ID - " + tempSubstituteID);
                }

                //add the id of the substitute into the databse 
                string insertString = "Update dbo.Department SET Substitute_LM_ID ='" + tempSubstituteID + "' Where Line_Manager_ID='"+userID+"'";
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(insertString, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                        connection.Close();
                }
                Response.Write("<script> alert ('Successfully added substitute line manager')</script>");
            }
            return Index();
        } 
    }
}