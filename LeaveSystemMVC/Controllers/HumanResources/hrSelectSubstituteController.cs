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
    public class hrSelectSubstituteController : Controller
    {
        // GET: hrSelectSubstitute
        public ActionResult Index()
        {          
            return View();
        }
    }
}