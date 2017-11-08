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
            //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Employee_ID, First_Name, Last_Name FROM dbo.Employee";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
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
            TempData["Employee_Details"] = model;
            return View(model);
        }

        public ActionResult Edit(string id)
        {
            try { if (id.Equals(null)) { return RedirectToAction("Index"); } } catch (Exception e) { return RedirectToAction("Index"); }
            int staff_id = int.Parse(id);
            var lv = new Models.sleaveBalanceModel();
            var model = new List<Models.sleaveBalanceModel>();

            //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select * FROM dbo.Leave_Type";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
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
                    //  model.Add(lv);
                }

                connection.Close();
            }
            ViewBag.sid = staff_id;
            ViewBag.annual = getBalance(staff_id, lv.annualID);
            ViewBag.maternity = getBalance(staff_id, lv.maternityID);
            ViewBag.sick = getBalance(staff_id, lv.sickID);
            ViewBag.daysInLieue = getBalance(staff_id, lv.daysInLieueID);
            ViewBag.compassionate = getBalance(staff_id, lv.compassionateID);
            ViewBag.shortLeaveHours = getBalance(staff_id, lv.shortID);

            return View(lv);

        }
        public struct Empl
        {
            public Empl(string f, string l, int b) { firstN = f; lastN = l; id = b; }
            string firstN { get; set; }
            string lastN { get; set; }
            int id { set; get; }
        }
        public int getBalance(int staffid, int leaveid)
        {

            int result = 0, count = 0;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select Balance FROM dbo.Leave_Balance Where Employee_ID ='" + staffid + "' And Leave_ID= '" + leaveid + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count++;
                        result = (int)reader["Balance"];

                        //  model.Add(lv);
                    }
                }

                connection.Close();
                if (count == 0)
                {
                    InsertBalance(staffid, leaveid, 0);
                }
            }
            return result;
        }
        public void InsertBalance(int staffid, int leaveid, int bal)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string insertQuery = "Insert into dbo.Leave_Balance (Employee_ID, Leave_ID, Balance) Values('" + staffid + "','" + leaveid + "','" + bal + "')";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(insertQuery, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                }
                connection.Close();
            }
        }
        [HttpPost]
        public ActionResult Edit(Models.sleaveBalanceModel mode)
        {
            //int id = 0;
            int id = mode.empId;

            if (ModelState.IsValid)
            {
                UpdateBalance(mode.annualID, id, mode.annual);
                UpdateBalance(mode.compassionateID, id, mode.compassionate);
                UpdateBalance(mode.daysInLieueID, id, mode.daysInLieue);
                UpdateBalance(mode.maternityID, id, mode.maternityID);
                UpdateBalance(mode.shortID, id, mode.shortLeaveHours);
                Response.Write("<script> alert('Success. The Information has been updated.');</script>");
            }
            else
            {
                ModelState.AddModelError("errmsg", "Failed: An error occured. Please check your input and try again.");
            }
            return Edit(id.ToString());
        }
        public void UpdateBalance(int id, int sid, int duration)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryUpdate = "Update dbo.Leave_Balance SET Balance='" + duration + "' WHERE Leave_ID='" + id + "' And Employee_ID='" + sid + "'";
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryUpdate, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                }
                connection.Close();
            }
        }
    }
}