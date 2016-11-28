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
                            ViewBag.annualDuration = (int)reader["Leave_Duration"];
                            ViewBag.annualID = (int)reader["Leave_ID"];
                            ViewBag.annualName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Maternity"))
                        {
                            ViewBag.matDuration = (int)reader["Leave_Duration"];
                            ViewBag.matID = (int)reader["Leave_ID"];
                            ViewBag.matName = (string)reader["Leave_Name"];
                        }

                        else if (reader["Leave_Name"].Equals("Sick"))
                        {
                            ViewBag.sickDuration = (int)reader["Leave_Duration"];
                            ViewBag.sickID = (int)reader["Leave_ID"];
                            ViewBag.sickName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("DIL"))
                        {
                            ViewBag.dilDuration = (int)reader["Leave_Duration"];
                            ViewBag.dilID = (int)reader["Leave_ID"];
                            ViewBag.dilName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Compassionate"))
                        {
                            ViewBag.compDuration = (int)reader["Leave_Duration"];
                            ViewBag.compID = (int)reader["Leave_ID"];
                            ViewBag.compName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Short_Hours"))
                        {
                            ViewBag.shortDuration = (int)reader["Leave_Duration"];
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
                            ViewBag.annualDuration = (int)reader["Duration"];
                            ViewBag.annualID = (int)reader["Leave_ID"];
                            ViewBag.annualName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Maternity"))
                        {
                            ViewBag.matDuration = (int)reader["Duration"];
                            ViewBag.matID = (int)reader["Leave_ID"];
                            ViewBag.matName = (string)reader["Leave_Name"];
                        }

                        else if (reader["Leave_Name"].Equals("Sick"))
                        {
                            ViewBag.sickDuration = (int)reader["Duration"];
                            ViewBag.sickID = (int)reader["Leave_ID"];
                            ViewBag.sickName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("DIL"))
                        {
                            ViewBag.dilDuration = (int)reader["Duration"];
                            ViewBag.dilID = (int)reader["Leave_ID"];
                            ViewBag.dilName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Compassionate"))
                        {
                            ViewBag.compDuration = (int)reader["Duration"];
                            ViewBag.compID = (int)reader["Leave_ID"];
                            ViewBag.compName = (string)reader["Leave_Name"];
                        }
                        else if (reader["Leave_Name"].Equals("Short_Hours"))
                        {
                            ViewBag.shortDuration = (int)reader["Duration"];
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
                UpdateBalance(mode.maternityID, mode.maternityID);
                UpdateBalance(mode.shortID, mode.shortLeaveHours);
                Response.Write("<script> alert('Sucess. The Information has been updated.');location.href='Display'</script>");
            }
            else
            {
                ModelState.AddModelError("errmsg", "Failed: An Error Occured. Please Check your input and try again");
            }
            return Display();
        }
        public void UpdateBalance(int id, int duration)
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