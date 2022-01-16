using LNF.Data;
using LNF.Helpdesk;
using System.Configuration;

namespace Help.Models
{
    public class HelpdeskModel
    {
        public IClient CurrentUser { get; set; }

        public int GetPriority()
        {
            return (int)TicketPriorty.GeneralQuestion;
        }

        public string GetQueue()
        {
            if (string.IsNullOrEmpty(ConfigurationManager.AppSettings["HelpdeskQueue"]))
                return "helpdesk.support@lnf.umich.edu";
            else
                return ConfigurationManager.AppSettings["HelpdeskQueue"];
        }

        public string GetCurrentName()
        {
            return string.Format("{0} {1}", CurrentUser.FName, CurrentUser.LName);
        }

        public string GetCurrentEmail()
        {
            return CurrentUser.Email;
        }
    }
}