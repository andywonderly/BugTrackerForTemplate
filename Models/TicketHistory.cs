using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker2.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Property { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Changed { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }

        public virtual Ticket Ticket { get; set; }



    }

}