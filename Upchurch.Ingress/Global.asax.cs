using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;

namespace Upchurch.Ingress
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register); //API
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            //RouteConfig.RegisterRoutes(System.Web.Routing.RouteTable.Routes); //Routes
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}