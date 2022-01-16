using Help.Models;
using LNF;
using LNF.Data;
using LNF.Help;
using System;
using System.Web.Mvc;

namespace Help.Controllers
{
    public class FeesController : HelpController
    {
        public FeesController(IProvider provider) : base(provider) { }

        [Route("fees")]
        public ActionResult Index()
        {
            var model = GetModel();
            return View(model);
        }

        [Route("fees/admin")]
        public ActionResult Admin()
        {
            if (IsAdmin())
            {
                var model = GetModel();
                return View(model);
            }
            else
                return RedirectToAction("Index", "Fees", null);
        }

        [Route("fees/admin/edit/{id}")]
        public ActionResult AdminEditSection(Guid id)
        {
            if (IsAdmin())
            {
                var repo = new FeesRepository();
                var model = new FeeAdminModel { Id = id, FeeSection = repo.GetSection(id) };
                return View(model);
            }
            else
                return RedirectToAction("Index", "Fees", null);
        }

        [HttpPost, Route("fees/admin/add")]
        public ActionResult AddSection()
        {
            if (IsAdmin())
            {
                string title = Request.Form["AddSectionTitle"];

                if (!string.IsNullOrEmpty(title))
                {
                    var repo = new FeesRepository();
                    repo.AddSection(new FeeSection { Id = Guid.NewGuid(), Title = title, Links = new FeeLink[0] });
                }

                return RedirectToAction("Admin", "Fees");
            }
            else
                return RedirectToAction("Index", "Fees", null);
        }

        [HttpPost, Route("fees/admin/delete/{id}")]
        public ActionResult DeleteSection(Guid id)
        {
            if (IsAdmin())
            {
                var repo = new FeesRepository();
                repo.DeleteSection(id);
                return RedirectToAction("Admin", "Fees");
            }
            else
                return RedirectToAction("Index", "Fees", null);
        }

        [HttpPost, Route("fees/admin/{id}/update")]
        public ActionResult UpdateSection(Guid id)
        {
            if (IsAdmin())
            {
                string title = Request.Form["SectionTitle"];

                if (!string.IsNullOrEmpty(title))
                {
                    var repo = new FeesRepository();
                    repo.UpdateSection(id, title);
                }

                return RedirectToAction("AdminEditSection", "Fees", new { id = id.ToString("N") });
            }
            else
                return RedirectToAction("Index", "Fees", null);
        }

        [HttpPost, Route("fees/admin/{id}/link/{index}/update")]
        public ActionResult UpdateSectionLink(Guid id, int index)
        {
            if (IsAdmin())
            {
                string command = Request.Form["Command"];

                FeesRepository repo;

                switch (command)
                {
                    case "update":
                        string text = Request.Form["LinkText"];
                        string url = Request.Form["LinkUrl"];

                        if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(url))
                        {
                            repo = new FeesRepository();
                            repo.UpdateFeeLink(id, index, text, url);
                        }
                        break;
                    case "delete":
                        repo = new FeesRepository();
                        repo.RemoveFeeLink(id, index);
                        break;
                }


                return RedirectToAction("AdminEditSection", "Fees", new { id = id.ToString("N") });
            }
            else
                return RedirectToAction("Index", "Fees", null);
        }

        [HttpPost, Route("fees/admin/{id}/link/add")]
        public ActionResult AddSectionLink(Guid id)
        {
            if (IsAdmin())
            {
                var text = Request.Form["AddLinkText"];
                var url = Request.Form["AddLinkUrl"];

                if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(url))
                {
                    var repo = new FeesRepository();
                    repo.AddFeeLink(id, new FeeLink { Text = text, Url = url });
                }

                return RedirectToAction("AdminEditSection", "Fees", new { id = id.ToString("N") });
            }
            else
                return RedirectToAction("Index", "Fees", null);
        }

        private bool IsAdmin()
        {
            return CurrentUser().HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer);
        }

        private FeesModel GetModel()
        {
            var repo = new FeesRepository();

            var model = new FeesModel
            {
                IsAdmin = IsAdmin(),
                Sections = repo.FindSections()
            };

            return model;
        }
    }
}