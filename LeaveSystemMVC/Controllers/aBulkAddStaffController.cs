using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.IO;
using LeaveSystemMVC.CustomLibraries;

namespace LeaveSystemMVC.Controllers
{
    public class aBulkAddStaffController : Controller
    {
        sEmployeeModel tempEmp = new sEmployeeModel();
        [HttpGet]
        // GET: aBulkAddStaff
        public ActionResult Index()
        {
            ViewBag.Message = "File name here...";
            tempEmp.firstName = "File name here...";
            
            return View(tempEmp);
        }
        
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase upload)
        {
            if(upload != null && upload.ContentLength > 0)
            {
                if(upload.FileName.EndsWith(".csv"))
                {
                    try
                    {
                        string path = Path.Combine(Server.MapPath("~/App_Data"), Path.GetFileName(upload.FileName));
                        upload.SaveAs(path);
                        CsvParser csvParser = new CsvParser(path);
                        csvParser.readFile();
                        ViewBag.Message = "File uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }
                }
                else
                {
                    ViewBag.Message = "File type not supported";
                    ModelState.AddModelError("upload", "The file type is not supported");
                    return View();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file";
            }
            tempEmp.firstName = upload.FileName;
            return View(tempEmp);
        }
    }
}