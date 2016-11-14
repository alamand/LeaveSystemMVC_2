using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeaveSystemMVC.Models;
using System.IO;
using LeaveSystemMVC.CustomLibraries;
using System.Globalization;

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
                        CsvFile csv = new CsvFile(Path.Combine(Server.MapPath("~/App_Data"), Path.GetFileName(upload.FileName)));
                        upload.SaveAs(csv.path);
                        var tableRows = csv.readFile();
                        var newEmployees = new List<sEmployeeModel>();
                        foreach(string[] tableRow in tableRows)
                        {
                            int column = 0;
                            foreach(string tableColumn in tableRow)
                            {
                                var newEmployee = new sEmployeeModel();
                                switch(column)
                                {
                                    case 0:
                                        int ID;
                                        int.TryParse(tableColumn, out ID);
                                        newEmployee.staffID = ID;
                                        break;
                                    case 1:
                                        newEmployee.firstName = tableColumn;
                                        break;
                                    case 2:
                                        newEmployee.lastName = tableColumn;
                                        break;
                                    case 3:
                                        newEmployee.designation = tableColumn;
                                        break;
                                    case 4:
                                        newEmployee.deptName = tableColumn;
                                        break;
                                    case 5:
                                        newEmployee.gender = tableColumn;
                                        break;
                                    case 6:
                                        /*
                                        DateTime startDate = DateTime.ParseExact(tableColumn, "MM/dd/yyyy",
                                            System.Globalization.CultureInfo.InvariantCulture);*/
                                        DateTime startDate = DateTime.Parse(tableColumn);
                                        newEmployee.empStartDate = startDate;
                                        break;
                                }
                                column++;
                            }
                            column = 0;
                        }
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