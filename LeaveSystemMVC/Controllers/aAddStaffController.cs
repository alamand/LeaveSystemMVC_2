using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;


namespace LeaveSystemMVC.Controllers
{
    public class aAddStaffController : Controller
    {

        // GET: aAddStaff
        [HttpGet]

        public ActionResult Index()
        {
            return View();
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


    }
}