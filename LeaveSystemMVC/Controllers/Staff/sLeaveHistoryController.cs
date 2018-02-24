﻿using LeaveSystemMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Claims;
using System.IO;
using System.Net.Mail;
using System.Net;
using Hangfire;


namespace LeaveSystemMVC.Controllers
{
    public class sLeaveHistoryController : ControllerBase
    {
        // GET: sLeaveHistory
        public ActionResult Index()
        {
            var model = new List<sLeaveModel>();
            List<sLeaveModel> leaveList = GetLeaveModel("Employee.Employee_ID", GetLoggedInID());

            foreach (var leave in leaveList)
            {
                if (!leave.leaveStatusName.Equals("Pending_HR") && !leave.leaveStatusName.Equals("Pending_LM"))
                    model.Add(leave);
            }
            
        return View(model);
        }

        [HttpGet]
        public ActionResult View(int appID)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.WarningMessage = TempData["WarningMEssage"];
            ViewBag.ErrorMessage = TempData["ErrorMessage"];
            return View(GetLeaveModel("Leave_Application_ID", appID)[0]);
        }

        [HttpPost]
        public ActionResult Update(sLeaveModel model, HttpPostedFileBase file)
        {
            string fileName = UploadFile(file, model.leaveAppID);
            if (fileName != "")
            {
                if (TempData["ErrorMessage"] == null)
                {
                    String queryString = "UPDATE dbo.Leave SET Documentation = '" + fileName + "' WHERE Leave_Application_ID = " + model.leaveAppID;
                    DBExecuteQuery(queryString);
                    TempData["SuccessMessage"] = "Your documentation has been uploaded successfully.";
                }
            }
            else
            {
                TempData["WarningMessage"] = "You haven't selected a file to upload.";
            }
            return RedirectToAction("View", new { appID = model.leaveAppID });
        }

        private string UploadFile(HttpPostedFileBase file, int appID)
        {
            string fileName = "";

            // Verify that the user selected a file
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    // extract only the filename
                    fileName = Path.GetFileName(file.FileName);
                    string fName = appID + "-" + fileName;
                    string ext = Path.GetExtension(file.FileName);
                    if (ext != ".doc" && ext != ".docx" && ext != ".pdf" && ext != ".txt" && ext != ".rtf" &&
                        ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".bmp" &&
                        ext != ".csv" && ext != ".xls" && ext != ".xlsx" && ext != ".xlsm")
                    {
                        TempData["ErrorMEssage"] = "You have selected an invalid file type. " +
                            "<br /> Please upload one of the following file types; <b>.doc</b>, <b>.docx</b>, <b>.pdf</b>, <b>.txt</b>, <b>.rtf</b>, <b>.png</b>" +
                            ", <b>.jpg</b>, <b>.jpg</b>, <b>.jpeg</b>, <b>.bmp</b>, <b>.csv</b>, <b>.xls</b>, <b>.xlsx</b> or <b>.xlsm</b>";
                    }
                    else
                    {
                        // store the file inside ~/App_Data/Documentation folder
                        var path = Path.Combine(Server.MapPath("~/App_Data/Documentation"), fName);
                        file.SaveAs(path);
                        RemoveFile(appID, path);
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMEssage"] = "ERROR:" + ex.Message.ToString();
                }
            }
            return fileName;
        }

        private void RemoveFile(int appID, String fName)
        {
            string dir = Server.MapPath("~/App_Data/Documentation") + "\\";
            Output(dir);

            string[] fileList = Directory.GetFiles(dir, appID + "-*");
            foreach (string file in fileList)
            {
                if (!file.Equals(fName))
                    System.IO.File.Delete(file);
            }
        }
    }
}