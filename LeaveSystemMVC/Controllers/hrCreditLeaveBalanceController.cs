using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class hrCreditLeaveBalanceController : Controller
    {
        // GET: hrCreditLeaveBalance
        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(int? value)
        {
            if (ModelState.IsValid)
            {
                var list = new List<Leave>();
                var driver = new List<Leave>();
                var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
                string queryString = "Select * from LeaveSystem.dbo.Leave_Type";
                using (var connection = new SqlConnection(connectionString))
                {
                    var command = new SqlCommand(queryString, connection);

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = (int)reader["Leave_ID"];
                            var name = (string)reader["Leave_Name"];
                            var duration = (int)reader["Duration"];
                            if (!name.Equals("Annual")) { list.Add(new Leave(id, name, duration)); }
                        }
                    }

                    connection.Close();
                }
                foreach (var item in list)
                {
                    CreditBalance(item.id, item.duration);
                }
            }
            CreditAnnual();
            Response.Write("<script> alert('Sucess. The Balance has been reset.');</script>");
            return View();
        }

    
    public struct Leave
    {
        public Leave(int a, string ab, int b) { id = a; name = ab; duration = b; }
        public int id { get; set; }
        public string name { get; set; }
        public int duration { get; set; }
    }
    public struct Emp
    {
        public Emp(int lbid, int eid, int lid, int pbal, int dur) { lb_ID = lbid; e_id = eid; l_id = lid; prevbalance = pbal; duration = dur; }
        public int lb_ID { get; set; }
        public int e_id { get; set; }
        public int l_id { get; set; }
        public int prevbalance { get; set; }
        public int duration { get; set; }
    }
    public void CreditBalance(int leaveID, int balance)
    {
        var model = new List<Models.hrHolidaysCalender>();
        var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        //var connectionString = ConfigurationManager.ConnectionStrings["CustomConnection"].ConnectionString;
        string queryString = "Update dbo.Leave_Balance SET Balance='" + balance + "' WHERE Leave_ID='" + leaveID + "'";
        using (var connection = new SqlConnection(connectionString))
        {
            var command = new SqlCommand(queryString, connection);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
            }

            connection.Close();
        }

    }
    public bool isDriver(int id)
    {
        string desig = "";
        var driverID = getDriverID();
        var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        string queryString = "Select Designation From dbo.Employee Where Employee_ID='" + id + "'";
        using (var connection = new SqlConnection(connectionString))
        {
            var command = new SqlCommand(queryString, connection);
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) { desig = (string)reader["Designation"]; }
            }
            connection.Close();
        }
        if (desig.Equals("Driver")) { return true; }
        else { return false; }

    }
    public int getDriverID()
    {
        int id = 0;
        var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        string queryString = "Select Employee_ID From dbo.Employee Where Designation ='Driver'";
        using (var connection = new SqlConnection(connectionString))
        {
            var command = new SqlCommand(queryString, connection);
            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read()) { id = (int)reader["Employee_ID"]; }
            }
            connection.Close();
        }

        return id;
    }
    public void CreditAnnual()
    {
        int annualid = getAnnualID();
        int duration = getAnnualDuration(annualid);
        var employee = new List<Emp>();
        var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        string queryString = "Select * from LeaveSystem.dbo.Leave_Balance Where Leave_ID=" + annualid;
        using (var connection = new SqlConnection(connectionString))
        {

            var command = new SqlCommand(queryString, connection);

            connection.Open();
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var lbid = (int)reader["Leave_Balance_ID"];
                    var empid = (int)reader["Employee_ID"];
                    var lid = (int)reader["Leave_ID"];
                    var prevbal = (int)reader["Balance"];
                    employee.Add(new Emp(lbid, empid, lid, prevbal, duration));
                }
            }
            connection.Close();
            foreach (var item in employee)
            {
                var balance = item.prevbalance + item.duration;
                CreditAnnual(item.l_id, item.e_id, balance);
            }
        }

    }
    public void CreditAnnual(int lid, int eid, int balance)
    {
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryUpdate = "Update dbo.Leave_Balance SET Balance='" + balance + "' WHERE Leave_ID='" + lid + "' AND  Employee_ID = '" + eid + "'";
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
    public int getAnnualDuration(int leave_ID)
        {
            int duration = 0;
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select * From dbo.Leave_Type Where Leave_ID ='" + leave_ID + "'";
            using (var connection = new SqlConnection(connectionString))
            {

                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        duration = ((int)reader["Duration"]);

                    }
                }

                connection.Close();
            }
            return duration;
        }
    public int getAnnualID()
    {
            var annualID = 0;

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "Select * from LeaveSystem.dbo.Leave_Type";
            using (var connection = new SqlConnection(connectionString))
            {

                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = (int)reader["Leave_ID"];
                        var name = (string)reader["Leave_Name"];
                        var duration = (int)reader["Duration"];
                        if (name.Equals("Annual")) { annualID = id; }
                        
                    }
                }

                connection.Close();
            }
            return annualID;
        }
}
}