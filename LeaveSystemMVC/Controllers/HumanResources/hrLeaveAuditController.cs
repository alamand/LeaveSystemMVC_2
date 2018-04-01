﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;

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

    }
}