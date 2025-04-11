using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Grocery.Startup))]
namespace Grocery
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
