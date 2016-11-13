using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace LeaveSystemMVC.Controllers
{
    public class hrHolidaysCalenderController : Controller
    {
        // GET: hrHolidaysCalender

        public ActionResult Index()
        {
            var calender = new LeaveSystemMVC.Models.hrHolidaysCalender();
            return View();
        }
        [HttpGet]
        public ActionResult CreateHoliday()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateHoliday([Bind(Exclude = "description")] Models.hrHolidaysCalender calender)
        {
            if (string.IsNullOrWhiteSpace(calender.holidayName))
            {
                ModelState.AddModelError("holidayName", "Holiday Name Must be entered");
            }
            else if (string.IsNullOrEmpty(calender.startDate.ToString()))
            {
                ModelState.AddModelError("startDate", "Start Date Must not be left empty");
            }
            else if (calender.startDate.Equals(DateTime.Today))
            {
                ModelState.AddModelError("startDate", "The Start Date cannot be today");
            }
            else if (calender.endDate.Equals(DateTime.Today))
            {
                ModelState.AddModelError("endDate", "The End Date cannot be today");
            }
            if (ModelState.IsValid)

            {
                
                var model = new List<Models.hrHolidaysCalender>();
                DateTime dt = DateTime.ParseExact(calender.startDate.ToString(), "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);
                string date = dt.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                string datee= "'" + date+"'";
                string datee2 = "'12/10/2016'";
                // string date = calender.startDate.Day+"/"+calender.startDate.Month+"/"+calender.startDate.Year;
                //string date=calender.startDate.ToString("dd-mm-yyyy");
                var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
                string insert = "INSERT INTO dbo.Public_Holiday (Name, Date)"
                 //+ "VALUES('"+calender.holidayName + "','" + date.ToString()/*s*/ + "') " ;
                 + "VALUES('" + calender.holidayName + "',"+datee2+")";
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(insert, connection);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    connection.Close();
                }


                return RedirectToAction("Index");
            }
            return CreateHoliday();
        }
        public ActionResult Display()
        {
            // DataTable dtTemplate= new SqlDataAdapter()

            /*string holidayName = "";
            DateTime startDate = DateTime.Today;
            DateTime endDate = DateTime.Today;
            string description = "";
            List<string> empRoles = new List<string>();
            */
            var model = new List<Models.hrHolidaysCalender>();
            var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
            string queryString = "Select * FROM dbo.Public_Holiday" ;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                
                connection.Open();
                 using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var calender = new Models.hrHolidaysCalender();
                        calender.holidayName = (string)reader["Name"];
                        calender.startDate = (DateTime)reader["Date"];
                        model.Add(calender);                        
                    }
                }
               
                connection.Close();
            }

            return View(model);
        }
        /*public ActionResult Index([Bind(Exclude = "description")] Models.hrHolidaysCalender calender)
        {
            return View();
        }*/

    }
}