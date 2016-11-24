using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;


namespace LeaveSystemMVC.Controllers
{
    public class sLeaveHistoryController : Controller
    {
        // GET: sLeaveHistory
        public ActionResult Index()
        {
            var model = new List<Models.sLeaveModel>();
            var claimsIdentity = User.Identity as System.Security.Claims.ClaimsIdentity;
            var c = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            ViewBag.claim = c;
            string a = c.ToString();
            a = a.Substring(a.Length - 5);
            //System.Diagnostics.Debug.WriteLine("id is:"+a + ".");


            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string query = "Select * FROM dbo.leave,dbo.Leave_Type where Employee_ID = '" + a + "' AND Start_Date < GETDATE() AND leave.Leave_ID = Leave_Type.Leave_ID and leave.Status IN (2,3,4,5,7)";

            using (var connection = new SqlConnection(connectionString)){
                var command = new SqlCommand(query, connection);

                connection.Open();

                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var leave = new Models.sLeaveModel();
                        string leave1 = (string)reader["Leave_Name"];

                        if (leave1.Equals("Annual"))
                            leave.leaveType = "Annual";

                        if (leave1.Equals("Sick"))
                            leave.leaveType = "Sick";

                        if (leave1.Equals("Compassionate"))
                            leave.leaveType = "Compassionate";

                        if (leave1.Equals("Maternity"))
                            leave.leaveType = "Maternity";

                        if (leave1.Equals("Short_Hours"))
                            leave.leaveType = "Short";

                        if (leave1.Equals("Unpaid"))
                            leave.leaveType = "Unpaid";

                        leave.startDate = (DateTime)reader["Start_Date"];

                        string date1 = leave.startDate.ToString("yyyy-MM-dd");
                        
                        //ViewBag.stDt = date1;

                        leave.endDate = (DateTime)reader["End_Date"];
                        string date2 = leave.endDate.ToString("yyyy-MM-dd");
                        //ViewBag.enDt = date2;
                        
                        leave.leaveDuration = (int)reader["Total_Leave_Days"];
                        if (!reader.IsDBNull(11))
                        {
                            leave.shortStartTime = (TimeSpan)reader["Start_Hrs"];
                        }else { 
                        leave.shortStartTime = new TimeSpan(0, 0, 0, 0, 0);
                        }
                        if (!reader.IsDBNull(12))
                        {
                            leave.shortEndTime = (TimeSpan)reader["End_Hrs"];
                        }
                        else
                        {
                            leave.shortEndTime = new TimeSpan(0, 0, 0, 0, 0);
                        }
                        leave.leaveStatus = (int)reader["Status"];
                        if (!reader.IsDBNull(15))
                            leave.hrComment = (string)reader["HR_Comment"];
                        else
                            leave.hrComment = "";
                        if (!reader.IsDBNull(14))
                            leave.lmComment = (string)reader["LM_Comment"];
                        else
                            leave.hrComment = "";

                        model.Add(leave);
                    }
                }
                connection.Close();
            }

                return View(model);
        }
    }
}