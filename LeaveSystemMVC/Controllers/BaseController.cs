using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Diagnostics;
using System.Configuration;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class BaseController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        protected int GetLoggedInID()
        {
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var identity = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            string loggedInID = identity.ToString();
            loggedInID = loggedInID.Substring(loggedInID.Length - 5);
            return Convert.ToInt32(loggedInID);
        }

        protected void SetMessageViewBags()
        {
            ViewBag.InfoMessage = TempData["InfoMessage"];
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.WarningMessage = TempData["WarningMEssage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
        }

        protected Balance GetLeaveBalanceModel(int empID = 0)
        {
            var lb = new Balance();
            SqlCommand cmd = new SqlCommand();

            // this query will be used to get the default durations on all leave types
            cmd.CommandText = "Select * FROM dbo.Leave_Type";        

            // the word Duration is the column name in dbo.Leave_Types
            string balanceColName = "Duration";

            // change the query, gets balances on all leave types for a specific employee
            if (empID > 0)
            {
                // recalls this method to get the leave IDs and default balances
                lb = GetLeaveBalanceModel();
                lb.empId = empID;

                // sets all balances to 0, so that if the database does not containt a record for the employee, it won't use the default balance
                lb.annual = lb.compassionate = lb.daysInLieu = lb.maternity = lb.sick = lb.unpaid = lb.pilgrimage = lb.shortHours = 0;

                // the word Balance is the column name in dbo.Leave_Balance
                balanceColName = "Balance";

                cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
                cmd.CommandText = "SELECT Leave_Type.Leave_Type_ID, Leave_Name, Balance " +
                    "FROM dbo.Leave_Balance, dbo.Leave_Type " +
                    "Where Leave_Type.Leave_Type_ID = Leave_Balance.Leave_Type_ID AND Employee_ID = @empID";
            }

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                switch ((string)row["Leave_Name"])
                {
                    case "Annual":
                        lb.annualID = (int)row["Leave_Type_ID"];
                        lb.annual = (decimal)row[balanceColName];
                        break;

                    case "Maternity":
                        lb.maternityID = (int)row["Leave_Type_ID"];
                        lb.maternity = (decimal)row[balanceColName];
                        break;

                    case "Sick":
                        lb.sickID = (int)row["Leave_Type_ID"];
                        lb.sick = (decimal)row[balanceColName];
                        break;

                    case "DIL":
                        lb.daysInLieuID = (int)row["Leave_Type_ID"];
                        lb.daysInLieu = (decimal)row[balanceColName];
                        break;

                    case "Compassionate":
                        lb.compassionateID = (int)row["Leave_Type_ID"];
                        lb.compassionate = (decimal)row[balanceColName];
                        break;

                    case "Short_Hours":
                        lb.shortHoursID = (int)row["Leave_Type_ID"];
                        lb.shortHours = (decimal)row[balanceColName];
                        break;

                    case "Pilgrimage":
                        lb.pilgrimageID = (int)row["Leave_Type_ID"];
                        lb.pilgrimage = (decimal)row[balanceColName];
                        break;

                    case "Unpaid":
                        lb.unpaidID = (int)row["Leave_Type_ID"];
                        lb.unpaid = (decimal)row[balanceColName];
                        break;

                    default:
                        break; ;
                }
            }
            
            return lb;
        }

        protected Employee GetEmployeeModel(int empID)
        {
            Employee employeeModel = new Employee();
            SqlCommand cmd = new SqlCommand();
            DataBase db = new DataBase();

            bool reportingIDExist = IsReportingExist(empID);                
            bool startDateExist = IsStartDateExist(empID);

            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;

            if (reportingIDExist && startDateExist) // @TODO: fix this when line manager substitution and employment period feature is added
                cmd.CommandText = "SELECT * FROM dbo.Employee, dbo.Reporting, dbo.Employment_Period WHERE Employee.Employee_ID = Employment_Period.Employee_ID AND " +
                    "Employee.Employee_ID = Reporting.Employee_ID AND Employee.Employee_ID = @empID";
            else if (reportingIDExist)
                cmd.CommandText = "SELECT * FROM dbo.Employee, dbo.Reporting WHERE Employee.Employee_ID = Reporting.Employee_ID AND Employee.Employee_ID = @empID";
            else if (startDateExist)
                cmd.CommandText = "SELECT * FROM dbo.Employee, dbo.Employment_Period WHERE Employee.Employee_ID = Employment_Period.Employee_ID AND Employee.Employee_ID = @empID";
            else
                cmd.CommandText = "SELECT * FROM dbo.Employee WHERE Employee.Employee_ID = @empID";

            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                // @TODO: fix the DBNull checking after the DB has been updated
                employeeModel.staffID = (int)row["Employee_ID"];
                employeeModel.firstName = (string)row["First_Name"];
                employeeModel.lastName = (string)row["Last_Name"];
                employeeModel.userName = (string)row["User_Name"];
                employeeModel.designation = (string)row["Designation"];
                employeeModel.email = (string)row["Email"];
                employeeModel.gender = ((string)row["Gender"])[0];
                employeeModel.deptID = (row["Department_ID"] != DBNull.Value) ? (int)row["Department_ID"] : (int?)null;
                employeeModel.phoneNo = (row["Ph_No"] != DBNull.Value) ? (string)row["Ph_No"] : "";
                employeeModel.accountStatus = (bool)row["Account_Status"];
                if (reportingIDExist)
                    employeeModel.reportsToLineManagerID = (row["Report_To_ID"] != DBNull.Value) ? (int)row["Report_To_ID"] : (int?)null;
                employeeModel.religionID = (row["Religion_ID"] != DBNull.Value) ? (int)row["Religion_ID"] : 0;
                employeeModel.dateOfBirth = (!DBNull.Value.Equals(row["Date_Of_Birth"])) ? (DateTime)row["Date_Of_Birth"] : new DateTime();
                employeeModel.nationalityID = (row["Nationality_ID"] != DBNull.Value) ? (int)row["Nationality_ID"] : 0;
                employeeModel.onProbation = (row["Probation"] != DBNull.Value) ? (bool)row["Probation"] : false;
                if (startDateExist)
                    employeeModel.empStartDate = (row["Emp_Start_Date"] != DBNull.Value) ? (DateTime)row["Emp_Start_Date"] : new DateTime();
            }

            
            cmd.CommandText = "SELECT Role_ID FROM dbo.Employee_Role WHERE Employee_ID = @empID";
            dataTable = db.Fetch(cmd);

            Dictionary dic = new Dictionary();
            var roleDictionary = dic.GetRole();

            foreach (DataRow row in dataTable.Rows)
            {
                if (roleDictionary[(int)row["Role_ID"]].Equals("Staff"))
                    employeeModel.isStaff = true;

                if (roleDictionary[(int)row["Role_ID"]].Equals("LM"))
                    employeeModel.isLM = true;

                if (roleDictionary[(int)row["Role_ID"]].Equals("HR"))
                    employeeModel.isHR = true;

                if (roleDictionary[(int)row["Role_ID"]].Equals("HR_Responsible"))
                    employeeModel.isHRResponsible = true;

                if (roleDictionary[(int)row["Role_ID"]].Equals("Admin"))
                    employeeModel.isAdmin = true;
            }
                

            return employeeModel;
        }

        protected List<Employee> GetEmployeeModel()
        {
            List<Employee> empList = new List<Employee>(); 
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "SELECT Employee_ID FROM dbo.Employee";

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                empList.Add(GetEmployeeModel((int)row["Employee_ID"]));
            }
            
            return empList;
        }

        protected List<Employee> GetEmploymentPeriod(int empID)
        {
            List<Employee> employmentList = new List<Employee>();
            SqlCommand cmd = new SqlCommand();
            DataBase db = new DataBase();

            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.CommandText = "SELECT * FROM dbo.Employment_Period WHERE Employee_ID = @empID ORDER BY Emp_Start_Date";

            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                var employment = new Employee
                {
                    staffID = (int)row["Employee_ID"],
                    empStartDate = (row["Emp_Start_Date"] != DBNull.Value) ? (DateTime)row["Emp_Start_Date"] : new DateTime(),
                    empEndDate = (row["Emp_End_Date"] != DBNull.Value) ? (DateTime)row["Emp_End_Date"] : new DateTime()
                };
                employmentList.Add(employment);
            }
           
            return employmentList;
        }

        private bool IsReportingExist(int empID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.CommandText = "SELECT COUNT(*) FROM dbo.Reporting WHERE Employee_ID = @empID";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        private bool IsStartDateExist(int empID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.CommandText = "SELECT COUNT(*) FROM dbo.Employment_Period WHERE Employee_ID = @empID";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        protected bool IsLeaveBalanceExist(int empID, int typeID)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
            cmd.CommandText = "SELECT COUNT(*) FROM dbo.Leave_Balance WHERE Employee_ID = @empID AND Leave_Type_ID = @typeID";
            DataBase db = new DataBase();
            return db.Contains(cmd);
        }

        protected bool IsPublicHoliday(DateTime date)
        {
            bool isPublicHoliday = false;

            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@date", SqlDbType.DateTime).Value = date.ToString("yyyy-MM-dd");
            cmd.CommandText = "SELECT * FROM dbo.Public_Holiday WHERE Date = @date";

            DataTable dataTable = db.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                DateTime day = (DateTime)row["Date"];
                isPublicHoliday = date.Equals(day) ? true : false;
            }

            return isPublicHoliday;
        }

        protected List<Leave> GetLeaveModel(string listFor = "", int id = 0)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT Leave_Application_ID, Employee.Employee_ID, First_Name, Last_Name, Leave.Start_Date, Leave.Reporting_Back_Date, Leave.Leave_Type_ID, Leave_Type.Leave_Name, Leave_Type.Display_Name as Leave_Type_Display, " +
                "Contact_Outside_UAE, Comment, Documentation, Flight_Ticket, Total_Leave, Start_Hrs, End_Hrs, Leave.Leave_Status_ID, Status_Name, Leave_Status.Display_Name as Leave_Status_Display, HR_Comment, LM_Comment, Leave.Personal_Email, Leave.Is_Half_Start_Date, Leave.Is_Half_Reporting_Back_Date " +
                "FROM dbo.Leave, dbo.Employee, dbo.Leave_Type, dbo.Leave_Status, dbo.Department, dbo.Reporting " +
                "WHERE Leave.Employee_ID = Employee.Employee_ID AND Leave.Leave_Type_ID = Leave_Type.Leave_Type_ID AND " +
                "Leave.Leave_Status_ID = Leave_Status.Leave_Status_ID AND Department.Department_ID = Employee.Department_ID AND Employee.Employee_ID = Reporting.Employee_ID";

            if (!listFor.Equals("") && id != 0)
            {
                cmd.Parameters.Add("@listFor", SqlDbType.NChar).Value = listFor;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                cmd.CommandText += " AND " + listFor + " = @id";
            }

            return GetLeaveModel(cmd);
        }

        protected List<Leave> GetLeaveModel(SqlCommand sqlCommand)
        {
            List<Leave> leaveList = new List<Leave>();
            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(sqlCommand);

            foreach (DataRow row in dataTable.Rows)
            {
                var leave = new Leave
                {
                    leaveAppID = (int)row["Leave_Application_ID"],
                    employeeID = (int)row["Employee_ID"],
                    employeeName = (string)row["First_Name"] + " " + (string)row["Last_Name"],
                    startDate = (!DBNull.Value.Equals(row["Start_Date"])) ? (DateTime)row["Start_Date"] : new DateTime(0, 0, 0),
                    returnDate = (!DBNull.Value.Equals(row["Reporting_Back_Date"])) ? (DateTime)row["Reporting_Back_Date"] : new DateTime(0, 0, 0),
                    leaveTypeID = (int)row["Leave_Type_ID"],
                    leaveTypeName = (string)row["Leave_Name"],
                    leaveTypeDisplayName = (string)row["Leave_Type_Display"],
                    contactDetails = (!DBNull.Value.Equals(row["Contact_Outside_UAE"])) ? (string)row["Contact_Outside_UAE"] : "",
                    comments = (!DBNull.Value.Equals(row["Comment"])) ? (string)row["Comment"] : "",
                    documentation = (!DBNull.Value.Equals(row["Documentation"])) ? (string)row["Documentation"] : "",
                    bookAirTicket = (!DBNull.Value.Equals(row["Flight_Ticket"])) ? (bool)row["Flight_Ticket"] : false,
                    leaveDuration = (decimal)row["Total_Leave"],
                    shortStartTime = (!DBNull.Value.Equals(row["Start_Hrs"])) ? (TimeSpan)row["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                    shortEndTime = (!DBNull.Value.Equals(row["End_Hrs"])) ? (TimeSpan)row["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                    leaveStatusID = (int)row["Leave_Status_ID"],
                    leaveStatusName = (string)row["Status_Name"],
                    leaveStatusDisplayName = (string)row["Leave_Status_Display"],
                    hrComment = (!DBNull.Value.Equals(row["HR_Comment"])) ? (string)row["HR_Comment"] : "",
                    lmComment = (!DBNull.Value.Equals(row["LM_Comment"])) ? (string)row["LM_Comment"] : "",
                    email = (!DBNull.Value.Equals(row["Personal_Email"])) ? (string)row["Personal_Email"] : "",
                    isStartDateHalfDay = (!DBNull.Value.Equals(row["Is_Half_Start_Date"])) ? (bool)row["Is_Half_Start_Date"] : false,
                    isReturnDateHalfDay = (!DBNull.Value.Equals(row["Is_Half_Reporting_Back_Date"])) ? (bool)row["Is_Half_Reporting_Back_Date"] : false
                };
                leaveList.Add(leave);
            }

            return leaveList;
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

        private Tuple<int, DateTime> GetLeaveApproval(int appID, string role)
        {
            Dictionary dic = new Dictionary();
            int prevStatus = dic.GetLeaveStatus().FirstOrDefault(obj => obj.Value ==  (role.Equals("LM") ? "Pending_LM" : "Pending_HR")).Key;

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@appID", SqlDbType.Int).Value = appID;
            cmd.Parameters.Add("@prevStatus", SqlDbType.Int).Value = prevStatus;
            cmd.CommandText = "SELECT Modified_By, Modified_On FROM dbo.Audit_Leave_Application WHERE Leave_Application_ID = @appID AND Value_Before = @prevStatus";

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            int approvedByID = 0;
            DateTime date = new DateTime();

            foreach (DataRow row in dataTable.Rows)
            {
                approvedByID = (int)row["Modified_By"];
                date = (DateTime)row["Modified_On"];
            }

            return new Tuple<int, DateTime>(approvedByID, date);
        }

        protected List<Reporting> GetReportingList(int empID = 0)
        {
            List<Reporting> reportingList = new List<Reporting>();
            Dictionary dic = new Dictionary();

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.CommandText = "SELECT Report_To_ID, Employee_ID, From_ID, To_ID, Substitution_Level, is_Active " +
                "FROM dbo.Reporting_Map " +
                "FULL OUTER JOIN dbo.Reporting ON dbo.Reporting_Map.Original_ID = Reporting.Report_To_ID";

            if (empID != 0)
                cmd.CommandText += " WHERE Report_To_ID = @empID OR To_ID = @empID OR From_ID = @empID";

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                Reporting reporting = new Reporting
                {
                    employeeID = (int)row["Employee_ID"],
                    reportToID = (int)row["Report_To_ID"],
                    fromID = (row["From_ID"] != DBNull.Value) ? (int)row["From_ID"] : (int?)null,
                    toID = (row["To_ID"] != DBNull.Value) ? (int)row["To_ID"] : (int?)null,
                    subLevel = (row["Substitution_Level"] != DBNull.Value) ? (int)row["Substitution_Level"] : (int?)null,
                    isActive = (row["Is_Active"] != DBNull.Value) ? (bool)row["is_Active"] : (bool?)null,
                    employeeName = dic.GetEmployee()[(int)row["Employee_ID"]]
                };
                reportingList.Add(reporting);
            }
            
            return reportingList;
        }

        [HttpGet]
        public ActionResult GeneratePDF(int appID)
        {
            try
            {
                #pragma warning disable CS0618 // Type or member is obsolete

                Leave leave = GetLeaveModel("Leave_Application_ID", appID)[0];
                Employee employee = GetEmployeeModel(leave.employeeID);
                Dictionary dic = new Dictionary();

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
                XImage image = XImage.FromFile(Server.MapPath("~/Content/TAG_logo.png"));
                gfx.DrawImage(image, 0, 0, 150, 50);

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
                int newLine = 20;

                // Draw the header above the first box
                gfx.DrawString("Application Details", fontHeader, XBrushes.Black, new XRect(0, 80, page.Width, page.Height), XStringFormat.TopCenter);

                gfx.DrawString("Leave Application ID", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine + 10);
                gfx.DrawString(leave.leaveAppID.ToString(), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Leave Type", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(leave.leaveTypeDisplayName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Leave Status", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(leave.leaveStatusDisplayName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Employee ID", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(leave.employeeID.ToString(), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Employee Name", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(leave.employeeName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Designation", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(employee.designation, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Department", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(employee.deptID != null ? dic.GetDepartment()[(int)employee.deptID] : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);     // @TODO: Change department to department name

                gfx.DrawString("Start Date", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(leave.startDate.ToString("dd/MM/yyyy"), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Return Date", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(leave.returnDate.ToString("dd/MM/yyyy"), fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Start Time", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString((leave.shortStartTime != new TimeSpan()) ? leave.shortStartTime.ToString() : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("End Time", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString((leave.shortEndTime != new TimeSpan()) ? leave.shortEndTime.ToString() : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Duration", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(leave.leaveDuration.ToString() + " day(s)", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Documentation", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString(leave.documentation, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Contact Details (outside UAE)", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);

                gfx.DrawString("Phone Number", fontBold, XBrushes.Black, xAxisTitle + 20, yAxis = yAxis + 20);
                gfx.DrawString(leave.contactDetails, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Personal E-Mail", fontBold, XBrushes.Black, xAxisTitle + 20, yAxis = yAxis + 20);
                gfx.DrawString(leave.email, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Employee Comment", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 20);
                tf.DrawString(leave.comments, fontRegular, XBrushes.Black, new XRect(xAxisValue, yAxis - 10, 350, 100));

                // Draw the header above the second box
                gfx.DrawString("Approvals", fontHeader, XBrushes.Black, new XRect(0, 540, page.Width, page.Height), XStringFormat.TopCenter);

                // Get the LM's employee id and date of approval for this leave application
                Tuple<int, DateTime> approvalLM = GetLeaveApproval(leave.leaveAppID, "LM");
                Employee approvalLMEmployee = GetEmployeeModel(approvalLM.Item1);

                gfx.DrawString("Division Head", fontBold, XBrushes.Black, xAxisTitle, yAxis = 580);
                gfx.DrawString(approvalLMEmployee.firstName + " " + approvalLMEmployee.lastName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Date", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString((approvalLM.Item2 != new DateTime()) ? approvalLM.Item2.ToString("dd/MM/yyyy") : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Line Manager Comment", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                tf.DrawString(leave.lmComment, fontRegular, XBrushes.Black, new XRect(xAxisValue, yAxis - 10, 350, 100));

                // Get the HR's employee id and date of approval for this leave application
                Tuple<int, DateTime> approvalHR = GetLeaveApproval(leave.leaveAppID, "HR");
                Employee approvalHREmployee = GetEmployeeModel(approvalHR.Item1);

                gfx.DrawString("HR Head", fontBold, XBrushes.Black, xAxisTitle, yAxis = yAxis + 100);
                gfx.DrawString(approvalHREmployee.firstName + " " + approvalHREmployee.lastName, fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("Date", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                gfx.DrawString((approvalHR.Item2 != new DateTime()) ? approvalHR.Item2.ToString("dd/MM/yyyy") : "", fontRegular, XBrushes.Black, xAxisValue, yAxis);

                gfx.DrawString("HR Comment", fontBold, XBrushes.Black, xAxisTitle, yAxis += newLine);
                tf.DrawString(leave.hrComment, fontRegular, XBrushes.Black, new XRect(xAxisValue, yAxis - 10, 350, 100));

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

    }
}