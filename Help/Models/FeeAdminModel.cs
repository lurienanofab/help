using LNF.Help;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Help.Models
{
    public class FeeAdminModel
    {
        public Guid Id { get; internal set; }
        public FeeSection FeeSection { get; internal set; }
    }
}