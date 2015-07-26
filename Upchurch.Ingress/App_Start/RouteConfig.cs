using System.Web.Mvc;
using System.Web.Routing;

namespace Upchurch.Ingress
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute("Default", "", new {controller = "Home", action = "Index"});

            routes.MapRoute("Update", "{cp}", new { controller = "Home", action = "Update" }, new { cp = @"\d+" });

            routes.MapRoute("Partials", "{action}", new {controller = "Home", action = "Index"});
        }
    }
}