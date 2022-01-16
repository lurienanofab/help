using LNF.Help;
using System.Collections.Generic;

namespace Help.Models
{
    public class FeesModel
    {
        public bool IsAdmin { get; set; }
        public IEnumerable<FeeSection> Sections { get; set; }
    }
}