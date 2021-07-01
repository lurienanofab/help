using Help.Models;
using LNF;
using System.Web.Mvc;

namespace Help.Controllers
{
    public class HomeController : HelpController
    {
        public HomeController(IProvider provider) : base(provider) { }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Fees()
        {
            return View();
        }

        public ActionResult UserCommittee()
        {
            return View();
        }

        public ActionResult Helpdesk(HelpdeskModel model)
        {
            model.CurrentUser = CurrentUser();
            return View(model);
        }

        public ActionResult Calendar(string cal)
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