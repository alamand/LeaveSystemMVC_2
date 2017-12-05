using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.Configuration;
using System.Data.SqlClient;

namespace LeaveSystemMVC.Controllers
{
    public class lmSubordinateLeaveHistoryController : ControllerBase
    {
        // GET: lmSubordinateLeaveHistory
        public ActionResult Index()
        {
            var model = new List<Tuple<sEmployeeModel, sLeaveModel>>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string query = "SELECT Leave_Application_ID, e.Employee_ID, First_Name, Last_Name, Leave_Name, Leave_Status_ID, Start_Date, Reporting_Back_Date, Start_Hrs, End_Hrs, Total_Leave_Days " +
                "FROM dbo.leave l,dbo.Leave_Type t, dbo.Employee e " +
                "WHERE e.Reporting_ID = "+ GetLoggedInID() +" AND e.Employee_ID = l.Employee_ID AND l.Leave_ID = t.Leave_ID AND l.leave_Status_ID IN (2,3,4,5)" +
                "ORDER BY First_Name, Last_Name, Start_Date";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var leave = new sLeaveModel();
                        var employee = new sEmployeeModel();

                        employee.staffID = (int)reader["Employee_ID"];
                        employee.firstName = (string)reader["First_Name"];
                        employee.lastName = (string)reader["Last_Name"];

                        leave.leaveID = reader["Leave_Application_ID"].ToString();
                        leave.leaveType = (string)reader["Leave_Name"];
                        leave.startDate = (DateTime)reader["Start_Date"];
                        leave.endDate = (DateTime)reader["Reporting_Back_Date"];
                        leave.leaveDuration = (int)reader["Total_Leave_Days"];
                        leave.shortStartTime = (!DBNull.Value.Equals(reader["Start_Hrs"])) ? (TimeSpan)reader["Start_Hrs"] : new TimeSpan(0, 0, 0, 0, 0);
                        leave.shortEndTime = (!DBNull.Value.Equals(reader["End_Hrs"])) ? (TimeSpan)reader["End_Hrs"] : new TimeSpan(0, 0, 0, 0, 0);
                        leave.leaveStatus = (int)reader["Leave_Status_ID"];

                        model.Add(new Tuple<sEmployeeModel, sLeaveModel>(employee, leave));
                    }
                }
                connection.Close();
            }
            return View(model);
        }

    }
}