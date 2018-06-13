using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrPendingApplicationController : BaseController
    {
        // GET: hrPendingApplication
        [HttpGet]
        public ActionResult Index()
        {
            List<Leave> retrievedApplications = new List<Leave>();

            // extract all pending applications
            foreach (var leave in GetLeaveModel())
            {
                if (leave.leaveStatusName.Equals("Pending_HR"))
                    retrievedApplications.Add(leave);
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.WarningMessage = TempData["WarningMessage"];

            return View(retrievedApplications);
        }

        [HttpGet]
        public ActionResult Select(int appID)
        {
            Dictionary dic = new Dictionary();

            Leave passingLeave = GetLeaveModel().First(leave => leave.leaveAppID == appID);
            var employee = GetEmployeeModel(passingLeave.employeeID);

            ViewData["Preview"] = AccumulationPreview(passingLeave);
            ViewData["LeaveHistory"] = GetLeaveHistory(passingLeave.employeeID);
            ViewData["Balances"] = GetLeaveBalanceModel(passingLeave.employeeID);
            ViewData["Gender"] = employee.gender;
            ViewData["Religion"] = dic.GetReligionName()[employee.religionID];
            TempData["Employee"] = employee;

            return View(passingLeave);
        }

        [HttpPost]
        public ActionResult Select(Leave leave, string submit)
        {
            switch (submit)
            {
                case "Approve":
                    Approve(leave);
                    break;

                case "Reject":
                    Reject(leave);
                    break;

                default:
                    break;
            }

            if (TempData["SuccessMessage"] != null || TempData["WarningMessage"] != null)
                return RedirectToAction("Index");
            else
                return Select(leave.leaveAppID);
        }

        private Dictionary<string, decimal> AccumulationPreview(Leave leave)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();
            Balance leaveBalance = GetLeaveBalanceModel(leave.employeeID);

            // these leave types use half days
            if (leave.leaveTypeName.Equals("Annual") || 
                leave.leaveTypeName.Equals("Sick") || 
                leave.leaveTypeName.Equals("Compassionate") ||
                leave.leaveTypeName.Equals("Unpaid") || 
                leave.leaveTypeName.Equals("DIL"))
                AdjustHalfDays(leave);

            // gets the total number of days, this involves excluding weekends and public holidays
            decimal numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            switch (leave.leaveTypeName)
            {
                case "Annual":
                    balanceDeduction = LeaveAppAnnual(leaveBalance, numOfDays);
                    break;

                case "Sick":
                    balanceDeduction = LeaveAppSick(leaveBalance, numOfDays);
                    break;

                case "Maternity":
                    TimeSpan diff = leave.returnDate - leave.startDate;
                    int numOfPublicHolidays = GetNumOfPublicHolidays(leave.startDate, leave.returnDate);
                    // Maternity leave includes weekends but excludes public holidays
                    numOfDays = diff.Days - numOfPublicHolidays;
                    balanceDeduction = LeaveAppMaternity(leaveBalance, numOfDays);
                    break;

                case "Compassionate":
                    balanceDeduction = LeaveAppCompassionate(leaveBalance, numOfDays);
                    break;

                case "Short_Hours":
                    TimeSpan span = (TimeSpan)leave.shortEndTime - (TimeSpan)leave.shortStartTime;
                    balanceDeduction.Add("Short_Hours", (decimal)span.TotalHours);
                    break;

                case "Pilgrimage":
                    balanceDeduction.Add("Pilgrimage", (decimal)leaveBalance.pilgrimage);
                    break;

                case "Unpaid":
                    balanceDeduction.Add("Unpaid", numOfDays);
                    break;

                case "DIL":
                    balanceDeduction.Add("DIL", numOfDays);
                    break;

                default:
                    break; ;
            }

            return balanceDeduction;
        }

        private void AdjustHalfDays(Leave model)
        {
            //half a day of leave
            if (model.startDate.Equals(model.returnDate) && (model.isStartDateHalfDay == true || model.isReturnDateHalfDay == true))
                model.returnDate = model.returnDate.AddDays(0.5);
            else
            {
                if (model.isStartDateHalfDay == true && !IsPublicHoliday(model.startDate)) // leave starts at 12pm on startDate
                    model.startDate = model.startDate.AddDays(0.5);

                if (model.isReturnDateHalfDay == true && !IsPublicHoliday(model.returnDate)) // leave ends at 12pm on returnDate
                    model.returnDate = model.returnDate.AddDays(0.5);
            }
        }

        private Dictionary<string, decimal> LeaveAppAnnual(Balance lb, decimal numOfDays)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();

            // keeps track of how much credit points should be deducted from each balance type
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in DIL, if yes, then simply deduct from DIL, 
            // if not, deduct all the balance from DIL and the remainder from annual balance. if annual balance 
            // is insufficient, then add the remaining number of days to unpaid balance.
            if (lb.daysInLieu < numOfDays)
            {
                deductDIL = lb.daysInLieu;
                if (lb.daysInLieu + lb.annual < numOfDays)
                {
                    deductAnnual = lb.annual;
                    addUnpaid = numOfDays - deductDIL - deductAnnual;
                }
                else
                {
                    deductAnnual = numOfDays - deductDIL;
                }
            }
            else
            {
                deductDIL = numOfDays;
            }

            // add what will be deducted in the dictionary
            if (deductDIL > 0)
                balanceDeduction.Add("DIL", deductDIL);

            if (deductAnnual > 0)
                balanceDeduction.Add("Annual", deductAnnual);

            if (addUnpaid > 0)
                balanceDeduction.Add("Unpaid", addUnpaid);

            return balanceDeduction;
        }

        private Dictionary<string, decimal> LeaveAppSick(Balance lb, decimal numOfDays)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();

            // keeps track of how much credit points should be deducted from each balance type
            decimal deductDIL = 0;
            decimal deductSick = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Sick --> DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in sick, if yes, then simply deduct from sick, 
            // if not, deduct all the balance from sick and the remainder from DIL balance. if DIL balance 
            // is insufficient, deduct all the balance from DIL and the remainder from annual balance. 
            // if annual balance is insufficient, deduct all from annual and then add the remaining number 
            // of days to unpaid balance.
            if (lb.sick < numOfDays)
            {
                deductSick = lb.sick;
                if (lb.sick + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (lb.sick + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - deductSick - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - deductSick - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - deductSick;
                }
            }
            else
            {
                deductSick = numOfDays;
            }

            // add what will be deducted in the dictionary
            if (deductSick > 0)
                balanceDeduction.Add("Sick", deductSick);

            if (deductDIL > 0)
                balanceDeduction.Add("DIL", deductDIL);

            if (deductAnnual > 0)
                balanceDeduction.Add("Annual", deductAnnual);

            if (addUnpaid > 0)
                balanceDeduction.Add("Unpaid", addUnpaid);

            return balanceDeduction;
        }

        private Dictionary<string, decimal> LeaveAppMaternity(Balance lb, decimal numOfDays)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();

            // keeps track of how much credit points should be deducted from each balance type
            decimal deductMaternity = 0;
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Maternity --> DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in maternity, if yes, then simply deduct from maternity, 
            // if not, deduct all the balance from maternity and the remainder from DIL balance. if DIL balance 
            // is insufficient, deduct all the balance from DIL and the renmainder from annual, finnaly if annual 
            // is insufficient, deduct all from annual and add the remaining number of days to unpaid balance.
            if (lb.maternity < numOfDays)
            {
                deductMaternity = lb.maternity;
                if (lb.maternity + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (lb.maternity + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - deductMaternity - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - deductMaternity - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - deductMaternity;
                }
            }
            else
            {
                deductMaternity = numOfDays;
            }

            // add what will be deducted in the dictionary
            if (deductMaternity > 0)
                balanceDeduction.Add("Maternity", deductMaternity);

            if (deductDIL > 0)
                balanceDeduction.Add("DIL", deductDIL);

            if (deductAnnual > 0)
                balanceDeduction.Add("Annual", deductAnnual);

            if (addUnpaid > 0)
                balanceDeduction.Add("Unpaid", addUnpaid);

            return balanceDeduction;
        }

        private Dictionary<string, decimal> LeaveAppCompassionate(Balance lb, decimal numOfDays)
        {
            Dictionary<string, decimal> balanceDeduction = new Dictionary<string, decimal>();
            decimal maxDIL = GetLeaveBalanceModel().compassionate;

            // keeps track of how much credit points should be deducted from each balance type
            decimal addCompassionate = 0;
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Compassionate --> DIL --> Annual --> Unpaid
            // checks if the applicant has not exceeded the compassionate limit, if yes, then simply add to compassionate, 
            // if not, deduct all the balance from DIL and the remainder from Annual balance. if Annual balance 
            // is insufficient, deduct all the balance from DIL and the remainder from annual balance. 
            // if annual balance is insufficient, deduct all from annual and then add the remaining number 
            // of days to unpaid balance.
            if (maxDIL < numOfDays)
            {
                addCompassionate = maxDIL;
                if (maxDIL + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (maxDIL + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - maxDIL - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - maxDIL - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - maxDIL;
                }
            }
            else
            {
                addCompassionate = numOfDays;
            }

            // add what will be deducted in the dictionary
            if (addCompassionate > 0)
                balanceDeduction.Add("Compassionate", addCompassionate);

            if (deductDIL > 0)
                balanceDeduction.Add("DIL", deductDIL);

            if (deductAnnual > 0)
                balanceDeduction.Add("Annual", deductAnnual);

            if (addUnpaid > 0)
                balanceDeduction.Add("Unpaid", addUnpaid);

            return balanceDeduction;
        }

        private decimal GetNumOfDays(DateTime sDate, DateTime eDate)
        {
            // @TODO: Test for all cases
            TimeSpan diff = eDate - sDate;
            decimal numOfDays = diff.Days + (diff.Hours / (decimal)24.0); //number of days and hours excluding public holidays and weekends
            decimal fullNumOfDays = numOfDays; // number of days including public holidays and weekends

            // exclude weekend
            // go through each day from start date up to number of days
            for (var i = 0; i < fullNumOfDays; i++)
            {
                switch (sDate.AddDays(i).DayOfWeek)
                {
                    case DayOfWeek.Saturday:
                        numOfDays--;            
                        break;

                    case DayOfWeek.Friday:
                        numOfDays--;
                        break;
                }
            }

            // exclude public holidays
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@sDate", SqlDbType.DateTime).Value = sDate.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@eDate", SqlDbType.DateTime).Value = eDate.ToString("yyyy-MM-dd");
            cmd.CommandText = "SELECT * FROM dbo.Public_Holiday WHERE Date BETWEEN @sDate AND @eDate";

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                DateTime day = (DateTime)row["Date"];
                for (var i = 0; i < fullNumOfDays; i++)
                {
                    if (((sDate.AddDays(i)).Date).Equals(day.Date))
                        numOfDays--;
                }
            }

            return numOfDays;
        }

        private int GetNumOfPublicHolidays(DateTime sDate, DateTime eDate)
        {
            TimeSpan diff = eDate - sDate;
            int numOfDays = diff.Days;
            int fullNumOfDays = numOfDays;
            int numOfPublicHolidays = 0;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@sDate", SqlDbType.DateTime).Value = sDate.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@eDate", SqlDbType.DateTime).Value = eDate.ToString("yyyy-MM-dd");
            cmd.CommandText = "SELECT * FROM dbo.Public_Holiday WHERE Date BETWEEN @sDate AND @eDate";

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                DateTime day = (DateTime)row["Date"];
                for (var i = 0; i < fullNumOfDays; i++)
                {
                    if (sDate.AddDays(i).Equals(day))
                        numOfPublicHolidays++;
                }
            }
            
            return numOfPublicHolidays;
        }

        private void Reject(Leave leave)
        {
            Dictionary dic = new Dictionary();
            int rejectedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Rejected_HR").Key;

            UpdateLeaveApplication(leave, rejectedID);

            Employee emp = GetEmployeeModel(leave.employeeID);
            Employee empHR = GetEmployeeModel(GetLoggedInID());
            Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);

            Email email = new Email();
            email.RejectedLeaveApplicationByHR(emp, empLM, empHR, leave);

            // sets the notification message to be displayed
            TempData["WarningMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>rejected</b> successfully.<br/>";
        }

        private void Approve(Leave leave)
        {
            switch (leave.leaveTypeName)
            {
                case "Annual":
                    ApproveAnnualLeave(leave);
                    break;

                case "Sick":
                    ApproveSickLeave(leave);
                    break;

                case "Maternity":
                    ApproveMaternityLeave(leave);
                    break;

                case "Compassionate":
                    ApproveCompassionate(leave);
                    break;

                case "Short_Hours":
                    ApproveShortHours(leave);
                    break;

                case "Pilgrimage":
                    ApprovePilgrimage(leave);
                    break;

                case "Unpaid":
                    ApproveUnpaid(leave);
                    break;

                case "DIL":
                    ApproveDIL(leave);
                    break;

                default:
                    break;
            }
        }

        private void ApproveAnnualLeave(Leave leave)
        {
            Balance lb = GetLeaveBalanceModel(leave.employeeID);

            AdjustHalfDays(leave);

            // gets the total number of days, this involves excluding weekends and public holidays
            decimal numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            // keeps track of how much credit points should be deducted from each balance type
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in DIL, if yes, then simply deduct from DIL, 
            // if not, deduct all the balance from DIL and the remainder from annual balance. if annual balance 
            // is insufficient, then add the remaining number of days to unpaid balance.
            if (lb.daysInLieu < numOfDays)
            {
                deductDIL = lb.daysInLieu;
                if (lb.daysInLieu + lb.annual < numOfDays)
                {
                    deductAnnual = lb.annual;
                    addUnpaid = numOfDays - deductDIL - deductAnnual;
                }
                else
                {
                    deductAnnual = numOfDays - deductDIL;
                }
            }
            else
            {
                deductDIL = numOfDays;
            }

            Dictionary dic = new Dictionary();
            int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

            UpdateLeaveApplication(leave, approvedID);

            string comment = "Approved Leave Application";

            if (deductDIL > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.daysInLieuID, lb.daysInLieu, lb.daysInLieu - deductDIL, comment);

            if (deductAnnual > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.annualID, lb.annual, lb.annual - deductAnnual, comment);

            if (addUnpaid > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.unpaidID, lb.unpaid, lb.unpaid + addUnpaid, comment);

            Employee emp = GetEmployeeModel(leave.employeeID);
            Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
            Employee empHR = GetEmployeeModel(GetLoggedInID());
            new Email().ApprovedLeaveApplication(emp, empLM, empHR, leave);

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
        }

        private void ApproveSickLeave(Leave leave)
        {
            Balance lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            decimal numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            // keeps track of how much credit points should be deducted from each balance type
            decimal deductDIL = 0;
            decimal deductSick = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Sick --> DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in sick, if yes, then simply deduct from sick, 
            // if not, deduct all the balance from sick and the remainder from DIL balance. if DIL balance 
            // is insufficient, deduct all the balance from DIL and the remainder from annual balance. 
            // if annual balance is insufficient, deduct all from annual and then add the remaining number 
            // of days to unpaid balance.
            if (lb.sick < numOfDays)
            {
                deductSick = lb.sick;
                if (lb.sick + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (lb.sick + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - deductSick - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - deductSick - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - deductSick;
                }
            }
            else
            {
                deductSick = numOfDays;
            }

            Dictionary dic = new Dictionary();
            int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

            UpdateLeaveApplication(leave, approvedID);

            string comment = "Approved Leave Application";

            if (deductSick > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.sickID, lb.sick, lb.sick - deductSick, comment);
            
            if (deductDIL > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.daysInLieuID, lb.daysInLieu, lb.daysInLieu - deductDIL, comment);

            if (deductAnnual > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.annualID, lb.annual, lb.annual - deductAnnual, comment);

            if (addUnpaid > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.unpaidID, lb.unpaid, lb.unpaid + addUnpaid, comment);

            Employee emp = GetEmployeeModel(leave.employeeID);
            Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
            Employee empHR = GetEmployeeModel(GetLoggedInID());
            new Email().ApprovedLeaveApplication(emp, empLM, empHR, leave);

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
        }

        private void ApproveMaternityLeave(Leave leave)
        {
            Balance lb = GetLeaveBalanceModel(leave.employeeID);

            TimeSpan diff = leave.returnDate - leave.startDate;

            int numOfPublicHolidays = GetNumOfPublicHolidays(leave.startDate, leave.returnDate);

            // Maternity leave includes weekends but excludes public holidays
            int numOfDays = diff.Days - numOfPublicHolidays;

            // keeps track of how much credit points should be deducted from each balance type
            decimal deductMaternity = 0;
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Maternity --> DIL --> Annual --> Unpaid
            // checks if the applicant has enough balance in maternity, if yes, then simply deduct from maternity, 
            // if not, deduct all the balance from maternity and the remainder from DIL balance. if DIL balance 
            // is insufficient, deduct all the balance from DIL and the renmainder from annual, finnaly if annual 
            // is insufficient, deduct all from annual and add the remaining number of days to unpaid balance.
            if (lb.maternity < numOfDays)
            {
                deductMaternity = lb.maternity;
                if (lb.maternity + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (lb.maternity + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - deductMaternity - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - deductMaternity - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - deductMaternity;
                }
            }
            else
            {
                deductMaternity = numOfDays;
            }

            Dictionary dic = new Dictionary();
            int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

            UpdateLeaveApplication(leave, approvedID);

            string comment = "Approved Leave Application";

            if (deductMaternity > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.maternityID, lb.maternity, lb.maternity - deductMaternity, comment);

            if (deductDIL > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.daysInLieuID, lb.daysInLieu, lb.daysInLieu - deductDIL, comment);

            if (deductAnnual > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.annualID, lb.annual, lb.annual - deductAnnual, comment);

            if (addUnpaid > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.unpaidID, lb.unpaid, lb.unpaid + addUnpaid, comment);

            Employee emp = GetEmployeeModel(leave.employeeID);
            Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
            Employee empHR = GetEmployeeModel(GetLoggedInID());
            new Email().ApprovedLeaveApplication(emp, empLM, empHR, leave);

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";

        }

        private void ApproveCompassionate(Leave leave)
        {
            Balance lb = GetLeaveBalanceModel(leave.employeeID);
            decimal maxDIL = GetLeaveBalanceModel().compassionate;

            // gets the total number of days, this involves excluding weekends and public holidays
            decimal numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            // keeps track of how much credit points should be deducted from each balance type
            decimal addCompassionate = 0;
            decimal deductDIL = 0;
            decimal deductAnnual = 0;
            decimal addUnpaid = 0;

            // deduction order: Compassionate --> DIL --> Annual --> Unpaid
            // checks if the applicant has not exceeded the compassionate limit, if yes, then simply add to compassionate, 
            // if not, deduct all the balance from DIL and the remainder from Annual balance. if Annual balance 
            // is insufficient, deduct all the balance from DIL and the remainder from annual balance. 
            // if annual balance is insufficient, deduct all from annual and then add the remaining number 
            // of days to unpaid balance.
            if (maxDIL < numOfDays)
            {
                addCompassionate = maxDIL;
                if (maxDIL + lb.daysInLieu < numOfDays)
                {
                    deductDIL = lb.daysInLieu;
                    if (maxDIL + lb.daysInLieu + lb.annual < numOfDays)
                    {
                        deductAnnual = lb.annual;
                        addUnpaid = numOfDays - maxDIL - deductDIL - deductAnnual;
                    }
                    else
                    {
                        deductAnnual = numOfDays - maxDIL - deductDIL;
                    }
                }
                else
                {
                    deductDIL = numOfDays - maxDIL;
                }
            }
            else
            {
                addCompassionate = numOfDays;
            }

            Dictionary dic = new Dictionary();
            int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

            UpdateLeaveApplication(leave, approvedID);

            string comment = "Approved Leave Application";

            if (addCompassionate > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.compassionateID, lb.compassionate, lb.compassionate + addCompassionate, comment);

            if (deductDIL > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.daysInLieuID, lb.daysInLieu, lb.daysInLieu - deductDIL, comment);

            if (deductAnnual > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.annualID, lb.annual, lb.annual - deductAnnual, comment);

            if (addUnpaid > 0)
                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.unpaidID, lb.unpaid, lb.unpaid + addUnpaid, comment);

            Employee emp = GetEmployeeModel(leave.employeeID);
            Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
            Employee empHR = GetEmployeeModel(GetLoggedInID());
            new Email().ApprovedLeaveApplication(emp, empLM, empHR, leave);

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";

        }

        private void ApproveShortHours(Leave leave)
        {
            Balance lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of hours
            TimeSpan span = leave.shortEndTime - leave.shortStartTime;

            // does the user have enough balance?
            if (lb.shortHours >= (decimal)span.TotalHours)
            {
                Dictionary dic = new Dictionary();
                int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

                UpdateLeaveApplication(leave, approvedID);

                string comment = "Approved Leave Application";

                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.shortHoursID, lb.shortHours, lb.shortHours - (decimal)span.TotalHours, comment);

                Employee emp = GetEmployeeModel(leave.employeeID);
                Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                Employee empHR = GetEmployeeModel(GetLoggedInID());
                new Email().ApprovedLeaveApplication(emp, empLM, empHR, leave);

                // sets the notification message to be displayed
                TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "Leave application ID <b>" + leave.leaveAppID + "</b> can't be approved, <b>" + leave.employeeName + "</b> does not have enough balance.";
            }
        }

        private void ApproveDIL(Leave leave)
        {
            Balance lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            decimal numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            // does the user have enough balance?
            if (lb.daysInLieu >= numOfDays)
            {
                Dictionary dic = new Dictionary();
                int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

                UpdateLeaveApplication(leave, approvedID);

                string comment = "Approved Leave Application";

                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.daysInLieuID, lb.daysInLieu, lb.daysInLieu - numOfDays, comment);

                Employee emp = GetEmployeeModel(leave.employeeID);
                Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                Employee empHR = GetEmployeeModel(GetLoggedInID());

                Email email = new Email();
                email.ApprovedLeaveApplication(emp, empLM, empHR, leave);

                // sets the notification message to be displayed
                TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "Leave application ID <b>" + leave.leaveAppID + "</b> can't be approved, <b>" + leave.employeeName + "</b> does not have enough balance.";
            }
        }

        private void ApprovePilgrimage(Leave leave)
        {
            Balance lb = GetLeaveBalanceModel(leave.employeeID);

            // gets the total number of days, this involves excluding weekends and public holidays
            decimal numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            // does the user have enough balance?
            if (lb.pilgrimage >= numOfDays)
            {
                Dictionary dic = new Dictionary();
                int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

                UpdateLeaveApplication(leave, approvedID);

                string comment = "Approved Leave Application";

                UpdateBalance(leave.employeeID, leave.leaveAppID, lb.pilgrimageID, lb.pilgrimage, lb.pilgrimage - lb.pilgrimage, comment);

                Employee emp = GetEmployeeModel(leave.employeeID);
                Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
                Employee empHR = GetEmployeeModel(GetLoggedInID());
                new Email().ApprovedLeaveApplication(emp, empLM, empHR, leave);

                // sets the notification message to be displayed
                TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
            }
            else
            {
                ViewBag.ErrorMessage = "Leave application ID <b>" + leave.leaveAppID + "</b> can't be approved, <b>" + leave.employeeName + "</b> does not have enough balance.";
            }
        }

        private void ApproveUnpaid(Leave leave)
        {
            Balance lb = GetLeaveBalanceModel(leave.employeeID);

            AdjustHalfDays(leave);

            // gets the total number of days, this involves excluding weekends and public holidays
            decimal numOfDays = GetNumOfDays(leave.startDate, leave.returnDate);

            Dictionary dic = new Dictionary();
            int approvedID = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value == "Approved").Key;

            UpdateLeaveApplication(leave, approvedID);

            string comment = "Approved Leave Application";

            UpdateBalance(leave.employeeID, leave.leaveAppID, lb.unpaidID, lb.unpaid, lb.unpaid + numOfDays, comment);

            Employee emp = GetEmployeeModel(leave.employeeID);
            Employee empLM = GetEmployeeModel((int)emp.reportsToLineManagerID);
            Employee empHR = GetEmployeeModel(GetLoggedInID());
            new Email().ApprovedLeaveApplication(emp, empLM, empHR, leave);

            // sets the notification message to be displayed
            TempData["SuccessMessage"] = "Leave application ID <b>" + leave.leaveAppID + "</b> for <b>" + leave.employeeName + "</b> has been <b>approved</b> successfully.<br/>";
        }

        private void UpdateLeaveApplication(Leave leave, int approvalID)
        {
            int prevStatus = leave.leaveStatusID;
            DataBase db = new DataBase();

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@approvalID", SqlDbType.Int).Value = approvalID;
            cmd.Parameters.Add("@hrComment", SqlDbType.NChar).Value = leave.hrComment ?? "";
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = leave.leaveAppID;
            cmd.CommandText = "UPDATE dbo.Leave SET Leave_Status_ID = @approvalID, HR_Comment = @hrComment WHERE Leave_Application_ID = @appID";
            db.Execute(cmd);

            cmd.Parameters.Clear();
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = leave.leaveAppID;
            cmd.Parameters.Add("@prevStatus", SqlDbType.Int).Value = prevStatus;
            cmd.Parameters.Add("@approvalID", SqlDbType.Int).Value = approvalID;
            cmd.Parameters.Add("@loggedInID", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@today", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Application (Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On) " +
                  "VALUES(@appID, 'Leave_Status_ID', @prevStatus, @approvalID, @loggedInID, @today)";
            db.Execute(cmd);
        }

        private void UpdateBalance(int empID, int appID, int leaveID, decimal valBefore, decimal valAfter, string comment)
        {
            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
            cmd.Parameters.Add("@leaveID", SqlDbType.Int).Value = leaveID;
            cmd.Parameters.Add("@valBefore", SqlDbType.Decimal).Value = valBefore;
            cmd.Parameters.Add("@valAfter", SqlDbType.Decimal).Value = valAfter;
            cmd.Parameters.Add("@loggedInID", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@today", SqlDbType.NChar).Value = DateTime.Today.ToString("yyyy-MM-dd");
            cmd.Parameters.Add("@comment", SqlDbType.NChar).Value = comment ?? "";

            if (IsLeaveBalanceExist(empID, leaveID))
            {
                cmd.CommandText = "UPDATE dbo.Leave_Balance SET Balance = @valAfter WHERE Employee_ID = @empID AND Leave_Type_ID = @leaveID";
                db.Execute(cmd);

                cmd.Parameters.Add("@balanceID", SqlDbType.NChar).Value = GetLeaveBalanceID(empID, leaveID);
                cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On, Comment) " +
                   "VALUES(@balanceID, @appID, 'Balance', @valBefore, @valAfter, @loggedInID, @today, @comment)";
                db.Execute(cmd);
            }
            else
            {
                cmd.CommandText = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_Type_ID, Balance) VALUES(@empID, @leaveID, @valAfter)";
                db.Execute(cmd);

                cmd.Parameters.Add("@balanceID", SqlDbType.NChar).Value = GetLeaveBalanceID(empID, leaveID);
                cmd.CommandText = "INSERT INTO dbo.Audit_Leave_Balance (Leave_Balance_ID, Leave_Application_ID, Column_Name, Value_After, Created_By, Created_On, Comment) " +
                   "VALUES(@balanceID, @appID, 'Balance', @valAfter, @loggedInID, @today, @comment)";
                db.Execute(cmd);
            }
        }

        private List<Leave> GetLeaveHistory(int empID)
        {
            var leaveHistory = new List<Leave>();
            var leaves = GetLeaveModel("Employee.Employee_ID", empID);

            foreach (var leave in leaves)
            {
                if (!leave.leaveStatusName.Equals("Pending_LM") && !leave.leaveStatusName.Equals("Pending_HR"))
                    leaveHistory.Add(leave);
            }

            return leaveHistory;
        }

        private int GetLeaveBalanceID(int empID, int typeID)
        {
            int leaveBalanceID = 0;
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
            cmd.CommandText = "SELECT Leave_Balance_ID FROM dbo.Leave_Balance WHERE Employee_ID = @empID AND Leave_Type_ID = @typeID";
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                leaveBalanceID = (int)row["Leave_Balance_ID"];
            }

            return leaveBalanceID;
        }
    }
}