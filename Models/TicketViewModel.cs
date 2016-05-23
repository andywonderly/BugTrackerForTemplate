using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker2.Models
{
    public class TicketViewModel
    {

        public int Id { get; set; }
        public int TicketId { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public IEnumerable<SelectListItem> Projects { get; set; }
        public string selected { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
        public string TicketTypeId { get; set; }
        public string TicketPriorityId { get; set; }
        public string TicketStatusId { get; set; }
        public string OwnerUserId { get; set; }
        public string AssignedToUserId { get; set; }
        public string ProjectName { get; set; }
        public string OwnerUserDisplayName { get; set; }
        public string AssignedToUserDisplayName { get; set; }
        public string TicketStatusName { get; set; }
        public string TicketPriorityName { get; set; }
        public string TicketTypeName { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string DevWarning { get; set; }
        public string ProjectManager { get; set; }
        public bool CurrentUserIsOwner { get; set; }
        public bool CurrentUserIsAssignedDev { get; set; }
        public bool CurrentUserIsProjectManager { get; set; }

        public ICollection<TicketHistory> TicketHistories { get; set; }
        public ICollection<TicketComment> TicketComments { get; set; }
        public ICollection<TicketAttachment> TicketAttachments { get; set; }

    }
}