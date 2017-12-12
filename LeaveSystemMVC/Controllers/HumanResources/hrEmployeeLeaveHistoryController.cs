using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class hrEmployeeLeaveHistoryController : Controller
    {
        // GET: hrEmployeeLeaveHistory
        public ActionResult Index()
        {
            // TODO: ADD FILTERS BY DEPARTMENT, EMPLOYEE and DATE (START-END).
            var model = new List<Tuple<Models.sEmployeeModel, Models.sLeaveModel>>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string query = "SELECT Leave_Application_ID, e.Employee_ID, First_Name, Last_Name, Leave_Name, Leave_Status_ID, Start_Date, Reporting_Back_Date, Start_Hrs, End_Hrs, Total_Leave " +
                "FROM dbo.leave l,dbo.Leave_Type t, dbo.Employee e " +
                "WHERE e.Employee_ID = l.Employee_ID AND l.Leave_ID = t.Leave_ID AND l.leave_Status_ID IN (2,3,4,5)" +
                "ORDER BY First_Name, Last_Name, Start_Date";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employee = new Models.sEmployeeModel
                        {
                            staffID = (int)reader["Employee_ID"],
                            firstName = (string)reader["First_Name"],
                            lastName = (string)reader["Last_Name"]
                        };

                        var leave = new Models.sLeaveModel
                        {
                            leaveID = reader["Leave_Application_ID"].ToString(),
                            leaveType = (string)reader["Leave_Name"],
                            startDate = (DateTime)reader["Start_Date"],
                            endDate = (DateTime)reader["Reporting_Back_Date"],
                            leaveDuration = (int)reader["Total_Leave"],
                            shortStartTime = (!DBNull.Value.Equals(reader["Start_Hrs"])) ? (TimeSpan)reader["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                            shortEndTime = (!DBNull.Value.Equals(reader["End_Hrs"])) ? (TimeSpan)reader["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0),
                            leaveStatus = (int)reader["Leave_Status_ID"]
                        };

                        model.Add(new Tuple<Models.sEmployeeModel, Models.sLeaveModel>(employee,leave));
                    }
                }
                connection.Close();
            }
            return View(model);
        }
    }
}