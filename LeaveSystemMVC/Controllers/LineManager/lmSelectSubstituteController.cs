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
    public class lmSelectSubstituteController : ControllerBase
    {
        // GET: lmSelectSubstitute
        public ActionResult Index()
        {
            // gets everything from the Reporting_Map table
            List<lmReporting> reportingList = GetReportingList();

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

            SetMessageViewBags();
            ViewData["SelectableSubstitute"] = selectableSubstitutes;
            ViewData["EmployeeNames"] = DBEmployeeList();
            ViewData["LoggedID"] = GetLoggedInID();

            return View(GetReportingList(loggedInID));
        }

        [HttpPost]
        public ActionResult Promote(int reportToID, int subLevel, FormCollection form)
        {
            int selectedSubID = Convert.ToInt32(form[reportToID.ToString()]);
            int lmRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "LM").Key;
            int loggedInID = GetLoggedInID();

            string queryString;

            // gives the substitute employee a new LM role
            queryString = "INSERT INTO dbo.Employee_Role(Employee_ID, Role_ID) VALUES('" + selectedSubID + "', '" + lmRoleID + "')";
            DBExecuteQuery(queryString);

            // updates the highest substitution level to in-active
            queryString = "UPDATE dbo.Reporting_Map SET Is_Active = 'False' WHERE Original_ID = '" + reportToID + "' AND Substitution_Level = '" + subLevel + "'";
            DBExecuteQuery(queryString);

            // adds a new record to the Reporting_Map with the selected substitute as active 
            queryString = "INSERT INTO dbo.Reporting_Map(Original_ID, From_ID, To_ID, Substitution_Level, Is_Active) VALUES('" + reportToID + "', '" + loggedInID + "', '" + selectedSubID + "', '" + ((subLevel != 0) ? (subLevel+1) : 1) + "', 'True')";
            DBExecuteQuery(queryString);

            var names = DBEmployeeList();
            TempData["SuccessMessage"] = "<b>" + names[selectedSubID] + "</b> is the new substitute for <b>" + names[reportToID] + "</b>'s subordinates.";

            return RedirectToAction("Index");
        }

        public ActionResult Demote(int reportToID, int subLevel)
        {
            string queryString;
            // note that the Reporting_Map substitution level starts from null (no substitute selected).
            // so if the original LM retrieves his/her permissions, it needs to demote all levels greater or equal to 1
            // else if a substitute is retrieving the permission from a sub-substitute, then it needs to demote all levels greater than his/her level
            string comparison = (reportToID == GetLoggedInID()) ? ">=" : ">";

            // demoting employees
            queryString = "SELECT To_ID FROM dbo.Reporting_Map WHERE Original_ID = '" + reportToID + "' AND Substitution_Level " + comparison + " '" + subLevel + "'";
            DemoteRole(queryString);

            // removing the records from the Reporting_Map
            queryString = "DELETE FROM dbo.Reporting_Map WHERE Original_ID = '" + reportToID + "' AND Substitution_Level " + comparison + " '" + subLevel + "'";
            DBExecuteQuery(queryString);

            // updating the highest substitution level in-active to active
            queryString = "UPDATE dbo.Reporting_Map SET Is_Active = 'True' WHERE Original_ID = '" + reportToID + "' AND Substitution_Level = '" + subLevel + "'";
            DBExecuteQuery(queryString);

            var names = DBEmployeeList();
            TempData["SuccessMessage"] = "Your line manager role has been successfully returned to you.";

            return RedirectToAction("Index");
        }

        private void DemoteRole(string queryString)
        {
            // holds the list of employes to be demoted (can be in a chain)
            List<int> demotionList = new List<int>();
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        demotionList.Add((int)reader["To_ID"]);
                    }
                }
                connection.Close();
            }

            // removes the LM role for each employee in the list
            foreach (int empID in demotionList)
            {
                queryString = "DELETE FROM dbo.Employee_Role WHERE Emp_Role_ID = '" + GetEmpRoleID(empID) + "'";
                DBExecuteQuery(queryString);
            }
        }

        private int GetEmpRoleID(int empID)
        {
            int lmRoleID = DBRoleList().FirstOrDefault(obj => obj.Value == "LM").Key;
            string queryString = "SELECT MAX(Emp_Role_ID) AS ERoleID FROM dbo.Employee_Role WHERE Employee_ID = '" + empID + "' AND Role_ID = '" + lmRoleID + "'";
            int empRoleID = 0;

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        empRoleID = (int)reader["ERoleID"];
                    }
                }
                connection.Close();
            }

            return empRoleID;
        }

        private Dictionary<int, string> GetSelectableEmployees(int reportToID)
        {
            List<sEmployeeModel> allEmployees = GetEmployeeModel();
            Dictionary<int, string> selectableEmp = new Dictionary<int, string>();

            foreach (sEmployeeModel emp in allEmployees)
            {
                // is this employee a line manager or substitute line manager? and not the original line manager of the subordinates
                if ((emp.reportsToLineManagerID == reportToID || emp.isLM) && emp.staffID != reportToID)
                    selectableEmp.Add((int)emp.staffID, emp.firstName + " " + emp.lastName);
            }

            return selectableEmp;
        }
    }
}
