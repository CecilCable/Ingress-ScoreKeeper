
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Upchurch.Ingress.Startup))]

namespace Upchurch.Ingress
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
        }
    }
}