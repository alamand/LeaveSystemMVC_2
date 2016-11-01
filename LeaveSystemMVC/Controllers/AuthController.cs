using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using LeaveSystemMVC.CustomLibraries;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Claims;

namespace LeaveSystemMVC.Controllers
{
    [AllowAnonymous]
    public class AuthController : Controller
    {
        // GET: Auth
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Users model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.UserID > 0)
            {
                int empID = 0;
                string password = "";
                string firstName = "";
                string lastName = "";
                string empRole = "";

                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                string queryString = "Select Employee_ID, Password, First_Name, Last_Name FROM dbo.Employee WHERE Employee_ID = " + model.UserID;
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(queryString, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            empID = (int)reader[0];
                            password = (string)reader[1];
                            firstName = (string)reader[2];
                            lastName = (string)reader[3];
                        }
                    }
                    queryString = "Select Role_Name From dbo.Role Full Join dbo.Employee_Role On dbo.Role.Role_ID = dbo.Employee_Role.Role_ID Where dbo.Employee_Role.Employee_ID = " + model.UserID;
                    command = new SqlCommand(queryString, connection);
                    using (var reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            empRole = (string)reader[0];
                        }
                    }
                    connection.Close();
                }


                if (model.UserID == empID && model.Password == password)
                {
                    string idString = empID.ToString();
                    string fullNameString = firstName + " " + lastName;
            
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, fullNameString),
                        new Claim(ClaimTypes.NameIdentifier,  idString),
                        new Claim(ClaimTypes.Role, empRole)
                    };
                    var identity = new ClaimsIdentity(claims, "ApplicationCookie");

                    var ctx = Request.GetOwinContext();
                    var authManager = ctx.Authentication;

                    //Makes our claims list persist throughout the application session
                    authManager.SignIn(identity);

                    return RedirectToAction("Index", "LApplication");
                }
            }

            ModelState.AddModelError("", "Invalid UserID or Password");
            return View(model);
        }

        public ActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;

            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Login", "Auth");
        }
    }
}