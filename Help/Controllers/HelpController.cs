using LNF;
using LNF.Data;
using LNF.Web;
using System.Web.Mvc;

namespace Help.Controllers
{
    public abstract class HelpController : Controller
    {
        protected IProvider Provider { get; }

        protected IClient CurrentUser() => HttpContext.CurrentUser(Provider);

        public HelpController(IProvider provider)
        {
            Provider = provider;
        }
    }
}