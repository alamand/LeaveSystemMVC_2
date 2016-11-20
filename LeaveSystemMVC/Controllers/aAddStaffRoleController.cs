
//Adding a new staff role controller 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;


namespace LeaveSystemMVC.Controllers
{
    public class aAddStaffRoleController : Controller
    {
        // GET: aAddStaffRole
        [HttpGet]
        public ActionResult Index()
        {
            List<string> roleNames = new List<string>(); //used to generate a list of existing staff roles 

            //to get the list of staff roles from the system 
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Role_Name FROM dbo.Role ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        roleNames.Add((string)reader[0]);
                    }
                }
                connection.Close();
                ViewBag.roleNames = roleNames; //displays the list 
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index (Models.aStaffRole newRoleName)
        {
            //checks if new staff role name is filled 
            if (newRoleName.staffRoleName == null)
            {
                ModelState.AddModelError("staffRoleName", "New Staff Role Name must be entered");
            }

            //checks if staff role already exists 
            List<string> staffRoleName = new List<string>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Role_Name FROM dbo.Role ";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        staffRoleName.Add((string)reader[0]);
                    }
                }
                connection.Close();
            }
            foreach (var name in staffRoleName)
            {
                if (newRoleName.staffRoleName.ToLower() == name.ToLower())
                {
                    ModelState.AddModelError("staffRoleName", "Staff Role name already exsists");

                }
            }

            //if the checks are passed 
            //gets the enterd staff role name and adds it to the database 
            if (ModelState.IsValid)
                {
                    connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    string insertString = "INSERT INTO dbo.Role (Role_Name) VALUES (' " + newRoleName.staffRoleName + "')";
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var command = new SqlCommand(insertString, connection);
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                            connection.Close();
                    }
                Response.Write("<script> alert ('Successfully created a new staff role');location.href='Index'</script>");
            }
            return Index() ;
        } 
    }
}