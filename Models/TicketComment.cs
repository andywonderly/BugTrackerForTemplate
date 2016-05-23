using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker2.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTimeOffset Created { get; set; }
        public int TicketId { get; set; }
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        //public int Tickets_Id { get; set; }

        public virtual Ticket Ticket { get; set; }
    }
}