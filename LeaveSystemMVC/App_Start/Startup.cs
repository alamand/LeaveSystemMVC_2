using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Hangfire;
using System.Configuration;
using Hangfire.SqlServer;

namespace LeaveSystemMVC.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            DataAnnotationsModelValidatorProvider.AddImplicitRequiredAttributeForValueTypes = false;
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                ExpireTimeSpan = TimeSpan.FromMinutes(15),
                AuthenticationType = "ApplicationCookie",
                LoginPath = new PathString("/Auth/Login")
            });

            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString, new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(1) });

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}