using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class hrEditDefaultLeaveDurationController : Controller
    {
        // GET: hrEditDefaultLeaveDuration
        [HttpGet]
        public ActionResult Index()
        {
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
                            ViewBag.annualDuration = (decimal)reader["Duration"];
                            ViewBag.annualID = (int)reader["Leave_ID"];
                            ViewBag.annualName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Maternity"))
                        {
                            ViewBag.matDuration = (decimal)reader["Duration"];
                            ViewBag.matID = (int)reader["Leave_ID"];
                            ViewBag.matName = (string)reader["Leave_Name"];
                        }

                        else if (reader["Leave_Name"].Equals("Sick"))
                        {
                            ViewBag.sickDuration = (decimal)reader["Duration"];
                            ViewBag.sickID = (int)reader["Leave_ID"];
                            ViewBag.sickName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("DIL"))
                        {
                            ViewBag.dilDuration = (decimal)reader["Duration"];
                            ViewBag.dilID = (int)reader["Leave_ID"];
                            ViewBag.dilName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Compassionate"))
                        {
                            ViewBag.compDuration = (decimal)reader["Duration"];
                            ViewBag.compID = (int)reader["Leave_ID"];
                            ViewBag.compName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Short_Hours"))
                        {
                            ViewBag.shortDuration = (decimal)reader["Duration"];
                            ViewBag.shortID = (int)reader["Leave_ID"];
                            ViewBag.shortName = (string)reader["Leave_Name"];
                        }
                    }
                }

                connection.Close();
            }

            return View(model);
        }
        public ActionResult Display()
        {
            var lv = new Models.sleaveBalanceModel();
            var model = new List<Models.sleaveBalanceModel>();
            var id = 0;
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
                            ViewBag.annualDuration = (decimal)reader["Duration"];
                            ViewBag.annualID = (int)reader["Leave_ID"];
                            ViewBag.annualName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Maternity"))
                        {
                            ViewBag.matDuration = (decimal)reader["Duration"];
                            ViewBag.matID = (int)reader["Leave_ID"];
                            ViewBag.matName = (string)reader["Leave_Name"];
                        }

                        else if (reader["Leave_Name"].Equals("Sick"))
                        {
                            ViewBag.sickDuration = (decimal)reader["Duration"];
                            ViewBag.sickID = (int)reader["Leave_ID"];
                            ViewBag.sickName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("DIL"))
                        {
                            ViewBag.dilDuration = (decimal)reader["Duration"];
                            ViewBag.dilID = (int)reader["Leave_ID"];
                            ViewBag.dilName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Compassionate"))
                        {
                            ViewBag.compDuration = (decimal)reader["Duration"];
                            ViewBag.compID = (int)reader["Leave_ID"];
                            ViewBag.compName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Short_Hours"))
                        {
                            ViewBag.shortDuration = (decimal)reader["Duration"];
                            ViewBag.shortID = (int)reader["Leave_ID"];
                            ViewBag.shortName = (string)reader["Leave_Name"];
                        }

                    }
                    //  model.Add(lv);
                }

                connection.Close();
            }

            return View(lv);
        }
        [HttpPost]
        public ActionResult Display(Models.sleaveBalanceModel mode)
        {

            if (ModelState.IsValid)
            {
                UpdateBalance(mode.annualID, mode.annual);
                UpdateBalance(mode.compassionateID, mode.compassionate);
                UpdateBalance(mode.daysInLieueID, mode.daysInLieue);
                UpdateBalance(mode.maternityID, mode.maternity);
                UpdateBalance(mode.sickID, mode.sick);
                UpdateBalance(mode.shortID, mode.shortLeaveHours);
                Response.Write("<script> alert('Success. The information has been updated.');</script>");
            }
            else
            {
                ModelState.AddModelError("errmsg", "Failed: An error occured. Please check your input and try again.");
            }
            return Display();
        }
        public void UpdateBalance(int id, decimal duration)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryUpdate = "Update dbo.Leave_Type SET Duration='" + duration + "' WHERE Leave_ID='" + id + "'";
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
        public void UpdateBalance(int id, double duration)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryUpdate = "Update dbo.Leave_Type SET Duration='" + duration + "' WHERE Leave_ID='" + id + "'";
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