using LNF.Data;

namespace Help.Models
{
    public class CalendarModel
    {
        public IClient CurrentUser { get; set; }
        public string Calendar { get; set; }
    }
}