using System.Web.Mvc;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class HomeController : ControllerBase
    {
        // GET: Home
        public ActionResult Index()
        {
            sEmployeeModel emp = GetEmployeeModel(GetLoggedInID());
            Session["UserName"] = emp.firstName + " " + emp.lastName;
            return View();
        }
    }
}