using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Security.Claims;
using System.Collections.Generic;
using Microsoft.Owin.Security;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        // GET: Auth
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User model)
        {
            if(!ModelState.IsValid)
                return View(model);

            if (model.UserID == null)
            {
                ModelState.AddModelError("", "Invalid UserID or Password");
                return View(model);
            }

            int enteredID;
            bool isLoginNumeric = int.TryParse(model.UserID, out enteredID);

            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();

            if (isLoginNumeric)
            {
                cmd.Parameters.Add("@empID", SqlDbType.Int).Value = enteredID;
                cmd.Parameters.Add("@password", SqlDbType.NChar).Value = model.Password;
                cmd.CommandText = "SELECT Employee_ID FROM dbo.Employee WHERE Employee_ID = @empID AND Password = @password";
            }
            else
            {
                cmd.Parameters.Add("@username", SqlDbType.NChar).Value = model.UserID;
                cmd.Parameters.Add("@password", SqlDbType.NChar).Value = model.Password;
                cmd.CommandText = "SELECT Employee_ID FROM dbo.Employee WHERE User_Name = @username AND Password = @password";
            }

            DataTable dataTable = db.Fetch(cmd);

            if (dataTable.Rows.Count == 0)
            {
                ModelState.AddModelError("", "Invalid UserID or Password");
                return View(model);
            }

            enteredID = (int)dataTable.Rows[0]["Employee_ID"];
            Employee employee = GetEmployeeModel(enteredID);

            if (!employee.accountStatus)
            {
                ModelState.AddModelError("", "Account is disabled");
                return View(model);
            }

            authenticateClaim(employee);
            return RedirectToAction("Index", "Home");
        }

        private void authenticateClaim(Employee employee)
        {
            string idString = employee.staffID.ToString();
            string fullName = employee.firstName + " " + employee.lastName;
            Dictionary dic = new Dictionary();
            Dictionary<int, string> roles = dic.GetRole();

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, fullName));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, idString));

            if (employee.isStaff)
                claims.Add(new Claim(ClaimTypes.Role, "Staff"));

            if (employee.isLM)
                claims.Add(new Claim(ClaimTypes.Role, "LM"));

            if (employee.isHR)
                claims.Add(new Claim(ClaimTypes.Role, "HR"));

            if (employee.isHRResponsible)
                claims.Add(new Claim(ClaimTypes.Role, "HR_Responsible"));

            if (employee.isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            var identity = new ClaimsIdentity(claims, "ApplicationCookie");
            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;

            //Makes our claims list persist throughout the application session
            authManager.SignIn(new AuthenticationProperties { IsPersistent = true }, identity);

        }

        /*Look into @ http://stackoverflow.com/questions/26182660/how-to-logout-user-in-owin-asp-net-mvc5 
        http://stackoverflow.com/questions/17592530/cookie-is-not-delete-in-mvcc */
        public ActionResult Logout()
        {
            var ctx = Request.GetOwinContext();
            var authManager = ctx.Authentication;
            authManager.SignOut("ApplicationCookie");
            return RedirectToAction("Login", "Auth");
        }
    }
}
