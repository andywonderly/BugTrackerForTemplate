using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker2.Models
{
    public class DashboardViewModel
    {
        public string UserId { get; set; }
        public string UserDisplayName { get; set; }
        public List<Ticket> UserAssignments { get; set; }
        public List<Project> UserProjects { get; set; }
        public List<Ticket> UserTickets { get; set; }
        public List<TicketStatus> TicketStatuses { get; set; }
        public List<TicketPriority> TicketPriorities { get; set; }
        public List<TicketType> TicketTypes { get; set; }
        public List<ApplicationUser> AllUsers { get; set; }
    }
}