/*
 * Author: M Hamza Rahimy
 */
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

            if (model.UserID != null)
            {
                int enteredID;
                bool isLoginNumeric = int.TryParse(model.UserID, out enteredID);

                /*Data returned from the database will
                 be assigned to these variables.
                 The critical ones are password, username
                 and empID.*/
                int empID = -1;
                string password = "";
                string firstName = "";
                string lastName = "";
                string username = "";
                bool isActivated = false;
                /////////////////////

                List<string> empRoles = new List<string>();
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                string queryString;
                if(isLoginNumeric)
                {
                    queryString = "Select Employee_ID, Password, First_Name, Last_Name, User_Name, Account_Status FROM dbo.Employee WHERE Employee_ID = '" + enteredID + "'";
                }
                else
                {
                    queryString = "Select Employee_ID, Password, First_Name, Last_Name, User_Name, Account_Status FROM dbo.Employee WHERE User_Name = '" + model.UserID + "'";
                }
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
                            username = (string)reader[4];
                            isActivated = (bool)reader[5];
                        }
                    }
                    if(!isActivated)
                    {
                        connection.Close();
                        ModelState.AddModelError("", "Account is disabled");
                        return View(model);
                    }
                    //This if statement is unnecessary for the login procedure but it saves clock cycles.
                    //When we don't have this if statement, we will go through the roles table trying to 
                    //find roles that correspond to the employee ID -1 in the Employee_Role table.
                    //Obviously it won't return anything unless the database is actually storing 
                    //employee ID numbers below 0.
                    //
                    if (empID >= 0) 
                    {
                        queryString = "Select Role_Name From dbo.Role Full Join dbo.Employee_Role On dbo.Role.Role_ID = dbo.Employee_Role.Role_ID Where dbo.Employee_Role.Employee_ID = '" + empID + "'";
                        command = new SqlCommand(queryString, connection);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                empRoles.Add((string)reader[0]);
                            }
                        }
                    }
                    connection.Close();
                }


                /*Check if the user entered an employeeID or username
                    and then check if the username and password that were entered
                    matches the ones returned from the database, if returned at all.
                    Obviously if nothing was returned from the database than it
                    will be a negative match.*/
                if (isLoginNumeric)
                {
                    if (enteredID == empID && model.Password == password)
                    {
                        string fullName = firstName + " " + lastName;
                        authenticateClaim(empID, fullName, empRoles);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    if (model.UserID.Equals(username) && model.Password == password)
                    {
                        string fullName = firstName + " " + lastName;
                        authenticateClaim(empID, fullName, empRoles);
                        return RedirectToAction("Index", "Home");
                    }
                }


            }

            ModelState.AddModelError("", "Invalid UserID or Password");
            return View(model);
        }

        /*This implementation makes specific user data such as name, 
         ID and roles persistent throughout their session until the
         Logout action is called. The Login is effectively incomplete
         without this.*/
        private void authenticateClaim(int empID, string fullNameString, List<string> empRoles)
        {
            string idString = empID.ToString();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, fullNameString),
                new Claim(ClaimTypes.NameIdentifier, idString)

            };
            foreach (var role in empRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "ApplicationCookie");

            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;

            //Makes our claims list persist throughout the application session
            authManager.SignIn(identity);

        }

        /*Look into @ http://stackoverflow.com/questions/26182660/how-to-logout-user-in-owin-asp-net-mvc5 
        http://stackoverflow.com/questions/17592530/cookie-is-not-delete-in-mvcc
             */
        public ActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;

            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Login", "Auth");
        }
    }
}