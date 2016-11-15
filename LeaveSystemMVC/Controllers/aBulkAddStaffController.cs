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
                        var newEmployees = new List<employeeCons>();
                        foreach(string[] tableRow in tableRows)
                        {
                            int column = 0;
                            var newEmployee = new employeeCons();
                            bool addEmployee = true;
                            foreach (string tableColumn in tableRow)
                            {
                                
                                switch(column)
                                {
                                    case 0:
                                        int ID;
                                        int.TryParse(tableColumn, out ID);
                                        newEmployee.employeeObject.staffID = ID;
                                        
                                        break;
                                    case 1:
                                        newEmployee.employeeObject.firstName = tableColumn;
                                        break;
                                    case 2:
                                        newEmployee.employeeObject.lastName = tableColumn;
                                        break;
                                    case 3:
                                        newEmployee.employeeObject.designation = tableColumn;
                                        break;
                                    case 4:
                                        newEmployee.employeeObject.deptName = tableColumn;
                                        break;
                                    case 5:
                                        newEmployee.employeeObject.gender = tableColumn;
                                        break;
                                    case 6:
                                        /*
                                        DateTime startDate = DateTime.ParseExact(tableColumn, "MM/dd/yyyy",
                                            System.Globalization.CultureInfo.InvariantCulture);*/
                                        DateTime startDate = DateTime.Parse(tableColumn);
                                        newEmployee.employeeObject.empStartDate = startDate;
                                        break;
                                    case 7:
                                        newEmployee.employeeObject.email = tableColumn;
                                        break;
                                    case 8:
                                        newEmployee.roles.Add(tableColumn);
                                        break;
                                    case 9:
                                        newEmployee.roles.Add(tableColumn);
                                        break;
                                    case 10:
                                        newEmployee.roles.Add(tableColumn);
                                        break;

                                    case 11:
                                        newEmployee.employeeObject.phoneNo = tableColumn;
                                        break;
                                    case 12:
                                        int annual;
                                        int.TryParse(tableColumn, out annual);
                                        newEmployee.balances.annual = annual;
                                        break;
                                    case 13:
                                        int maternity;
                                        int.TryParse(tableColumn, out maternity);
                                        newEmployee.balances.maternity = maternity;
                                        break;
                                    case 14:
                                        int sick;
                                        int.TryParse(tableColumn, out sick);
                                        newEmployee.balances.sick = sick;
                                        break;
                                    case 15:
                                        int compassionate;
                                        int.TryParse(tableColumn, out compassionate);
                                        newEmployee.balances.compassionate = compassionate;
                                        break;
                                    case 16:
                                        int dil;
                                        int.TryParse(tableColumn, out dil);
                                        newEmployee.balances.daysInLieue = dil;
                                        break;
                                    case 17:
                                        int hours;
                                        int.TryParse(tableColumn, out hours);
                                        newEmployee.balances.shortLeaveHours = hours;
                                        break;

                                }
                                if (!addEmployee)
                                    break;
                                column++;
                            }
                            if(addEmployee)
                                newEmployees.Add(newEmployee);

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

    public class employeeCons
    {
        public sEmployeeModel employeeObject;
        public List<string> roles;
        public sleaveBalanceModel balances;

        public employeeCons()
        {
            employeeObject = new sEmployeeModel();
            roles = new List<string>();
            balances = new sleaveBalanceModel();
        }
    }

}