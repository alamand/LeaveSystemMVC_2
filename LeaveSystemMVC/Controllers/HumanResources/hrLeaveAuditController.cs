using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using System.Drawing;
using System.Diagnostics;

namespace LeaveSystemMVC.Controllers.HumanResources
{
    public class hrLeaveAuditController : ControllerBase
    {
        // GET: hrLeaveAudit
        public ActionResult Index(int filterLeaveType = -1, string filterStartDate = "", string filterEndDate = "")
        {
            filterLeaveType = (filterLeaveType == -1) ? DBLeaveTypeList().FirstOrDefault(obj => obj.Value == "Annual").Key : filterLeaveType;
            string queryString = GetFilteredQuery(filterLeaveType, filterStartDate, filterEndDate);
            var model = GetTotalConsumption(queryString);

            ViewData["LeaveTypeList"] = DBLeaveNameList();
            ViewData["SelectedLeaveType"] = filterLeaveType;
            ViewData["SelectedStartDate"] = filterStartDate;
            ViewData["SelectedEndDate"] = filterEndDate;

            return View(model);
        }

        [HttpPost]
        public ActionResult Filter(FormCollection form)
        {
            int leaveTypeID = Convert.ToInt32(form["selectedLeaveType"]);
            string startDate = form["selectedStartDate"];
            string endDate = form["selectedEndDate"];
            return RedirectToAction("Index", new { filterLeaveType = leaveTypeID, filterStartDate = startDate, filterEndDate = endDate });
        }

        private string GetFilteredQuery(int leaveType, string sDate, string eDate)
        {
            var queryString = "SELECT Employee.Employee_ID, First_Name, Last_Name, Value_Before, Value_After, Leave_Name, Leave.Start_Date " +
                "FROM dbo.Employee " +
                "LEFT JOIN dbo.Leave_Balance ON Employee.Employee_ID = Leave_Balance.Employee_ID " +
                "INNER JOIN dbo.Department ON Employee.Department_ID = Department.Department_ID " +
                "INNER JOIN dbo.Leave_Type ON Leave_Balance.Leave_Type_ID = Leave_Type.Leave_Type_ID " +
                "LEFT JOIN dbo.Audit_Leave_Balance ON Leave_Balance.Leave_Balance_ID = Audit_Leave_Balance.Leave_Balance_ID " +
                "AND Audit_Leave_Balance.Comment != 'Leave quota per annum' AND Audit_Leave_Balance.Comment != 'Monthly reset'";

            if (leaveType >= 0)
                queryString += " AND Leave_Balance.Leave_Type_ID = '" + leaveType + "'";

            queryString += " LEFT JOIN dbo.Leave ON Leave.Leave_Application_ID = Audit_Leave_Balance.Leave_Application_ID";

            if (sDate.Length > 0)
                queryString += " WHERE (Leave.Start_Date >= '" + sDate + "' OR Leave.Start_Date IS NULL)";

            if (eDate.Length > 0)
            {
                queryString += (sDate.Length > 0) ? " AND" : " WHERE";
                queryString += " (Leave.Start_Date <= '" + eDate + "' OR Leave.Start_Date IS NULL)";
            }

            queryString += " ORDER BY First_Name, Last_Name";

            return queryString;
        }


