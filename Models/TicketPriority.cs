using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker2.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual Ticket Ticket { get; set; }

    }
}