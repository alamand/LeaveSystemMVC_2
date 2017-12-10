﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class hrProbationAdministrationController : ControllerBase
    {
        // GET: hrProbationAdministration
        public ActionResult Index()
        {
            // holds the list of employees who are on probation
            var onProbation = new List<Models.sEmployeeModel>();
            
            // select all employees who are on probation
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT e.Employee_ID, First_Name, Last_Name, Department_Name, DATEDIFF(day,Emp_Start_Date,GETDATE()) as Dif_Date, Emp_Start_Date, Emp_End_Date, Probation " +
                "FROM dbo.Employee e, dbo.Employment_Period p, dbo.Department d " +
                "WHERE e.Employee_ID = p.Employee_ID AND e.Department_ID = d.Department_ID AND (Probation=1 OR Probation IS NULL) " +
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
                        ViewData["Department" + employee.staffID.ToString()] = (string)reader["Department_Name"];
                        employee.empStartDate = (DateTime)reader["Emp_Start_Date"];
                        ViewData["Duration" + employee.staffID.ToString()] = (int)reader["Dif_Date"];
                        onProbation.Add(employee);
                    }
                }
                connection.Close();
            }
            
            ViewData["OnProbation"] = onProbation;
            return View();
        }

        // called when a user confirms a probation for a specific staff
        public ActionResult SetProbation(string id)
        {
            try { if (id.Equals(null)) { return RedirectToAction("Index"); } } catch (Exception e) { return RedirectToAction("Index"); }
            int staff_id = int.Parse(id);

            string queryString = "UPDATE dbo.Employee SET Probation = 0 WHERE Employee_ID = " + staff_id;
            DBExecuteQuery(queryString);

            // calculate balance
            SetBalance(staff_id);

            return RedirectToAction("Index");
        }

        // sets the default balances for staffs who are confirmed off probation
        private void SetBalance(int staff_id)
        {
            // holds default durations for all leave types
            Models.sleaveBalanceModel leaveTypes = GetLeaveBalanceModel();

            // calculate annual leave balance
            // ((Total days of employement duration + remaining days of the years) / days in a month) * 1.833
            // where 1.833 is the given balance for each month
            DateTime startDate = GetStartDate(staff_id);
            DateTime endYearDate = new DateTime(startDate.Year, 12, 30);
            double annualBalance = (endYearDate.Subtract(startDate).TotalDays / 30) * 1.833;
       
            // holds the query to be executed, whether it is insert or update
            string queryString;

            // checks if the database contain a record of this staff id and annual id
            // if it does, then update that record, else insert a new record
            // this is to avoid duplicate records with the same staff & leave id.
            if (IsLeaveBalanceExist(leaveTypes.annualID, staff_id))  
                queryString = "UPDATE dbo.Leave_Balance SET Balance = " + (decimal)annualBalance + " WHERE Employee_ID = " + staff_id + " AND Leave_ID = " + leaveTypes.annualID;
            else
                queryString = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) Values('" + staff_id + "','" + leaveTypes.annualID + "','" + (decimal)annualBalance + "')";
            DBExecuteQuery(queryString);

            if (IsLeaveBalanceExist(leaveTypes.sickID, staff_id))
                queryString = "UPDATE dbo.Leave_Balance SET Balance = " + leaveTypes.sick + " WHERE Employee_ID = " + staff_id + " AND Leave_ID = " + leaveTypes.sickID;
            else
                queryString = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) Values('" + staff_id + "','" + leaveTypes.sickID + "','" + leaveTypes.sick + "')";
            DBExecuteQuery(queryString);

            if (IsLeaveBalanceExist(leaveTypes.compassionateID, staff_id))
                queryString = "UPDATE dbo.Leave_Balance SET Balance = " + leaveTypes.compassionate + " WHERE Employee_ID = " + staff_id + " AND Leave_ID = " + leaveTypes.compassionateID;
            else
                queryString = "INSERT INTO dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) Values('" + staff_id + "','" + leaveTypes.compassionateID + "','" + leaveTypes.compassionate + "')";
            DBExecuteQuery(queryString);
        }

        // gets the staffs start date from the database
        // note that it does not check for returning staff (assuming returning staffs will never be on probation)
        private DateTime GetStartDate(int staff_id)
        {
            DateTime date = new DateTime();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            string queryString2 = "SELECT e.Employee_ID, Emp_Start_Date " +
                "FROM dbo.Employee e, dbo.Employment_Period p " +
                "WHERE e.Employee_ID = p.Employee_ID AND e.Employee_ID = " + staff_id;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString2, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        date = (DateTime)reader["Emp_Start_Date"];
                    }
                }
                connection.Close();
            }

            return date;
        }

        // Checks if the staff and leave id are in the records
        private bool IsLeaveBalanceExist(int leave_id, int staff_id)
        {
            bool exists = false;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT Count(*) From dbo.Leave_Balance WHERE Employee_ID = " + staff_id + " AND Leave_ID = " + leave_id;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                int count = (int)command.ExecuteScalar();
                if (count > 0)
                {
                    exists = true;
                }
                connection.Close();
            }
            return exists;
        }

    }
}