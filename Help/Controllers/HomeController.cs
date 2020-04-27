using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using Help.Models;
using LNF.Web;

namespace Help.Controllers
{
    public class HomeController : Controller
    {
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
            model.CurrentUser = HttpContext.CurrentUser();
            return View(model);
        }

        public ActionResult Calendar(string cal)
        {
            var model = new CalendarModel
            {
                Calendar = cal,
                CurrentUser = HttpContext.CurrentUser()
            };

            return View(model);
        }
    }
}