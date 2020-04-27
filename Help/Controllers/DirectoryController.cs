using Help.Models;
using LNF.Web;
using Newtonsoft.Json;
using System;
using System.Web;
using System.Web.Mvc;

namespace Help.Controllers
{
    public class DirectoryController : Controller
    {
        public ActionResult Index(DirectoryModel model)
        {
            model.CurrentUser = HttpContext.CurrentUser();
            CheckCookie(model);
            return View(model);
        }

        public ActionResult Staff(DirectoryModel model)
        {
            return RedirectToRoutePermanent("Directory");
        }

        public ActionResult Edit(DirectoryModel model)
        {
            model.CurrentUser = HttpContext.CurrentUser();
            if (model.Command == "save")
            {
                if (model.Save(Request.Form))
                    return RedirectToRoute("Directory");
            }
            else
                model.Load();

            return View(model);
        }

        private void CheckCookie(DirectoryModel model)
        {
            model.ViewDeleted = false;
            HttpCookie cookie = Request.Cookies["directory"];
            if (cookie != null)
            {
                var value = HttpUtility.UrlDecode(cookie.Value);
                
                var setting = JsonConvert.DeserializeAnonymousType(value, new { ViewDeleted = false });
                model.ViewDeleted = setting.ViewDeleted;
                cookie.Expires = DateTime.Now.AddYears(1);
                Response.SetCookie(cookie);
            }
        }
    }
}