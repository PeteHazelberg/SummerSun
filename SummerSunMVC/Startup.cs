using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SummerSunMVC.Startup))]
namespace SummerSunMVC
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
