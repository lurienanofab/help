using Help.Models;
using LNF;
using LNF.Data;
using LNF.Help;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Help.Controllers
{
    public class DirectoryController : HelpController
    {
        public DirectoryController(IProvider provider) : base(provider) { }

        [Route("directory")]
        public ActionResult Index(DirectoryModel model, string option = null)
        {
            if (option == "update-hours-text")
            {
                var items = Provider.Data.Client.GetStaffDirectories(active: null, deleted: null);

                foreach (var sd in items)
                {
                    var staffTime = new StaffTimeInfoCollection(sd.HoursXML);
                    var lines = staffTime.GetHoursText();
                    staffTime.SetHoursText(lines);
                    sd.HoursXML = staffTime.ToXML().ToString();
                    Provider.Data.Client.SaveStaffDirectory(sd);
                }

                return RedirectToAction("Index", "Directory", routeValues: null);
            }

            SetCurrentUser(model);
            SetStaffDirectory(model);
            SetDirectoryItems(model);
            SetStaffSelectListItems(model);
            CheckCookie(model);

            return View(model);
        }

        [Route("staff")]
        public ActionResult Staff()
        {
            return RedirectToActionPermanent("Index", "Directory", routeValues: null);
        }

        [Route("directory/edit/{StaffDirectoryID?}")]
        public ActionResult Edit(DirectoryModel model)
        {
            SetCurrentUser(model);
            SetStaffDirectory(model);
            SetStaffSelectListItems(model);

            if (model.Command == "save")
            {
                if (model.Save())
                {
                    Provider.Data.Client.SaveStaffDirectory(model.StaffDirectory);
                    return RedirectToAction("Index", "Directory");
                }
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

        private void SetCurrentUser(DirectoryModel model)
        {
            model.CurrentUser = CurrentUser();
        }

        private void SetStaffDirectory(DirectoryModel model)
        {
            model.StaffDirectory = null;
            if (model.StaffDirectoryID != 0)
                model.StaffDirectory = Provider.Data.Client.GetStaffDirectory(model.StaffDirectoryID);
        }

        private void SetDirectoryItems(DirectoryModel model)
        {
            model.DirectoryItems = Provider.Data.Client.GetStaffDirectories();
        }

        private void SetStaffSelectListItems(DirectoryModel model)
        {
            var staff = Provider.Data.Client.GetActiveClients(ClientPrivilege.Staff);
            var existing = Provider.Data.Client.GetStaffDirectories(active: null, deleted: null).Select(x => x.ClientID).ToArray();
            var result = staff.Where(x => !existing.Contains(x.ClientID)).Select(x => new SelectListItem() { Text = x.DisplayName, Value = x.ClientID.ToString() }).OrderBy(x => x.Text).ToArray();
            model.StaffSelectListItems = result;
        }
    }
}