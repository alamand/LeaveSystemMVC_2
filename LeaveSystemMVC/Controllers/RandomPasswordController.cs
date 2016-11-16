using LeaveSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class RandomPasswordController : Controller
    {
        // GET: RandomPassword
        [HttpGet]
        public ActionResult RandomPassword()
        {
            return View();
        }

        // POST: RandomPassword
        [HttpPost]
        public ActionResult RandomPassword(LeaveSystemMVC.Models.RandomPassword model)
        {
            System.Diagnostics.Debug.WriteLine(LeaveSystemMVC.Models.RandomPassword.Generate(7, 7));

            MailMessage message = new MailMessage();
            message.From = new MailAddress("project_ict333@murdochdubai.ac.ae", "GIMEL LMS");



            message.To.Add(new MailAddress("kunal.shah@murdochdubai.ac.ae"));

            message.Subject = "This is my subject";

            message.Body = "This is the content";

            SmtpClient client = new SmtpClient();

            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("project_ict333@murdochdubai.ac.ae", "ict@333");

            client.Send(message);
            return View();
        }

        
    }
}