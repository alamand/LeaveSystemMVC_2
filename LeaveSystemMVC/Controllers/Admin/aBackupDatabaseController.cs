using System;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace LeaveSystemMVC.Controllers
{
    public class aBackupDatabaseController : ControllerBase
    {
        // GET: aBackupDatabase
        public ActionResult Index()
        {
            ViewBag.InfoMessage = "Please note that this might take a while to generate.";
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View();
        }

        [HttpPost]
        public ActionResult DownloadBAK()
        {
            try
            {
                // Delete all bak files in App_Data
                deleteFiles("bak");            

                var conString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (SqlConnection sqlCon = new SqlConnection(conString))
                {
                    // Get Database Name
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(conString);
                    string dbName = builder.InitialCatalog.ToString();
                    
                    // Connect to the server and generate a backup
                    ServerConnection serverCon = new ServerConnection(sqlCon);
                    Server server = new Server(serverCon);
                    Backup backup = new Backup();
                    backup.Action = BackupActionType.Database;
                    backup.Database = dbName;

                    // Set the full path file name
                    string xFileName = dbName + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".bak";
                    string dlFile = Server.MapPath("~/App_Data/DB_Backup") + "/" + xFileName;

                    // Perform back up from database to file
                    BackupDeviceItem destination = new BackupDeviceItem(dlFile, DeviceType.File);
                    backup.Devices.Add(destination);
                    backup.SqlBackup(server);

                    // Close connections
                    serverCon.Disconnect();
                    sqlCon.Close();

                    return File(dlFile, "application/force-download", xFileName);
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "ERROR:" + ex.Message.ToString();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DownloadSQL()
        {
            try
            {
                // Delete all sql files in App_Data
                deleteFiles("sql"); 
                
                var conString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                using (SqlConnection sqlCon = new SqlConnection(conString))
                {
                    // Get Database Name
                    SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(conString);
                    string dbName = builder.InitialCatalog.ToString();

                    // Connect to the server and generate a scripter
                    ServerConnection serverCon = new ServerConnection(sqlCon);
                    Server server = new Server(serverCon);
                    Database database = server.Databases[dbName];
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
                    lScripts.Add("CREATE DATABASE [" + dbName + "]");
                    lScripts.Add("ALTER DATABASE [" + dbName + "] SET COMPATIBILITY_LEVEL = 120");
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
                                {
                                    lScripts.Add(s);
                                }
                            }
                        }
                    }

                    // add the final query to the list for enabling read/write
                    lScripts.Add("USE [master]");
                    lScripts.Add("ALTER DATABASE [" + dbName + "] SET READ_WRITE");

                    // Set the full path file name
                    string xFileName = dbName + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".sql";
                    string dlFile = Server.MapPath("~/App_Data/DB_Backup") + "/" + xFileName;

                    // Write the list to file
                    StreamWriter sWriter = System.IO.File.CreateText(dlFile);
                    foreach(string s in lScripts)
                    {
                        sWriter.Write(s + "\n");
                    }

                    // close file and connections
                    sWriter.Close();
                    serverCon.Disconnect();
                    sqlCon.Close();

                    return File(dlFile, "application/force-download", xFileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "ERROR:" + ex.Message.ToString();
            }

            return RedirectToAction("Index");
        }

        private void deleteFiles(string ext)
        {
            string dDirectory = Server.MapPath("~/App_Data/DB_Backup") + "\\";

            // Delete previously generated backups
            string[] fileList = Directory.GetFiles(dDirectory, "*." + ext);
            foreach (string file in fileList)
            {
                System.IO.File.Delete(file);
            }
        }
    }
}