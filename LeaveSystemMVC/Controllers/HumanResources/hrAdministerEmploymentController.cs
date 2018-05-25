using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Agent;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrAdministerEmploymentController : ControllerBase
    {
        // GET: hrAdministerEmployment
        public ActionResult Index(int selectedEmployee = 0)
        {
            SetMessageViewBags();

            // gets and stores to the ViewData all available Employees from the DB and adds a default key of 0 for de-selecting 
            ViewData["EmployeeList"] = AddDefaultToDictionary(DBStaffList(), 0, "- Select Employee -");

            // this sets the default selection, or the user selection from the dropdown
            ViewData["selectedEmployee"] = selectedEmployee;

            // did the user select an employee from the dropdown list?
            if (selectedEmployee != 0)
            {
                // create a list of employment periods (note that this is with the same employee/staff ID)
                List<sEmployeeModel> employmentList = GetEmploymentPeriod(selectedEmployee);

                // does these employment periods ends with an End Date, in other words, does the last employment period has an End Date?
                // if yes, then create a new empty employment period for the user to fill in the Start Date
                // else, the user will have to fill in the End Date for the latest Start Date
                if (IsEndDateExist(employmentList))
                {
                    sEmployeeModel emp = new sEmployeeModel();
                    emp.staffID = selectedEmployee;
                    employmentList.Add(emp);
                }

                // note that the user is allowed to enter only one date at a time, either a start date or end date.
                // this will be detrmined in the view code.
                // if the latest employment period has a start date and not an end date, then the user will be able to enter an end date for that employment period.
                // else if the latest employment period already has both start and end date, then the user will be able to enter a new start date.
                return View(employmentList);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(List<sEmployeeModel> employmentList)
        {
            string queryString;

            // gets the latest employment period.
            sEmployeeModel latestEmployment = employmentList[employmentList.Count - 1];

            // checks if the start date for this employment period already exist in DB.
            // true if the DB has the Start Date and not the End Date
            // false if the DB does not have any record for this staff ID with this Start Date
            Boolean startDateExist = IsStartDateExist((int)latestEmployment.staffID, latestEmployment.empStartDate);

            // if the DB does not have a record for this Start Date, and the Start Date is entered by the user.
            if (!startDateExist && !latestEmployment.empStartDate.ToShortDateString().Equals("01/01/0001"))
            {
                // is the entered start date at least one day after the previous end date?
                if (latestEmployment.empStartDate > employmentList[employmentList.Count-2].empEndDate)
                {
                    queryString = "INSERT INTO dbo.Employment_Period (Employee_ID, Emp_Start_Date) VALUES ('" + latestEmployment.staffID + "', '" + latestEmployment.empStartDate.ToString("yyyy-MM-dd") + "')";
                    DBExecuteQuery(queryString);
                    ChangeAccountStatus((int)latestEmployment.staffID, true, latestEmployment.empStartDate);
                    TempData["SuccessMessage"] = "You have successfully updated the employment period for <b>" + DBEmployeeList()[(int)latestEmployment.staffID] + "</b>.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to set an employment period. The selected Start Date must be <b>after</b> the previous End Date. <br/>" +
                        "The previous <b>End Date</b> was on <b>" + employmentList[employmentList.Count - 2].empEndDate.ToShortDateString() + "</b>, where the selected <b>Start Date</b> is on <b>" + latestEmployment.empStartDate.ToShortDateString() + "</b>.";
                }

                return RedirectToAction("Index", new { selectedEmployee = (int)latestEmployment.staffID });
            }
            // if the DB does have a record for this End Date, and the EnD Date is entered by the user.
            else if (startDateExist && !latestEmployment.empEndDate.ToShortDateString().Equals("01/01/0001"))
            {
                // is the entered end date at least one day after the start date?
                if (latestEmployment.empEndDate > latestEmployment.empStartDate)
                {
                    queryString = "UPDATE dbo.Employment_Period SET Emp_End_Date = '" + latestEmployment.empEndDate.ToString("yyyy-MM-dd") + "'" +
                    " WHERE Employee_ID = '" + latestEmployment.staffID + "' AND Emp_Start_Date = '" + latestEmployment.empStartDate.ToString("yyyy-MM-dd") + "'";
                    DBExecuteQuery(queryString);
                    ChangeAccountStatus((int)latestEmployment.staffID, false, latestEmployment.empEndDate);
                    TempData["SuccessMessage"] = "You have successfully updated employment period for <b>" + DBEmployeeList()[(int)latestEmployment.staffID] + "</b>.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to set an employment period. The selected End Date must be <b>after</b> the Start Date. <br/>" +
                        "The <b>Start Date</b> was on <b>" + latestEmployment.empStartDate.ToShortDateString() + "</b>, where the selected <b>End Date</b> is on <b>" + latestEmployment.empStartDate.ToShortDateString() + "<b/>.";
                }

                return RedirectToAction("Index", new { selectedEmployee = (int)latestEmployment.staffID });
            }
            else
            {
                // gets and stores to the ViewData all available Employees from the DB and adds a default key of 0 for de-selecting 
                ViewData["EmployeeList"] = AddDefaultToDictionary(DBEmployeeList(), 0, "- Select Employee -");

                // this sets the default selection, or the user selection from the dropdown
                ViewData["selectedEmployee"] = latestEmployment.staffID;
            }

            return View(employmentList);
        }

        [HttpPost]
        public ActionResult Select(FormCollection form)
        {
            // gets the selected employee ID and passes it to Index to display the information
            int id = Convert.ToInt32(form["selectedEmployee"]);
            return RedirectToAction("Index", new { selectedEmployee = id });
        }

        private Boolean IsStartDateExist(int empID, DateTime startDate)
        {
            bool isExist = false;
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var queryString = "SELECT COUNT(*) FROM dbo.Employment_Period WHERE Employee_ID = '" + empID + "' AND Emp_Start_Date = '" + startDate.ToString("yyyy-MM-dd") + "' AND Emp_End_Date IS NULL";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                isExist = ((int)command.ExecuteScalar() > 0) ? true : false;
                connection.Close();
            }
            return isExist;
        }

        private Boolean IsEndDateExist(List<sEmployeeModel> employmentList)
        {
            Boolean isExist = false;
            DateTime defaultDate = new DateTime(0001, 01, 01);

            foreach (var employment in employmentList)
            {
                isExist = false;
                isExist = (employment.empEndDate != defaultDate) ? true : false;
            }

            return isExist;
        }

        private void ChangeAccountStatus(int empID, Boolean status, DateTime date)
        {
            // Get instance of SQL Agent SMO object
            Server server = new Server("."); 
            JobServer jobServer = server.JobServer;

            // Create a schedule, set it to be executed once at the specified date
            JobSchedule schedule = new JobSchedule(jobServer, "Schedule_For_" + empID);
            schedule.FrequencyTypes = FrequencyTypes.OneTime;
            schedule.ActiveStartDate = date;
            schedule.IsEnabled = true;
            schedule.Create();

            // Create Job and assign the schedule to it
            Job job = new Job(jobServer, "Account_Status_" + DateTime.Now);
            job.Create();
            job.AddSharedSchedule(schedule.ID);
            job.ApplyToTargetServer(server.Name);

            // Create Job Step to activate/de-activate an account and delete the Job once done
            JobStep step = new JobStep(job, status + "-" + empID);
            step.Command = "UPDATE dbo.Employee SET Account_Status = '" + status + "' WHERE Employee_ID = '" + empID + "'; " +
                "USE msdb; EXEC sp_delete_job @job_name = N'" + job.Name + "';";
            step.SubSystem = AgentSubSystem.TransactSql;
            step.DatabaseName = "LeaveSystem";
            step.Create();
        }
    }
}