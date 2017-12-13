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
            var identity = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            string loggedInID = identity.ToString();
            loggedInID = loggedInID.Substring(loggedInID.Length - 5);

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string query = "Select Leave_Application_ID, Leave_Name, Leave_Status_ID, Start_Date, Reporting_Back_Date, Start_Hrs, End_Hrs, Total_Leave FROM dbo.leave,dbo.Leave_Type where Employee_ID = '" + loggedInID + "' AND leave.Leave_ID = Leave_Type.Leave_ID and leave.Leave_Status_ID IN (2,3,4,5)";

            using (var connection = new SqlConnection(connectionString)){
                var command = new SqlCommand(query, connection);

                connection.Open();

                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var leave = new Models.sLeaveModel();

                        leave.leaveID = (int)reader["Leave_Application_ID"];
                        leave.leaveType = (string)reader["Leave_Name"];
                        leave.startDate = (DateTime)reader["Start_Date"];
                        leave.endDate = (DateTime)reader["Reporting_Back_Date"];
                        leave.leaveDuration = (decimal)reader["Total_Leave"];
                        leave.shortStartTime = (!DBNull.Value.Equals(reader["Start_Hrs"])) ? (TimeSpan)reader["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0);
                        leave.shortEndTime = (!DBNull.Value.Equals(reader["End_Hrs"])) ? (TimeSpan)reader["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0);
                        leave.leaveStatus = (int)reader["Leave_Status_ID"];
                        model.Add(leave);
                    }
                }
                connection.Close();
            }

        return View(model);
        }
    }
}