using LNF;
using LNF.DependencyInjection;
using LNF.Impl.DependencyInjection;
using LNF.Web;
using System;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Mvc;
using System.Web.Routing;

namespace Help
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private IDisposable _uow;
        private static IContainerContext _context;

        protected void Application_Start()
        {
            var assemblies = BuildManager.GetReferencedAssemblies().Cast<Assembly>().ToArray();

            var webapp = new WebApp();

            webapp.Context.EnablePropertyInjection();

            var wcc = webapp.GetConfiguration();
            wcc.RegisterAllTypes();

            webapp.BootstrapMvc(assemblies);

            _context = webapp.Context;

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        void Application_BeginRequest(object sender, EventArgs e)
        {
            _uow = _context.GetInstance<IProvider>().DataAccess.StartUnitOfWork();
        }

        void Application_EndRequest(object sender, EventArgs e)
        {
            if (_uow != null)
                _uow.Dispose();
        }
    }
}
