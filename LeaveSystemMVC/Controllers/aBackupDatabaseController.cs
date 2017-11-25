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
    public class aBackupDatabaseController : Controller
    {
        // GET: aBackupDatabase
        public ActionResult Index()
        {
            // TODO: Add a dropdown to select a database
            using (SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                ServerConnection serverCon = new ServerConnection(sqlCon);
                Server server = new Server(serverCon);
                List<string> dbList = new List<string>();
                foreach (Database db in server.Databases)
                {
                    dbList.Add(db.Name);
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult DownloadBAK()
        {
            try
            {
                deleteFiles("bak");             // delete all bak files in App_Data
                string dbName = "LeaveSystem";  // change this when appropriate.

                // Connect to the server and generate a backup
                SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
                ServerConnection serverCon = new ServerConnection(sqlCon);
                Server server = new Server(serverCon);
                Backup backup = new Backup();
                backup.Action = BackupActionType.Database;
                backup.Database = "LeaveSystem";

                // Set the full path file name
                string xFileName = dbName + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".bak";
                string dlFile = Server.MapPath("~/App_Data") + "/" + xFileName;

                // Perform back up from database to file
                BackupDeviceItem destination = new BackupDeviceItem(dlFile, DeviceType.File);
                backup.Devices.Add(destination);
                backup.SqlBackup(server);
                serverCon.Disconnect();
                sqlCon.Close();

                return File(dlFile, "application/force-download", xFileName);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DownloadSQL()
        {
            try
            {
                deleteFiles("sql");             // delete all sql files in App_Data
                string dbName = "LeaveSystem";  // change this when appropriate.
                
                using (SqlConnection sqlCon = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
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
                            Triggers = true
                        }
                    };

                    List<string> lScripts = new List<string>();

                    lScripts.Add("USE [master]");
                    lScripts.Add("GO");
                    lScripts.Add("CREATE DATABASE [" + dbName + "]");
                    lScripts.Add("GO");
                    lScripts.Add("ALTER DATABASE [" + dbName + "] SET COMPATIBILITY_LEVEL = 120");

                    // gather all tables and data from the database and store it in the list
                    foreach (Table table in database.Tables)
                    {
                        foreach (string s in scripter.EnumScript(new Microsoft.SqlServer.Management.Sdk.Sfc.Urn[] { table.Urn }))
                        {
                            if (!lScripts.Contains(s))
                            {
                                lScripts.Add("GO");
                                lScripts.Add(s);
                            }
                        }
                    }

                    lScripts.Add("GO");
                    lScripts.Add("USE [master]");
                    lScripts.Add("GO");
                    lScripts.Add("ALTER DATABASE [" + dbName + "] SET READ_WRITE");
                    lScripts.Add("GO");

                    // Set the full path file name
                    string xFileName = dbName + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".sql";
                    string dlFile = Server.MapPath("~/App_Data") + "/" + xFileName;

                    // Write to file
                    StreamWriter sWriter = System.IO.File.CreateText(dlFile);
                    foreach(string s in lScripts)
                    {
                        sWriter.Write(s + "\n");
                    }

                    sWriter.Close();
                    serverCon.Disconnect();
                    sqlCon.Close();

                    return File(dlFile, "application/force-download", xFileName);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }

            return RedirectToAction("Index");
        }

        private void deleteFiles(string ext)
        {
            string dDirectory = Server.MapPath("~/App_Data") + "\\";

            // Delete previously generated backups
            string[] fileList = Directory.GetFiles(dDirectory, "*." + ext);
            foreach (string file in fileList)
            {
                System.IO.File.Delete(file);
            }
        }



        /*
        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase upload)
        {
            if (upload != null && upload.ContentLength > 0)
            {
                if (upload.FileName.EndsWith(".bak"))
                {
                    try
                    {
                        // save the file on the server
                        upload.SaveAs(Path.Combine(Server.MapPath("~/App_Data"), upload.FileName));

                        // create a path to the file to send to the service
                        string dbPath = Path.Combine(Server.MapPath("~/App_Data"), Path.GetFileName(upload.FileName));

                        // Connect to the server and generate a restore
                        ServerConnection con = new ServerConnection(@"MSITOWER\SQLEXPRESS");
                        Server server = new Server(con);
                        Restore destination = new Restore();
                        destination.Action = RestoreActionType.Database;
                        destination.Database = "LeaveSystem";

                        // Perform restore from file to database
                        BackupDeviceItem source = new BackupDeviceItem(dbPath, DeviceType.File);
                        destination.Devices.Add(source);
                        destination.ReplaceDatabase = true;
                        destination.SqlRestore(server);
                        con.Disconnect();
                        
                        // delete the file on the server
                        FileInfo file = new FileInfo(dbPath);
                        file.Delete();

                        Response.Write("<script> alert ('Restore operation succeeded.')</script>");
                    }
                    catch (Exception ex)    // Need to handle other file types.
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }
                } 
                else if (upload.FileName.EndsWith(".sql"))
                {
                    try
                    {
                        // save the file on the server
                        upload.SaveAs(Path.Combine(Server.MapPath("~/App_Data"), upload.FileName));

                        // create a path to the file to send to the service
                        string dbPath = Path.Combine(Server.MapPath("~/App_Data"), Path.GetFileName(upload.FileName));

                        // Perform restore from file to database
                        ServerConnection con = new ServerConnection(@"MSITOWER\SQLEXPRESS");
                        string script = System.IO.File.ReadAllText(dbPath);
                        Server server = new Server(con);
                        server.ConnectionContext.ExecuteNonQuery(script);

                        // delete the file on the server
                        FileInfo file = new FileInfo(dbPath);
                        file.Delete();

                        Response.Write("<script> alert ('Restore operation succeeded.')</script>");
                    }
                    catch (Exception ex)    // Need to handle other file types.
                    {
                        ViewBag.Message = "ERROR:" + ex.Message.ToString();
                    }
                }
                else
                {
                    ViewBag.Message = "File type not supported, restore using .bak or .sql.";
                    ModelState.AddModelError("Upload", "The file type is not supported");
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file";
            }

            return Index();
        }
        */
    }

}