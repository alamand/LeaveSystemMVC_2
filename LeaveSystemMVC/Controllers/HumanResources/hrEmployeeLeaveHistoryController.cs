using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;
using Hangfire;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Diagnostics;
using System.Drawing;
using PdfSharp.Drawing.Layout;

namespace LeaveSystemMVC.Controllers
{
    public class hrEmployeeLeaveHistoryController : ControllerBase
    {
        // GET: hrEmployeeLeaveHistory
        public ActionResult Index(int filterDepartmentID = -1, int filterLeaveType = -1, int filterLeaveStatus = -1, string filterSearch = "", string filterOrderBy = "", string filterStartDate = "", string filterEndDate = "")
        {
            string queryString = GetFilteredQuery(filterDepartmentID, filterLeaveType, filterLeaveStatus, filterSearch, filterOrderBy, filterStartDate, filterEndDate);
            var model = GetLeaveModel(queryString);

            ViewData["EnteredSearch"] = filterSearch;
            ViewData["DepartmentList"] = AddDefaultToDictionary(DBDepartmentList(), -1, "All Departments");
            ViewData["SelectedDepartment"] = filterDepartmentID;
            ViewData["LeaveStatusList"] = LeaveStatusList();
            ViewData["SelectedLeaveStatus"] = filterLeaveStatus;
            ViewData["OrderByList"] = OrderByList();
            ViewData["SelectedOrderBy"] = filterOrderBy;
            ViewData["LeaveTypeList"] = AddDefaultToDictionary(DBLeaveTypeList(), -1, "All Types");
            ViewData["SelectedLeaveType"] = filterLeaveType;
            ViewData["SelectedStartDate"] = filterStartDate;
            ViewData["SelectedEndDate"] = filterEndDate;

            return View(model);
        }

        [HttpGet]
        public ActionResult View(int appID)
        {
            SetMessageViewBags();
            return View(GetLeaveModel("Leave_Application_ID", appID)[0]);
        }

