using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class hrEditDefaultLeaveDurationController : BaseController
    {
        // GET: hrEditDefaultLeaveDuration
        [HttpGet]
        public ActionResult Index()
        {
            var model = GetLeaveBalanceModel();
            return View(model);
        }
        
        [HttpPost]
        public ActionResult Index(Balance model)
        {
            if (ModelState.IsValid)
            {
                UpdateBalance(model.annualID, model.annual);
                UpdateBalance(model.compassionateID, model.compassionate);                
                UpdateBalance(model.maternityID, model.maternity);
                UpdateBalance(model.sickID, model.sick);
                UpdateBalance(model.shortHoursID, model.shortHours);
                UpdateBalance(model.pilgrimageID, model.pilgrimage);
                ViewBag.SuccessMessage = "The information has been updated successfully.";
            }
            else
            {
                ViewBag.WarningMessage = "An error occured, please check your input and try again.";
            }
            return Index();
        }

        public void UpdateBalance(int typeID, decimal duration)
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@typeID", SqlDbType.Int).Value = typeID;
            cmd.Parameters.Add("@duration", SqlDbType.Int).Value = duration;
            cmd.CommandText = "UPDATE dbo.Leave_Type SET Duration = @duration WHERE Leave_Type_ID = @typeID";
            DataBase db = new DataBase();
            db.Execute(cmd);
        }
    }
}