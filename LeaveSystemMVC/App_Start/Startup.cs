using System;
using System.Web.Mvc;
using System.Configuration;
using Hangfire;
using Hangfire.SqlServer;
using Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;

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

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            GlobalConfiguration.Configuration.UseSqlServerStorage(connectionString, new SqlServerStorageOptions { QueuePollInterval = TimeSpan.FromSeconds(1) });

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}