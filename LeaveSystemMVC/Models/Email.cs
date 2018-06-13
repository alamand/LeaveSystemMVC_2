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
            message = "Dear " + employee.firstName + " " + employee.lastName;
            message += "\n\nYour " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been fully approved.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "Dear " + employeeLM.firstName + " " + employeeLM.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been fully approved.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));

            // message to the human resources
            message = "Dear " + employeeHR.firstName + " " + employeeHR.lastName;
            message += "\n\nYou have approved " + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + ".";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));
        }

        public void PendingLeaveApplicationByLM(Employee employee, Employee employeeLM, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Dear " + employee.firstName + " " + employee.lastName;
            message += "\n\nYour " + leave.leaveTypeDisplayName + " leave application has been submitted and is now pending approval by your line manager.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "Dear " + employeeLM.firstName + " " + employeeLM.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application is now pending your approval.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));
        }

        public void PendingLeaveApplicationByHR(Employee employee, Employee employeeLM, Employee employeeHR, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Dear " + employee.firstName + " " + employee.lastName;
            message += "\n\nYour " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been approved by your line manager and is now pending approval by human resources.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "Dear " + employeeLM.firstName + " " + employeeLM.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " is now pending approval with human resources.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));

            // message to the human resources
            message = "Dear " + employeeHR.firstName + " " + employeeHR.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " is now pending your approval.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));
        }

        public void RejectedLeaveApplicationByLM(Employee employee, Employee employeeLM, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Dear " + employee.firstName + " " + employee.lastName;
            message += "\n\nYour " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been rejected by your line manager.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "Dear " + employeeLM.firstName + " " + employeeLM.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been rejected successfully.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));
        }

        public void RejectedLeaveApplicationByHR(Employee employee, Employee employeeLM, Employee employeeHR, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Dear " + employee.firstName + " " + employee.lastName;
            message += "\n\nYour " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been rejected by human resources.";
            message += "\n\nKind regards \nTAG HR team";            
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "Dear " + employeeLM.firstName + " " + employeeLM.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been rejected by human resources.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));

            // message to the human resources
            message = "Dear " + employeeHR.firstName + " " + employeeHR.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been rejected successfully.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeHR.email, message));
        }

        public void CancelledLeaveApplicationByStaff(Employee employee, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Dear " + employee.firstName + " " + employee.lastName;
            message += "\n\nYour " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been cancelled successfully.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));
        }

        public void CancelledLeaveApplicationByLM(Employee employee, Employee employeeLM, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Dear " + employee.firstName + " " + employee.lastName;
            message += "\n\nYour " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been cancelled by your line manager.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the line manager
            message = "Dear " + employeeLM.firstName + " " + employeeLM.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been cancelled successfully.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employeeLM.email, message));
        }

        public void CancelledLeaveApplicationByHR(Employee employee, Employee employeeHR, Leave leave)
        {
            string message;

            // message to the applicant
            message = "Dear " + employee.firstName + " " + employee.lastName;
            message += "\n\nYour " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been cancelled by human resources.";
            message += "\n\nKind regards \nTAG HR team";
            BackgroundJob.Enqueue(() => SendMail(employee.email, message));

            // message to the human resources
            message = "Dear " + employeeHR.firstName + " " + employeeHR.lastName;
            message += "\n\n" + employee.firstName + " " + employee.lastName + "\'s " + leave.leaveTypeDisplayName + " leave application with ID " + leave.leaveAppID + " has been cancelled successfully.";
            message += "\n\nKind regards \nTAG HR team";
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