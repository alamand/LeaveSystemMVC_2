using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class hrEditBalanceController : Controller
    {
        // GET: hrEditBalance
        public ActionResult Index()
        {
            var model = new List<Models.sEmployeeModel>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Employee_ID, First_Name, Last_Name FROM dbo.Employee";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);      // retrieve employee id, first and last name
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // iterate through all employees in the database and add them all to the list
                    while (reader.Read())
                    {
                        var m = new Models.sEmployeeModel();
                        m.firstName = (string)reader["First_Name"];
                        m.lastName = (string)reader["Last_Name"];
                        m.staffID = (int)reader["Employee_ID"];
                        model.Add(m);
                    }
                }
                connection.Close();
            }

            return View(model);
        }

        private Models.sleaveBalanceModel ConstructLeaveBalance()
        {
            var lv = new Models.sleaveBalanceModel();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select * FROM dbo.Leave_Type";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);      // retrieve leave id and type

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    // iterate through all leave types in the database and update sleaveModel 
                    while (reader.Read())
                    {
                        if (reader["Leave_Name"].Equals("Annual"))
                        {
                            lv.annualID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("Maternity"))
                        {
                            lv.maternityID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("Sick"))
                        {
                            lv.sickID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("DIL"))
                        {
                            lv.daysInLieueID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("Compassionate"))
                        {
                            lv.compassionateID = (int)reader["Leave_ID"];
                        }
                        else if (reader["Leave_Name"].Equals("Short_Hours"))
                        {
                            lv.shortID = (int)reader["Leave_ID"];
                        }
                    }
                }
                connection.Close();
            }
            return lv;
        }

        public ActionResult Edit(string id)
        {
            try { if (id.Equals(null)) { return RedirectToAction("Index"); } } catch (Exception e) { return RedirectToAction("Index"); }
            int staff_id = int.Parse(id);
            var lv = ConstructLeaveBalance();

            ViewBag.sid = staff_id;                                             // pass the staff id to view
            ViewBag.annual = getBalance(staff_id, lv.annualID);                 // retrieve staff's annual balance and pass it to view
            ViewBag.maternity = getBalance(staff_id, lv.maternityID);           // retrieve staff's maternity balance and pass it to view
            ViewBag.sick = getBalance(staff_id, lv.sickID);                     // retrieve staff's sick balance and pass it to view
            ViewBag.daysInLieue = getBalance(staff_id, lv.daysInLieueID);       // retrieve staff's days in lieu balance and pass it to view
            ViewBag.compassionate = getBalance(staff_id, lv.compassionateID);   // retrieve staff's compassionate balance and pass it to view
            ViewBag.shortLeaveHours = getBalance(staff_id, lv.shortID);         // retrieve staff's short leave hours balance and pass it to view

            return View(lv);
        }

        private int getBalance(int staffid, int leaveid)
        {
            int balance = 0;                    // actual balance
            Boolean existingBalance = false;    // in case's where the record does not exist

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Balance FROM dbo.Leave_Balance Where Employee_ID ='" + staffid + "' And Leave_ID= '" + leaveid + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while(reader.Read()) 
                    {
                        balance = (int)reader["Balance"];
                        existingBalance = true;
                    }
                }
                connection.Close();

                if (!existingBalance)           // was the balance found?
                {
                    InsertBalance(staffid, leaveid, 0);     // if not, create a new one and set it to 0 by default
                }
            }

            return balance;
        }

        private void InsertBalance(int staffid, int leaveid, int bal)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string insertQuery = "Insert into dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) Values('" + staffid + "','" + leaveid + "','" + bal + "')";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(insertQuery, connection);

                connection.Open();
                using (var reader = command.ExecuteReader()){}
                connection.Close();
            }
        }

        [HttpPost]
        public ActionResult Edit(Models.sleaveBalanceModel mode)
        {
            int id = mode.empId;

            if (ModelState.IsValid)
            {
                UpdateBalance(mode.annualID, id, mode.annual);
                UpdateBalance(mode.maternityID, id, mode.maternity);
                UpdateBalance(mode.sickID, id, mode.sick);
                UpdateBalance(mode.compassionateID, id, mode.compassionate);
                UpdateBalance(mode.daysInLieueID, id, mode.daysInLieue);
                UpdateBalance(mode.shortID, id, mode.shortLeaveHours);
                Response.Write("<script> alert('Success. The Information has been updated.');</script>");
            }
            else
            {
                ModelState.AddModelError("errmsg", "Failed: An error occured. Please check your input and try again.");
            }
            return Edit(id.ToString());
        }

        private void UpdateBalance(int id, int sid, int duration)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryUpdate = "Update dbo.Leave_Balance SET Balance='" + duration + "' WHERE Leave_ID='" + id + "' And Employee_ID='" + sid + "'";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryUpdate, connection);
                connection.Open();
                using (var reader = command.ExecuteReader()){}
                connection.Close();
            }
        }
    }
}