using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebFormsIdentity.Startup))]
namespace WebFormsIdentity
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
