using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MontesERP.Startup))]
namespace MontesERP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
