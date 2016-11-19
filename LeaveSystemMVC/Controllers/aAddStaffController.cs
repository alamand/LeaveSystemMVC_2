using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Dynamic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class aAddStaffController : Controller
    {
        // GET: aAddStaff
        [HttpGet]
        public ActionResult Index()
        {
            /*The employee model object that will be passed into the view*/
            sEmployeeModel EmptyEmployee = new sEmployeeModel();

            //Intermediary staff roles/types selection list

            //Get list of available roles
            var connectionString = 
                ConfigurationManager.ConnectionStrings["DefaultConnection"].
                ConnectionString;
            var queryString = "SELECT Role_ID, Role_Name FROM dbo.Role";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        EmptyEmployee.staffTypeSelectionOptions.Add((int)reader[0], (string)reader[1]);
                    }
                }
                connection.Close();
            }

            //We should have all role types from the database now
            //end get roles

            //Get all departments for selection
            queryString = "SELECT Department_ID, Department_Name FROM dbo.Department";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        EmptyEmployee.departmentList.Add((int)reader[0], (string)reader[1]);
                    }
                }
                connection.Close();
            }
            //End get departments

            //all these lists should not be hardcoded

            List<string> sid = new List<string>();
            sid.Add("None");
            sid.Add("12345678");
            sid.Add("22345678");
            sid.Add("33345678");
            sid.Add("44445678");
            ViewBag.sid = sid;

            List<string> slm = new List<string>();
            slm.Add("None");
            slm.Add("Sukhpreet Singh Sidhu");
            slm.Add("Bidisha Sen");
            slm.Add("Mandy Northover");
            slm.Add("Dan Adkins");
            ViewBag.slm = slm;

            List<string> department = new List<string>();
            department.Add("None");
            department.Add("IT");
            department.Add("Academics");
            department.Add("HR");
            department.Add("Student Services");
            ViewBag.department = department;

            List<string> staffType = new List<string>();
            staffType.Add("None");
            staffType.Add("Admin");
            staffType.Add("Line  Manager");
            staffType.Add("HR");
            staffType.Add("Staff Member");
            ViewBag.staffType = staffType;
            return View(EmptyEmployee);
        }

        

        [HttpPost]
        public ActionResult Index(LeaveSystemMVC.Models.sEmployeeModel SE)
        {
            string temp_email = SE.email;
            string temp_username = SE.userName;

            MailMessage message = new MailMessage();
            message.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");



            message.To.Add(new MailAddress(temp_email));

            message.Subject = "Your User Details";
            string body = "";
            body = body + "Hi, Your user details are: username: " + temp_username + " and your password is: " + LeaveSystemMVC.Models.RandomPassword.Generate(7, 7);

            message.Body = body;
            SmtpClient client = new SmtpClient();

            client.EnableSsl = true;
            
            client.Credentials = new NetworkCredential("project_ict333@murdochdubai.ac.ae", "ict@333");

            client.Send(message);
            return Index();
        }

        /*Function referred to from @ http://nimblegecko.com/using-simple-drop-down-lists-in-ASP-NET-MVC/
         * Essentially encapsulates the creation of a list of SelectItem. Or a SelectList object.
         * Please note that this function is not being used because a better solution has replaced 
          it's specific use for creating dropdown lists out of dictionary objects. However, this might
          become useful later(possibly in other projects). This (especially the link) will be kept here
          essentially for future reference because it helped clear up a lot of confusions.*/
        private IEnumerable<SelectListItem> GetDropdownListItems(Dictionary<int, string> elements)
        {
            var selectList = new List<SelectListItem>();

            foreach(var element in elements)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.Key.ToString(),
                    Text = element.Value
                });
            }

            return selectList;
        }

    }
}