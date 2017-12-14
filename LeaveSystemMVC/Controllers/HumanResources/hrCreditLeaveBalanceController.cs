using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;


namespace LeaveSystemMVC.Controllers
{
    public class hrCreditLeaveBalanceController : ControllerBase
    {
        // GET: hrCreditLeaveBalance
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Credit()
        {
            var lb = GetLeaveBalanceModel();
            if (ModelState.IsValid)
            {
                CreditBalance(lb.compassionateID, lb.compassionate);
                CreditBalance(lb.maternityID, lb.maternity);
                CreditBalance(lb.shortHoursID, lb.shortHours);
                CreditBalance(lb.sickID, lb.sick);
                CreditBalance(lb.unpaidID, lb.unpaid);
                CreditAnnual(lb.annualID, lb.annual);
                //CreditPilgrimage();

                Response.Write("<script> alert('Success. The balance has been reset.');</script>");
            }

            return View();
        }

        public void CreditBalance(int leaveID, decimal duration)
        {
            string queryString = "UPDATE dbo.Leave_Balance SET Balance='" + duration + "' WHERE Leave_ID='" + leaveID + "'";
            DBExecuteQuery(queryString);
        }

        public void CreditAnnual(int leaveID, decimal duration)
        {
            var empList = new List<sleaveBalanceModel>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            string queryString = "SELECT Employee_ID, Balance FROM dbo.Leave_Balance WHERE Leave_ID = " + leaveID;

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var empID = (int)reader["Employee_ID"];
                        var balance = (decimal)reader["Balance"];
                        empList.Add(new sleaveBalanceModel{ empId = empID, annual = balance });
                    }
                }
                connection.Close();

                foreach (var item in empList)
                {
                    string queryUpdate = "UPDATE dbo.Leave_Balance SET Balance = '" + duration + item.annual + "' WHERE Leave_ID='" + leaveID + "' AND  Employee_ID = '" + item.empId + "'";
                    DBExecuteQuery(queryUpdate);
                }
            }
        }
        
    }
}