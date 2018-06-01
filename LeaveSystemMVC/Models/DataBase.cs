using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Smo.Agent;

namespace LeaveSystemMVC.Models
{
    public class DataBase
    {
        public string connectionString { get; set; }
        public string databaseName { get; set; }

        public DataBase()
        {
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            databaseName = new SqlConnectionStringBuilder(connectionString).InitialCatalog.ToString();
        }

        public bool Execute(SqlCommand sqlCommand)
        {
            bool success = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    sqlCommand.Connection = conn;
                    conn.Open();
                    using (sqlCommand.ExecuteReader()) { }
                    conn.Close();
                    success = true;
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }

            return success;
        }

        public DataTable Fetch(SqlCommand sqlCommand)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    sqlCommand.Connection = conn;
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = sqlCommand;
                    adapter.Fill(dataTable);
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }

            return dataTable;
        }

        public bool Contains(SqlCommand sqlCommand)
        {
            bool isExist = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    sqlCommand.Connection = conn;

                    conn.Open();
                    isExist = ((int)sqlCommand.ExecuteScalar() > 0) ? true : false;
                    conn.Close();
                }
            }
            catch (ConfigurationErrorsException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }

            return isExist;
        }

        public bool BackupBAK(String fileName, String path)
        {
            bool isReady = false;
            try
            {
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    // Connect to the server and generate a backup
                    ServerConnection serverCon = new ServerConnection(sqlCon);
                    Server server = new Server(serverCon);
                    Backup backup = new Backup();
                    backup.Action = BackupActionType.Database;
                    backup.Database = databaseName;

                    // Perform back up from database to file
                    BackupDeviceItem destination = new BackupDeviceItem(path, DeviceType.File);
                    backup.Devices.Add(destination);
                    backup.SqlBackup(server);

                    // Close connections
                    serverCon.Disconnect();
                    sqlCon.Close();

                    isReady = true;
                }
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }

            return isReady;
        }

        public bool BackupSQL(String fileName, String path)
        {
            bool isReady = false;
            try
            {
                using (SqlConnection sqlCon = new SqlConnection(connectionString))
                {
                    // Connect to the server and generate a scripter
                    ServerConnection serverCon = new ServerConnection(sqlCon);
                    Server server = new Server(serverCon);
                    Database database = server.Databases[databaseName];
                    Scripter scripter = new Scripter(server)
                    {
                        Options = {
                            ScriptSchema = true,
                            ScriptData = true,
                            WithDependencies = true,
                            ScriptDrops = false,
                            AnsiPadding = false,
                            DriAll = true,
                            Statistics = true,
                            Triggers = true,
                            IncludeHeaders = true,
                            IncludeDatabaseContext = true,
                            NoCollation = true
                        }
                    };

                    // List of queries
                    List<string> lScripts = new List<string>();

                    // Query to create a Database
                    lScripts.Add("USE [master]");
                    lScripts.Add("CREATE DATABASE [" + databaseName + "]");
                    lScripts.Add("ALTER DATABASE [" + databaseName + "] SET COMPATIBILITY_LEVEL = 120");
                    lScripts.Add("GO");

                    // gather all tables and data from the database and store its queries in the list
                    foreach (Table table in database.Tables)
                    {
                        // Sfc.Urn Magic
                        if (!table.Schema.StartsWith("HangFire")) //exclude HangFire tables
                        {
                            foreach (string s in scripter.EnumScript(new Microsoft.SqlServer.Management.Sdk.Sfc.Urn[] { table.Urn }))
                            {
                                if (!lScripts.Contains(s))
                                    lScripts.Add(s);
                            }
                        }
                    }

                    // add the final query to the list for enabling read/write
                    lScripts.Add("USE [master]");
                    lScripts.Add("ALTER DATABASE [" + databaseName + "] SET READ_WRITE");

                    // Write the list to file
                    StreamWriter sWriter = File.CreateText(path);
                    foreach (string s in lScripts)
                    {
                        sWriter.Write(s + "\n");
                    }

                    // close file and connections
                    sWriter.Close();
                    serverCon.Disconnect();
                    sqlCon.Close();

                    isReady = true;
                }
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (SqlException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }

            return isReady;
        }

        public Dictionary<int, string> Listing(string table, string id, string name)
        {
            Dictionary<int, string> list = new Dictionary<int, string>();
            string queryString = "SELECT " + id + ", " + name + " FROM " + table + "";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(queryString, connection);
                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((int)reader[id], reader[name].ToString());
                    }
                }
            }

            return list;
        }

        public void AddEmploymentPeriodSchedule(int empID, DateTime date, bool status)
        {
            try
            {
                // Get instance of SQL Agent SMO object
                Server server = new Server(".");
                JobServer jobServer = server.JobServer;

                // Create a schedule, set it to be executed once at the specified date
                JobSchedule schedule = new JobSchedule(jobServer, "Schedule_For_" + empID);
                schedule.FrequencyTypes = FrequencyTypes.OneTime;
                schedule.ActiveStartDate = date;
                schedule.IsEnabled = true;
                schedule.Create();

                // Create Job and assign the schedule to it
                Job job = new Job(jobServer, "Account_Status_" + DateTime.Now);
                job.Create();
                job.AddSharedSchedule(schedule.ID);
                job.ApplyToTargetServer(server.Name);

                // Create Job Step to activate/de-activate an account and delete the Job once done
                JobStep step = new JobStep(job, status + "-" + empID);
                step.Command = "UPDATE dbo.Employee SET Account_Status = '" + status + "' WHERE Employee_ID = '" + empID + "'; " +
                    "USE msdb; EXEC sp_delete_job @job_name = N'" + job.Name + "';";
                step.SubSystem = AgentSubSystem.TransactSql;
                step.DatabaseName = "LeaveSystem";
                step.Create();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
        }
    }
}