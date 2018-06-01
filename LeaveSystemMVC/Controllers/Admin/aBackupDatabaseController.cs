using System;
using System.IO;
using System.Web.Mvc;
using System.Diagnostics;
using LeaveSystemMVC.Models;

namespace LeaveSystemMVC.Controllers
{
    public class aBackupDatabaseController : BaseController
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
            // Delete all bak files in App_Data
            DeleteFiles("bak");
            DataBase db = new DataBase();

            // Set the full path file name
            string fileName = db.databaseName + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".bak";
            string path = Server.MapPath("~/App_Data/DB_Backup/") + fileName;

            bool isReady = db.BackupBAK(fileName, path);
            if (isReady)
            {
                return File(path, "application/force-download", fileName);
            }
            else
            {
                TempData["ErrorMessage"] = "ERROR: Failed to generate or download file.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult DownloadSQL()
        {
            // Delete all sql files in App_Data
            DeleteFiles("sql");
            DataBase db = new DataBase();

            // Set the full path file name
            string fileName = db.databaseName + DateTime.Now.ToString("yyyyMMdd-hhmmss") + ".sql";
            string path = Server.MapPath("~/App_Data/DB_Backup/") + fileName;

            bool isReady = db.BackupSQL(fileName, path);

            if (isReady)
            {
                return File(path, "application/force-download", fileName);
            }
            else
            {
                TempData["ErrorMessage"] = "ERROR: Failed to generate or download file.";
                return RedirectToAction("Index");
            }
        }

        private void DeleteFiles(string ext)
        {
            try
            {
                string dDirectory = Server.MapPath("~/App_Data/DB_Backup") + "\\";

                // Delete previously generated backups
                string[] fileList = Directory.GetFiles(dDirectory, "*." + ext);
                foreach (string file in fileList)
                {
                    System.IO.File.Delete(file);
                }
            }
            catch (NotImplementedException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (ArgumentException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (NotSupportedException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());            // @TODO: audit exception info
            }



        }
    }
}