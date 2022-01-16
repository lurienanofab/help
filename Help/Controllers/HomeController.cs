using Help.Models;
using LNF;
using System.Web.Mvc;

namespace Help.Controllers
{
    public class HomeController : HelpController
    {
        public HomeController(IProvider provider) : base(provider) { }

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("lnfc")]
        public ActionResult UserCommittee()
        {
            return View();
        }

        [Route("helpdesk")]
        public ActionResult Helpdesk(HelpdeskModel model)
        {
            model.CurrentUser = CurrentUser();
            return View(model);
        }

        [Route("calendar/{cal?}")]
        public ActionResult Calendar(string cal = "staff")
        {
            var model = new CalendarModel
            {
                Calendar = cal,
                CurrentUser = CurrentUser()
            };

            return View(model);
        }
    }
}