using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Collections.Generic;

namespace LeaveSystemMVC.Models
{
    public class Dictionary
    {
        private DataBase database;

        public Dictionary()
        {
            database = new DataBase();
        }

        public Dictionary<int, string> GetDictionary(string table, string id, string name)
        {
            database = new DataBase();
            return database.Listing(table, id, name);
        }

        public Dictionary<int, string> GetEmployee(int accountStatus = -1)
        {
            Dictionary<int, string> list = new Dictionary<int, string>();

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT Employee_ID, First_Name, Last_Name FROM dbo.Employee";

            if (accountStatus == 1 || accountStatus == 0)
            {
                cmd.Parameters.Add("@accountStatus", SqlDbType.Int).Value = accountStatus;
                cmd.CommandText += " WHERE Account_Status = " + accountStatus;
            }

            DataTable dataTable = database.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                string fullName = (string)row["First_Name"] + " " + (string)row["Last_Name"];
                list.Add((int)row["Employee_ID"], fullName);
            }

            return list;
        }

        public Dictionary<int, string> GetStaff(int accountStatus = -1)
        {
            Dictionary<int, string> list = new Dictionary<int, string>();

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@roleID", SqlDbType.Int).Value = GetRole().FirstOrDefault(obj => obj.Value == "Staff").Key;
            cmd.CommandText = "SELECT Employee.Employee_ID, First_Name, Last_Name " +
                "FROM dbo.Employee, dbo.Employee_Role " +
                "WHERE Employee.Employee_ID = Employee_Role.Employee_ID AND Employee_Role.Role_ID = @roleID";

            if (accountStatus == 1 || accountStatus == 0)
            {
                cmd.Parameters.Add("@accountStatus", SqlDbType.Int).Value = accountStatus;
                cmd.CommandText += " AND Employee.Account_Status = @accountStatus";
            }

            DataTable dataTable = database.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                string fullName = (string)row["First_Name"] + " " + (string)row["Last_Name"];
                list.Add((int)row["Employee_ID"], fullName);
            }

            return list;
        }

        public Dictionary<int, string> GetLeaveStatus()
        {
            return database.Listing("dbo.Leave_Status", "Leave_Status_ID", "Status_Name");
        }

        public Dictionary<int, string> GetLeaveStatusName()
        {
            return database.Listing("dbo.Leave_Status", "Leave_Status_ID", "Display_Name");
        }

        public Dictionary<int, string> GetLeaveType()
        {
            return database.Listing("dbo.Leave_Type", "Leave_Type_ID", "Leave_Name");
        }

        public Dictionary<int, string> GetLeaveTypeName()
        {
            return database.Listing("dbo.Leave_Type", "Leave_Type_ID", "Display_Name");
        }

        public Dictionary<int, string> GetNationality()
        {
            return database.Listing("dbo.Nationality", "Nationality_ID", "Nationality_Name");
        }

        public Dictionary<int, string> GetNationalityName()
        {
            return database.Listing("dbo.Nationality", "Nationality_ID", "Display_Name");
        }

        public Dictionary<int, string> GetDepartment()
        {
            return database.Listing("dbo.Department", "Department_ID", "Department_Name");
        }

        public Dictionary<int, string> GetDepartmentName()
        {
            return database.Listing("dbo.Department", "Department_ID", "Display_Name");
        }

        public Dictionary<int, string> GetRole()
        {
            return database.Listing("dbo.Role", "Role_ID", "Role_Name");
        }

        public Dictionary<int, string> GetRoleName()
        {
            return database.Listing("dbo.Role", "Role_ID", "Display_Name");
        }

        public Dictionary<int, string> GetReligion()
        {
            return database.Listing("dbo.Religion", "Religion_ID", "Religion_Name");
        }

        public Dictionary<int, string> GetReligionName()
        {
            return database.Listing("dbo.Religion", "Religion_ID", "Display_Name");
        }

        public Dictionary<int, string> GetLineManager()
        {
            Dictionary<int, string> list = new Dictionary<int, string>();

            SqlCommand cmd = new SqlCommand();
            cmd.Parameters.Add("@roleID", SqlDbType.Int).Value = GetRole().FirstOrDefault(obj => obj.Value == "LM").Key;
            cmd.CommandText = "SELECT Employee.Employee_ID, First_Name, Last_Name " +
                "FROM dbo.Employee, dbo.Employee_Role " +
                "WHERE Employee.Employee_ID = Employee_Role.Employee_ID AND Employee_Role.Role_ID = @roleID";

            DataTable dataTable = database.Fetch(cmd);
            foreach (DataRow row in dataTable.Rows)
            {
                string fullName = (string)row["First_Name"] + " " + (string)row["Last_Name"];
                if (!list.ContainsKey((int)row["Employee_ID"]))
                    list.Add((int)row["Employee_ID"], fullName);
            }

            return list;
        }

        public Dictionary<int, string> AddDefaultToDictionary(Dictionary<int, string> dictionary, int key, string value)
        {
            Dictionary<int, string> newDictionary = new Dictionary<int, string>();
            newDictionary.Add(key, value);
            foreach (KeyValuePair<int, string> entry in dictionary)
            {
                newDictionary.Add(entry.Key, entry.Value);
            }

            return newDictionary;
        }
    }
}