        [HttpGet]
        public ActionResult GeneratePDF(int appID)
        {
            try
            {
                #pragma warning disable CS0618 // Type or member is obsolete

                sLeaveModel leave = GetLeaveModel("Leave_Application_ID", appID)[0];
                sEmployeeModel employee = GetEmployeeModel(leave.employeeID);

                // Create a new PDF document
                PdfDocument document = new PdfDocument();
                document.Info.Title = leave.leaveAppID + " - " + leave.leaveTypeName;
                document.Info.Author = Session["UserName"].ToString();

                // Create an empty page
                PdfPage page = document.AddPage();

                // Set page margins
                page.TrimMargins.Top = 10;
                page.TrimMargins.Right = 10;
                page.TrimMargins.Left = 10;
                page.TrimMargins.Bottom = 10;

                // Get an XGraphics object for drawing
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Get an XTextFormatter object for writting multiple lines
                XTextFormatter tf = new XTextFormatter(gfx);

                // Create a font
                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.WinAnsi);
                XFont fontTitle = new XFont("Andalus", 20, XFontStyle.Bold, options);
                XFont fontHeader = new XFont("Calibri", 14, XFontStyle.Bold, options);
                XFont fontRegular = new XFont("Calibri", 12, XFontStyle.Regular, options);
                XFont fontBold = new XFont("Calibri", 12, XFontStyle.Bold, options);

                // Add a logo to the top left of the page
                XImage image = XImage.FromFile(Server.MapPath("~/App_Data/Images/Murdoch.png"));
                gfx.DrawImage(image, 0, 0, 150, 65);

                // Add a logo to the top right of the page
                image = XImage.FromFile(Server.MapPath("~/App_Data/Images/Global.png"));
                gfx.DrawImage(image, 455, 0, 130, 65);

                // Add a title to the top center of the page
                gfx.DrawString("LEAVE APPLICATION", fontTitle, XBrushes.Black, new XRect(0, 15, page.Width, page.Height), XStringFormat.TopCenter);

                // Draw the two boxes for application details and approvals
                gfx.DrawRoundedRectangle(XPens.Black, new Rectangle(20, 100, (int)page.Width.Value - 35, 430), new Size(3, 20));
                gfx.DrawRoundedRectangle(XPens.Black, new Rectangle(20, 560, (int)page.Width.Value - 35, 270), new Size(3, 20));

                // Set the starting points for each info
                // note that the yAxis adds up after each line (+20pt).
                int xAxisTitle = 40;
                int xAxisValue = 200;
                int yAxis = 90;

                // Draw the header above the first box
                gfx.DrawString("Application Details", fontHeader, XBrushes.Black, new XRect(0, 80, page.Width, page.Height), XStringFormat.TopCenter);

                gfx.DrawString("Leave Application ID", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 30);
                gfx.DrawString(leave.leaveAppID.ToString(), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Leave Type", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.leaveTypeName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Leave Status", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.leaveStatusDisplayName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Application Date", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.applicationDate.ToString("dd/MM/yyyy"), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Employee ID", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.employeeID.ToString(), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Employee Name", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.employeeName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Designation", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(employee.designation, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Department", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(employee.deptID != null ? DBDepartmentList()[(int)employee.deptID] : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Start Date", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.startDate.ToString("dd/MM/yyyy"), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Return Date", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.returnDate.ToString("dd/MM/yyyy"), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Start Time", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString((leave.shortStartTime != new TimeSpan()) ? leave.shortStartTime.ToString() : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("End Time", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString((leave.shortEndTime != new TimeSpan()) ? leave.shortEndTime.ToString() : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Duration", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.leaveDuration.ToString(), fontRegular, XBrushes.Black, xAxisValue, yAxis);
                
                gfx.DrawString("Documentation", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString(leave.documentation, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Contact Details (outside UAE)", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);

                gfx.DrawString("Phone Number", fontBold, XBrushes.Black, xAxisTitle+20, yAxis = yAxis + 20);
                gfx.DrawString(leave.contactDetails, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Personal E-Mail", fontBold, XBrushes.Black, xAxisTitle+20, yAxis = yAxis + 20);
                gfx.DrawString(leave.email, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Comments", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                tf.DrawString(leave.comments, fontRegular, XBrushes.Black, new XRect(xAxisValue, yAxis-10, 350 ,100));


                // Draw the header above the second box
                gfx.DrawString("Approvals", fontHeader, XBrushes.Black, new XRect(0, 540, page.Width, page.Height), XStringFormat.TopCenter);

                // Get the LM's employee id and date of approval for this leave application
                Tuple<int, DateTime> approvalLM = DBGetLeaveApproval(leave.leaveAppID, "LM");
                sEmployeeModel approvalLMEmployee = GetEmployeeModel(approvalLM.Item1);

                gfx.DrawString("Division Head", fontBold, XBrushes.Black, xAxisTitle, yAxis = 580);
                gfx.DrawString(approvalLMEmployee.firstName + " " + approvalLMEmployee.lastName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Date", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString((approvalLM.Item2 != new DateTime()) ? approvalLM.Item2.ToString("dd/MM/yyyy") : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Line Manager Comment", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                tf.DrawString(leave.lmComment, fontRegular, XBrushes.Black, new XRect(xAxisValue, yAxis-10, 350, 100));

                // Get the HR's employee id and date of approval for this leave application
                Tuple<int, DateTime> approvalHR = DBGetLeaveApproval(leave.leaveAppID, "HR");
                sEmployeeModel approvalHREmployee = GetEmployeeModel(approvalHR.Item1);

                gfx.DrawString("Received by HR Dept.", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 100);
                gfx.DrawString(approvalHREmployee.firstName + " " + approvalHREmployee.lastName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Date", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                gfx.DrawString((approvalHR.Item2 != new DateTime()) ? approvalHR.Item2.ToString("dd/MM/yyyy") : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("HR Comment", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                tf.DrawString(leave.hrComment, fontRegular, XBrushes.Black, new XRect(xAxisValue, yAxis-10, 350, 100));

                #pragma warning restore CS0618 // Type or member is obsolete

                // Save the document...
                string xFileName = document.Info.Title + ".pdf";
                string dlFile = Server.MapPath("~/App_Data/PDF_Reports") + "/" + xFileName;
                document.Save(dlFile);
                Process.Start(dlFile);
                return File(dlFile, "application/force-download", xFileName);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "ERROR:" + ex.Message.ToString();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Filter(FormCollection form)
        {
            int deptID = Convert.ToInt32(form["selectedDepartment"]);
            int leaveStatID = Convert.ToInt32(form["selectedLeaveStatus"]);
            int leaveTypeID = Convert.ToInt32(form["selectedLeaveType"]);
            string search = form["enteredSearch"];
            string orderBy = form["selectedOrderBy"];
            string startDate = form["selectedStartDate"];
            string endDate = form["selectedEndDate"];
            return RedirectToAction("Index", new { filterDepartmentID = deptID, filterLeaveType = leaveTypeID, filterLeaveStatus = leaveStatID, filterSearch = search, filterOrderBy = orderBy, filterStartDate = startDate, filterEndDate = endDate });
        }

        private string GetFilteredQuery(int deptID, int leaveType, int leaveStat, string search, string order, string sDate, string eDate)
        {
            var queryString = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, Leave.Reporting_Back_Date, Leave.Leave_Type_ID, Leave_Name, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, Leave_Status.Display_Name, HR_Comment, LM_Comment, Leave.Personal_Email, Leave.Is_Half_Start_Date, Leave.Is_Half_Reporting_Back_Date " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_Type_ID = Leave_Type.Leave_Type_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND Employee.Employee_ID = Reporting.Employee_ID " +
                "AND Leave_Status.Status_Name != 'Pending_LM' AND Leave_Status.Status_Name != 'Pending_HR'";

            // adds a filter query if a department is selected from the dropdown, note that -1 represents All Departments
            if (deptID >= 0)
            {
                queryString += " AND Department.Department_ID = " + deptID;
            }

            // adds a filter query if a leave type is selected from the dropdown, note that -1 represents All Types
            if (leaveType >= 0)
            {
                queryString += " AND Leave.Leave_Type_ID = " + leaveType;
            }

            // adds a filter query if a leave status is selected from the dropdown, note that -1 represents all status
            if (leaveStat >= 0)
            {
                queryString += " AND Leave.Leave_Status_ID = " + leaveStat;
            }

            // adds a filter query if search box contains character(s), note that 0 length means the search box is empty
            if (search.Length > 0)
            {
                queryString += " AND (Employee.Employee_ID LIKE '%" + search + "%' " +
                    "OR CONCAT(First_Name, ' ', Last_Name) LIKE '%" + search + "%')";
            }

            if (sDate.Length > 0)
            {
                queryString += " AND Leave.Start_Date >= '" + sDate + "'"; 
            }

            if (eDate.Length > 0)
            {
                queryString += " AND Leave.Start_Date <= '" + eDate + "'";
            }

            if (order.Length > 0)
            {
                queryString += " ORDER BY " + order;
            }

            return queryString;
        }

        private Dictionary<string, string> OrderByList()
        {
            var orderByList = new Dictionary<string, string>
            {
                { "Employee.First_Name ASC", "First Name | Ascending" },
                { "Employee.First_Name DESC", "First Name | Descending" },
                { "Employee.Last_Name ASC", "Last Name | Ascending" },
                { "Employee.Last_Name DESC", "Last Name | Descending" },
                { "Employee.Employee_ID ASC", "Employee ID | Ascending" },
                { "Employee.Employee_ID DESC", "Employee ID | Descending" }
            };
            return orderByList;
        }

        private Dictionary<int, string> LeaveStatusList()
        {
            Dictionary<int, string> leaveStatusList = new Dictionary<int, string>();

            leaveStatusList.Add(-1, "All Statuses");
            foreach (var status in DBLeaveStatusList())
            {
                if (!status.Value.Equals("Pending_LM") && !status.Value.Equals("Pending_HR"))
                    leaveStatusList.Add(status.Key, status.Value);
            }

            return leaveStatusList;
        }

        public ActionResult Cancel(int applicationID)
        {
            sLeaveModel leaveModel = GetLeaveModel("Leave_Application_ID", applicationID)[0];
            int cancelledID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Cancelled_HR").Key;
            string queryString = "UPDATE dbo.Leave SET Leave_Status_ID = '" + cancelledID + "' WHERE Leave_Application_ID = '" + applicationID + "'";
            DBExecuteQuery(queryString);

            DBRefundLeaveBalance(applicationID);

            int approvedID = DBLeaveStatusList().FirstOrDefault(obj => obj.Value == "Approved").Key;
            string quditString = "INSERT INTO dbo.Audit_Leave_Application (Leave_Application_ID, Column_Name, Value_Before, Value_After, Modified_By, Modified_On) " +
                  "VALUES('" + applicationID + "', 'Leave_Status_ID', '" + approvedID + "','" + cancelledID + "','" + GetLoggedInID() + "','" + DateTime.Today.ToString("yyyy-MM-dd") + "')";
            DBExecuteQuery(quditString);

            string message = "";
            message = "Your " + leaveModel.leaveTypeName + " leave application from " + leaveModel.startDate.ToShortDateString() + " to " + leaveModel.returnDate.ToShortDateString() + " with ID " + applicationID + " has been cancelled by human resources.";
            BackgroundJob.Enqueue(() => SendMail(GetEmployeeModel(leaveModel.employeeID).email, message));

            TempData["WarningMessage"] = "Leave application <b>" + applicationID + "</b> has been cancelled successfully.";
            return RedirectToAction("View", new { appID = applicationID });
        }


    }
}