using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker2.Models
{
    public class AssignTicketViewModel
    {
        public int TicketId { get; set; }
        public string AssignedToUserId { get; set; }
        public string selected { get; set; }
        public virtual ICollection<ApplicationUser> ProjectDevelopers { get; set; }
        public string Name { get; set; }
        public string TicketStatusId { get; set; }

    }
}