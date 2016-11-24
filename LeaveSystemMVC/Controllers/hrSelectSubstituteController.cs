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

            //for the next id to get the list of hr employeees
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Department_ID, Line_Manager_ID from dbo.Department Where Department_Name = 'HR' ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        tempDept = (int)reader[0];
                        tempID = (int)reader[1];
                    }
                }
            }


            //to show viable candidates who work in the hr department who are not currently either hr responsible or hr line manager 
            string searchString = "Select Employee_ID, First_Name, Last_Name From dbo.Employee Where Department_ID='"+tempDept+"' AND Employee_ID !='"+tempID+"' AND Employee_ID !='"+userID+"'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(searchString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string fullName = (string)reader[1] + " " + (string)reader[2];
                        substitute.substituteListOptions.Add((int)reader[0], fullName);
                    }
                }
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

            //to update current hr responsible as hr 
            string insertString = "Update dbo.Employee_Role SET Role_ID ='3' Where Employee_ID='" + userID + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(insertString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                connection.Close();
            }

            //to update the selected person as hr responsible 
            insertString = "Update dbo.Employee_Role SET Role_ID ='2' Where Employee_ID='" + tempSubstituteID + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(insertString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                connection.Close();
                Response.Write("<script> alert ('Successfully changed HR Responsible')</script>");
            }

            return Index();
        }
    }
}