        private List<Tuple<int, string, decimal>> GetTotalConsumption(string queryString)
        {
            List<Tuple<int, string, decimal>> empConsumptionList = new List<Tuple<int, string, decimal>>();
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int empID = (int)reader["Employee_ID"];
                        string firstName = (string)reader["First_Name"];
                        string lastName = (string)reader["Last_Name"];
                        string leaveType = (string)reader["Leave_Name"];
                        decimal vBefore = decimal.Parse((reader["Value_Before"] != DBNull.Value) ? (string)reader["Value_Before"] : "0");
                        decimal vAfter = decimal.Parse((reader["Value_After"] != DBNull.Value) ? (string)reader["Value_After"] : "0");

                        decimal consuption = 0;
                        if (leaveType.Equals("Compassionate") || leaveType.Equals("Unpaid"))
                            consuption = vAfter - vBefore;
                        else
                            consuption = vBefore - vAfter;

                        if (empConsumptionList.Any(m => m.Item1 == empID))
                        {
                            int indx = empConsumptionList.FindIndex(m => m.Item1 == empID);
                            consuption += empConsumptionList[indx].Item3;
                            empConsumptionList.RemoveAt(indx);
                        }

                        empConsumptionList.Add(new Tuple<int, string, decimal>(empID, firstName + " " + lastName, consuption));
                    }
                }
                connection.Close();
            }

            return empConsumptionList;
        }

        [HttpGet]
        public ActionResult GenerateAnnualLeaveAuditPDF()
        {
            try
            {
                #pragma warning disable CS0618 // Type or member is obsolete
                List<sEmployeeModel> employeeList = GetEmployeeModel();

                // Create a new PDF document
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Annual Leave Audit " + (DateTime.Now.Year-1);
                document.Info.Author = Session["UserName"].ToString();

                // Create an empty page
                PdfPage page = document.AddPage();

                // Set page margins
                page.TrimMargins.Top = 20;
                page.TrimMargins.Right = 35;
                page.TrimMargins.Left = 35;
                page.TrimMargins.Bottom = 20;

                // Get an XGraphics object for drawing
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Get an XTextFormatter object for writting multiple lines
                XTextFormatter tf = new XTextFormatter(gfx);

                // Create a font
                XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.WinAnsi);
                XFont font = new XFont("Calibri", 9, XFontStyle.Regular, options);

                int rowSize = 15;
                int colSize = 60;
                int nameColSize = 110;

                // Top row
                gfx.DrawRectangle(XPens.Black, new Rectangle(0, 0, nameColSize, rowSize));
                gfx.DrawRectangle(XPens.Black, new Rectangle(nameColSize, 0, colSize, rowSize));
                gfx.DrawRectangle(XPens.Black, new Rectangle(nameColSize + colSize, 0, colSize, rowSize));
                gfx.DrawRectangle(XPens.Black, new Rectangle(nameColSize + (colSize * 2), 0, colSize, rowSize));
                gfx.DrawRectangle(XPens.Black, new Rectangle(nameColSize + (colSize * 3), 0, (colSize * 5), rowSize));

                // header row
                gfx.DrawRectangle(XPens.Black, new Rectangle(0, rowSize, nameColSize, (rowSize * 4)));
                for (int x = nameColSize; x < (colSize * 9); x += colSize)
                {
                    gfx.DrawRectangle(XPens.Black, new Rectangle(x, rowSize, colSize, (rowSize * 4)));
                }

                // data rows
                int y = rowSize * 5;
                for (int row = 0; row < employeeList.Count; row++)
                {
                    gfx.DrawRectangle(XPens.Black, new Rectangle(0, y, nameColSize, rowSize));
                    for (int x = nameColSize; x < (colSize * 9); x += colSize)
                    {
                        gfx.DrawRectangle(XPens.Black, new Rectangle(x, y, colSize, rowSize));
                    }
                    y += rowSize;
                }

                // top row text
                gfx.DrawString("Leave balance as of", font, XBrushes.Black, 2, 12);
                DateTime endofThisYear = new DateTime(DateTime.Now.Year, 12, 31);
                gfx.DrawString(endofThisYear.ToString("dd/MM/yyyy"), font, XBrushes.Black, 112, 12);
                gfx.DrawString("Annual Leave", font, XBrushes.Black, 415, 12);

                // header row text
                int x2Position = rowSize + ((rowSize * 4) / 2) - (rowSize / 2) - 3;
                int x3Position = rowSize + ((rowSize * 4) / 3) - (rowSize / 3) - 1;
                int x4Position = rowSize + ((rowSize * 4) / 4) - (rowSize / 4) - 5;
                int x5Position = rowSize + ((rowSize * 4) / 5) - (rowSize / 5) - 6;

                tf.Alignment = XParagraphAlignment.Center;
                gfx.DrawString("Name", font, XBrushes.Black, new XRect(0, rowSize, nameColSize, (rowSize * 4)), XStringFormat.Center);
                gfx.DrawString("Joining Date", font, XBrushes.Black, new XRect(nameColSize, rowSize, colSize, (rowSize * 4)), XStringFormat.Center);
                tf.DrawString("Leave Start Date", font, XBrushes.Black, new XRect((nameColSize + colSize), x2Position, colSize, (rowSize * 4)));
                tf.DrawString("Closing Balance " + (DateTime.Now.Year - 1) + " as per HR", font, XBrushes.Black, new XRect((nameColSize + (colSize * 2)), x2Position, colSize, (rowSize * 4)));
                tf.DrawString("Opening Balance " + (new DateTime(DateTime.Now.Year, 1, 1)).ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect((nameColSize + (colSize * 3)), x3Position, colSize, (rowSize * 4)));
                tf.DrawString("Total Eligibility including opening bal (Days) upto " + (new DateTime(DateTime.Now.Year, 12, 31)).ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect((nameColSize + (colSize * 4)), x5Position, colSize, (rowSize * 4)));
                tf.DrawString("Leave Consumption", font, XBrushes.Black, new XRect((nameColSize + (colSize * 6)), x2Position, colSize, (rowSize * 4)));
                tf.DrawString("Closing Balance as at " + (new DateTime(DateTime.Now.Year, 12, 31)).ToString("dd/MM/yyyy"), font, XBrushes.Black, new XRect((nameColSize + (colSize * 7)), x3Position, colSize, (rowSize * 4)));

                // data rows text
                int yPosition = (rowSize * 6) - 3;
                for (int i = 0; i < employeeList.Count; i++)
                {
                    int xPosition = 2;
                    gfx.DrawString(employeeList[i].firstName + " " + employeeList[i].lastName, font, XBrushes.Black, xPosition, yPosition);
                    gfx.DrawString(employeeList[i].empStartDate.ToString("dd/MM/yyyy"), font, XBrushes.Black, xPosition += nameColSize, yPosition);
                    DateTime leaveStartDate = ((employeeList[i].empStartDate.Year >= DateTime.Now.Year-1) ? employeeList[i].empStartDate : new DateTime(DateTime.Now.Year-1, 1, 1));
                    gfx.DrawString(leaveStartDate.ToString("dd/MM/yyyy"), font, XBrushes.Black, xPosition += colSize, yPosition);

                    // @TODO: other datas to be filled
                    yPosition += rowSize;
                }
    
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