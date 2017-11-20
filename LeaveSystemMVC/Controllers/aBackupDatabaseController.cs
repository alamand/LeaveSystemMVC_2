/*
 * Author: Hamed Alhinai
 */
using System;
using System.Web.Mvc;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;


namespace LeaveSystemMVC.Controllers
{
    public class aBackupDatabaseController : Controller
    {
        // GET: aBackupDatabase
        public ActionResult Index()
        {
            // Copy paste the code in the bottom functions to try out.
            // soon will have it in UI and remove hard coded paths + exception checks.

            return View();
        }

        public static void BackupDatabase(string backupPath)
        {
            ServerConnection con = new ServerConnection(@"MSITOWER\SQLEXPRESS");
            Server server = new Server(con);
            Backup source = new Backup();
            source.Action = BackupActionType.Database;
            source.Database = "LeaveSystem";
            BackupDeviceItem destination = new BackupDeviceItem(@"D:\Desktop\LeaveSystem" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss") + ".bak", DeviceType.File);
            source.Devices.Add(destination);
            source.SqlBackup(server);
            con.Disconnect();
        }


        public static void RestoreDatabase(string restoreFile)
        {
            ServerConnection con = new ServerConnection(@"MSITOWER\SQLEXPRESS");
            Server server = new Server(con);
            Restore destination = new Restore();
            destination.Action = RestoreActionType.Database;
            destination.Database = "LeaveSystem";
            BackupDeviceItem source = new BackupDeviceItem(@"D:\Desktop\LeaveSystem.bak", DeviceType.File);
            destination.Devices.Add(source);
            destination.ReplaceDatabase = true;
            destination.SqlRestore(server);
        }
    }
}