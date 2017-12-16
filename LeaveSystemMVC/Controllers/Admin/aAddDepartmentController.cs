
//Adding a Department Controller 

using System.Collections.Generic;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class aAddDepartmentController : Controller
    {
        // GET: aAddDepartment
        public ActionResult Index()
        {
            List<string> departmentNames = new List<string>(); //list to display department names 

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
                
                connection.Close();
                ViewBag.departmentNames = departmentNames; //sends the list of departments to the view 
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(Models.aDepartment newDepartmentName)
        {
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
                    //adding the entered department name into the database 
                    newDepartmentName.departmentName = System.Text.RegularExpressions.Regex.Replace(newDepartmentName.departmentName, @"'", "");
                    string insertString = "Insert Into dbo.Department (Department_Name) VALUES ('" + newDepartmentName.departmentName + "')";
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var command = new SqlCommand(insertString, connection);
                        connection.Open();
                        using (var reader = command.ExecuteReader())
                            connection.Close();
                    }
                    Response.Write("<script> alert ('Successfully added a new department.')</script>");
                }
                return Index();
            }
        }
    }
}