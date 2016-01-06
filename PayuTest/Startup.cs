using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(PayuTest.Startup))]
namespace PayuTest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
