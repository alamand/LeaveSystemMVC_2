using LeaveSystemMVC.Models;
using System.Web.Mvc;

namespace LeaveSystemMVC.Controllers
{
    public class hrEditDefaultLeaveDurationController : ControllerBase
    {
        // GET: hrEditDefaultLeaveDuration
        [HttpGet]
        public ActionResult Index()
        {
            var model = GetLeaveBalanceModel();
            return View(model);
        }
        
        [HttpPost]
        public ActionResult Index(sleaveBalanceModel model)
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
                ViewBag.WarningMessage = "Update was not successful, please check your input(s) and try again.";
            }
            return Index();
        }

        public void UpdateBalance(int id, decimal duration)
        {
            string queryUpdate = "UPDATE dbo.Leave_Type SET Duration='" + duration + "' WHERE Leave_Type_ID='" + id + "'";
            DBExecuteQuery(queryUpdate);
        }
    }

}