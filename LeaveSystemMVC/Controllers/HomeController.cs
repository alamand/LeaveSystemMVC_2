using System.Web.Mvc;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            Employee emp = GetEmployeeModel(GetLoggedInID());
            Session["UserName"] = emp.firstName + " " + emp.lastName;
            return View();
        }
    }
}