using System;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace LeaveSystemMVC.Controllers
{
    public class aBackupDatabaseController : Controller
    {
        // GET: aBackupDatabase
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DownloadBAK()
        {
            try
            {
                string dDirectory = Server.MapPath("~/App_Data") + "\\";

                // Delete previously generated backups
                string[] fileList = Directory.GetFiles(dDirectory, "*.BAK");
                foreach (string file in fileList)
                {
                    System.IO.File.Delete(file);
                }

                string defaultCon = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                string dataSource = defaultCon.Split('=', ';')[1];      // Assure that Data Source is at first config in connectionString

                // Connect to the server and generate a backup
                ServerConnection con = new ServerConnection(dataSource);
                Server server = new Server(con);
                Backup source = new Backup();
                source.Action = BackupActionType.Database;
                source.Database = "LeaveSystem";

                // Set the full path file name
                string xFileName = "LeaveSystem" + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".bak";
                string dlFile = Server.MapPath("~/App_Data") + "/" + xFileName;

                // Perform back up from database to file
                BackupDeviceItem destination = new BackupDeviceItem(dlFile, DeviceType.File);
                source.Devices.Add(destination);
                source.SqlBackup(server);
                con.Disconnect();

                return File(dlFile, "application/force-download", xFileName);
            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }

            return Index();
        }

        [HttpPost]
        public ActionResult DownloadSQL()
        {
            try
            {
                string dDirectory = Server.MapPath("~/App_Data") + "\\";

                string[] fileList = Directory.GetFiles(dDirectory, "*.SQL");
                foreach (string file in fileList)
                {
                    System.IO.File.Delete(file);
                }

            }
            catch (Exception ex)
            {
                ViewBag.Message = "ERROR:" + ex.Message.ToString();
            }

            return Index();
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