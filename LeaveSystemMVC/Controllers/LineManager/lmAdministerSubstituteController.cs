using System;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class lmAdministerSubstituteController : BaseController
    {
        // GET: lmAdministerSubstitute
        public ActionResult Index()
        {
            // gets everything from the Reporting_Map table
            List<Reporting> reportingList = GetReportingList();

            // removes duplicates (caused by Employee_ID)
            var distinctList = reportingList.Select(m => new { m.reportToID, m.fromID, m.toID, m.isActive }).Distinct().ToList();

            // final selectable substitute (dropdown box)
            Dictionary<int, Dictionary<int, string>> selectableSubstitutes = new Dictionary<int, Dictionary<int, string>>();
            int loggedInID = GetLoggedInID();

            foreach (var item in distinctList)
            {
                // is this the original LM? and he/she is not substituting anyone?
                // if yes, then get all selectable employees for this user
                if (item.reportToID == loggedInID && item.fromID == null)
                    selectableSubstitutes.Add(item.reportToID, GetSelectableEmployees(loggedInID));

                // is this the latest substitute?
                if (item.toID == loggedInID && item.isActive == true)
                {
                    // get all selectable employees for the original LM
                    Dictionary<int, string> allSelectableSubstitutes = GetSelectableEmployees(item.reportToID);
                    Dictionary<int, string> finalSelectableSubstitutes = new Dictionary<int, string>();

                    foreach (var dict in allSelectableSubstitutes)
                    {
                        // is this employee id already in the Reporting_Map table (for this original_LM)?
                        // if not, then add him/her to the list
                        if (!distinctList.Select(m => m.toID).ToList().Contains(dict.Key))
                            finalSelectableSubstitutes.Add(dict.Key, dict.Value);
                    }

                    selectableSubstitutes.Add(item.reportToID, finalSelectableSubstitutes);
                }
            }

            Dictionary dic = new Dictionary();
            SetMessageViewBags();
            ViewData["SelectableSubstitute"] = selectableSubstitutes;
            ViewData["EmployeeNames"] = dic.GetEmployee();
            ViewData["LoggedID"] = GetLoggedInID();

            return View(GetReportingList(loggedInID));
        }

        [HttpPost]
        public ActionResult Promote(int reportToID, int subLevel, FormCollection form)
        {
            DataBase db = new DataBase();
            SqlCommand cmd = new SqlCommand();
            Dictionary dic = new Dictionary();
            int selectedSubID = Convert.ToInt32(form[reportToID.ToString()]);
            cmd.Parameters.Add("@selectedSubID", SqlDbType.Int).Value = selectedSubID;
            cmd.Parameters.Add("@lmRoleID", SqlDbType.Int).Value = dic.GetRole().FirstOrDefault(obj => obj.Value == "LM").Key;
            cmd.Parameters.Add("@loggedInID", SqlDbType.Int).Value = GetLoggedInID();
            cmd.Parameters.Add("@subLevel", SqlDbType.Int).Value = subLevel;
            cmd.Parameters.Add("@subLevelPlus", SqlDbType.Int).Value = ((subLevel != 0) ? (subLevel + 1) : 1);
            cmd.Parameters.Add("@reportToID", SqlDbType.Int).Value = reportToID;

            // gives the substitute employee a new LM role
            cmd.CommandText = "INSERT INTO dbo.Employee_Role(Employee_ID, Role_ID) VALUES(@selectedSubID, @lmRoleID)";
            db.Execute(cmd);

            // updates the highest substitution level to in-active
            cmd.CommandText = "UPDATE dbo.Reporting_Map SET Is_Active = 'False' WHERE Original_ID = @lmRoleID AND Substitution_Level = @subLevel";
            db.Execute(cmd);

            // adds a new record to the Reporting_Map with the selected substitute as active             
            cmd.CommandText = "INSERT INTO dbo.Reporting_Map(Original_ID, From_ID, To_ID, Substitution_Level, Is_Active) VALUES(@reportToID, @loggedInID, @selectedSubID, @subLevelPlus, 'True')";
            db.Execute(cmd);

            var names = dic.GetEmployee();
            TempData["SuccessMessage"] = "<b>" + names[selectedSubID] + "</b> is the new substitute for <b>" + names[reportToID] + "</b>'s subordinates.";

            return RedirectToAction("Index");
        }

        public ActionResult Demote(int reportToID, int subLevel)
        {
            // note that the Reporting_Map substitution level starts from null (no substitute selected).
            // so if the original LM retrieves his/her permissions, it needs to demote all levels greater or equal to 1
            // else if a substitute is retrieving the permission from a sub-substitute, then it needs to demote all levels greater than his/her level
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@reportToID", SqlDbType.Int).Value = reportToID;
            cmd.Parameters.Add("@subLevel", SqlDbType.Int).Value = subLevel;
            string comparison = (reportToID == GetLoggedInID()) ? ">=" : ">";

            DataBase db = new DataBase();
            // demoting employees
            cmd.CommandText = "SELECT To_ID FROM dbo.Reporting_Map WHERE Original_ID = @reportToID AND Substitution_Level " + comparison + " @subLevel";
            db.Execute(cmd);

            // removing the records from the Reporting_Map
            cmd.CommandText = "DELETE FROM dbo.Reporting_Map WHERE Original_ID = @reportToID AND Substitution_Level " + comparison + " @subLevel";
            db.Execute(cmd);

            // updating the highest substitution level in-active to active
            cmd.CommandText = "UPDATE dbo.Reporting_Map SET Is_Active = 'True' WHERE Original_ID = @reportToID AND Substitution_Level = @subLevel";
            db.Execute(cmd);

            Dictionary dic = new Dictionary();
            var names = dic.GetEmployee();
            TempData["SuccessMessage"] = "Your line manager role has been successfully returned to you.";

            return RedirectToAction("Index");
        }

        private int GetEmpRoleID(int empID)
        {
            int empRoleID = 0;
            Dictionary dic = new Dictionary();
            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            cmd.Parameters.Add("@lmRoleID", SqlDbType.Int).Value = dic.GetRole().FirstOrDefault(obj => obj.Value == "LM").Key;

            cmd.CommandText = "SELECT MAX(Emp_Role_ID) AS ERoleID FROM dbo.Employee_Role WHERE Employee_ID = @empID AND Role_ID = @lmRoleID";

            DataBase db = new DataBase();
            DataTable dataTable = db.Fetch(cmd);

            foreach (DataRow row in dataTable.Rows)
            {
                empRoleID = (int)row["ERoleID"];
            }
           
            return empRoleID;
        }

        private Dictionary<int, string> GetSelectableEmployees(int reportToID)
        {
            List<Employee> allEmployees = GetEmployeeModel();
            Dictionary<int, string> selectableEmp = new Dictionary<int, string>();

            foreach (Employee emp in allEmployees)
            {
                // is this employee a line manager or substitute line manager? and not the original line manager of the subordinates
                if ((emp.reportsToLineManagerID == reportToID || emp.isLM) && emp.staffID != reportToID)
                    selectableEmp.Add((int)emp.staffID, emp.firstName + " " + emp.lastName);
            }

            return selectableEmp;
        }
    }
}
