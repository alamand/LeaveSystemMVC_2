using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LeaveSystemMVC.Startup))]
namespace LeaveSystemMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
