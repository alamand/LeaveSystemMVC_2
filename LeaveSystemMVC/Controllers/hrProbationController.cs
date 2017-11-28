using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class hrProbationController : Controller
    {
        // GET: hrProbation

        public ActionResult Index()
        {
            var onProbation = new List<Models.sEmployeeModel>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select e.Employee_ID, First_Name, Last_Name, DATEDIFF(day,Emp_Start_Date,GETDATE()) as Dif_Date, Emp_Start_Date, Probation " +
                "FROM dbo.Employee e, dbo.Employment_Period p " +
                "WHERE e.Employee_ID = p.Employee_ID AND Emp_End_Date IS NULL AND (Probation=1 OR Probation IS NULL)" +
                "ORDER BY Emp_Start_Date";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);      
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employee = new Models.sEmployeeModel();
                        employee.staffID = (int)reader["Employee_ID"];
                        employee.firstName = (string)reader["First_Name"];
                        employee.lastName = (string)reader["Last_Name"];
                        employee.empStartDate = (DateTime)reader["Emp_Start_Date"];
                        ViewData["OnProbationDuration" + employee.staffID.ToString()] = (int)reader["Dif_Date"];
                        ViewData["OnProbationRemaining" + employee.staffID.ToString()] = (180 - (int)reader["Dif_Date"] < 0) ? 0 : 180 - (int)reader["Dif_Date"];
                        onProbation.Add(employee);
                    }
                }
                connection.Close();
            }

            var offProbation = new List<Models.sEmployeeModel>();
            string queryString2 = "Select e.Employee_ID, First_Name, Last_Name, DATEDIFF(day,Emp_Start_Date,GETDATE()) as Dif_Date, Emp_Start_Date, Probation " +
                "FROM dbo.Employee e, dbo.Employment_Period p " +
                "WHERE e.Employee_ID = p.Employee_ID AND Emp_End_Date IS NULL AND Probation=0";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);      
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // iterate through all employees in the database and add them all to the list
                    while (reader.Read())
                    {
                        var employee = new Models.sEmployeeModel();
                        employee.staffID = (int)reader["Employee_ID"];
                        employee.firstName = (string)reader["First_Name"];
                        employee.lastName = (string)reader["Last_Name"];
                        employee.empStartDate = (DateTime)reader["Emp_Start_Date"];
                        ViewData["OffProbDuration" + employee.staffID.ToString()] = (int)reader["Dif_Date"];
                        offProbation.Add(employee);
                    }
                }
                connection.Close();
            }

            ViewData["OnProbation"] = onProbation;
            ViewData["OffProbation"] = offProbation;
            return View();
        }

        public ActionResult SetProbation(string id, int probation)
        {
            try { if (id.Equals(null)) { return RedirectToAction("Index"); } } catch (Exception e) { return RedirectToAction("Index"); }
            int staff_id = int.Parse(id);

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "UPDATE dbo.Employee SET Probation = " + probation + " WHERE Employee_ID = " + staff_id;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader()){}
                connection.Close();
            }

            return RedirectToAction("Index");
        }
    }
}