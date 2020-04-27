using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Help
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Helpdesk",
                url: "helpdesk",
                defaults: new { controller = "Home", action = "Helpdesk" }
            );

            routes.MapRoute(
                name: "StaffCalendar",
                url: "calendar/{cal}",
                defaults: new { controller = "Home", action = "Calendar", cal = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DirectoryEdit",
                url: "directory/edit/{StaffDirectoryID}",
                defaults: new { controller = "Directory", action = "Edit", StaffDirectoryID = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DirectoryAlt",
                url: "staff",
                defaults: new { controller = "Directory", action = "Staff" }
            );

            routes.MapRoute(
                name: "Directory",
                url: "directory",
                defaults: new { controller = "Directory", action = "Index" }
            );

            routes.MapRoute(
                name: "UserCommittee",
                url: "lnfc",
                defaults: new { controller = "Home", action = "UserCommittee" }
            );

            routes.MapRoute(
                name: "Fees",
                url: "fees",
                defaults: new { controller = "Home", action = "Fees" }
            );

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
