using System;
using System.Diagnostics;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using Hangfire;

namespace LeaveSystemMVC.Models
{
    public class Email
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public void ApprovedLeaveApplication(Employee employee, Employee employeeLM, Employee employeeHR,Leave leave)
        {
            string message;

            // message to the applicant
            message = "Your leave application has been fully approved";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "Johns leave application has been fully approved";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));


            // message to the human resources
            message = "You have approved Johns leave application";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));
        }

        public void PendingLeaveApplicationByLM(Employee employee, Employee employeeLM, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Your leave application has been submitted and now it is pending by LM";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "John has submitted a new leave application";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));
        }

        public void PendingLeaveApplicationByHR(Employee employee, Employee employeeLM, Employee employeeHR, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Your leave application has been approved by LM and now it is pending by HR";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "You have approved Johns leave application, and now it is pending by HR";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));

            // message to the human resources
            message = "John has submitted a new leave application";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));
        }

        public void RejectedLeaveApplicationByLM(Employee employee, Employee employeeLM, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Your leave application has been rejected by LM";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "You have rejected Johns leave application";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));
        }

        public void RejectedLeaveApplicationByHR(Employee employee, Employee employeeHR, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Your leave application has been rejected by HR";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the human resources
            message = "You have rejected Johns leave application";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));
        }

        public void CancelledLeaveApplicationByStaff(Employee employee, Leave leave)
        {
            string message;

            // message to the applicant
            message = "You have successfully cancelled your leave application";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));
        }

        public void CancelledLeaveApplicationByLM(Employee employee, Employee employeeLM, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Your leave application has been cancelled by LM";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "You have cancelled Johns leave application";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));
        }

        public void CancelledLeaveApplicationByHR(Employee employee, Employee employeeHR, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Your leave application has been cancelled by HR";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the human resources
            message = "You have cancelled Johns leave application";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));
        }

        public void SendMail(string email, string message)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("leave@transnatedu.com", "TAG Leave System");
            mail.To.Add(new MailAddress(email));
            mail.Subject = "Leave Application Update";
            mail.Body = message + Environment.NewLine;

            SmtpClient client = new SmtpClient();
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("leave@transnatedu.com", "TagHr@007");

            try
            {
                client.Send(mail);
                Debug.WriteLine("Mail Sent");
            }
            catch (Exception e)
            {
                Debug.WriteLine("Mail NOT sent" + e.ToString());
            }
        }
    }
}