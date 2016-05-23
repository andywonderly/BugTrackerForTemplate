using BugTracker2.Models;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTracker2.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string UserProjectId3 { get; set; }
        //[ForeignKey("ProjectUserId")]
        public virtual ICollection<Project> Projects { get; set; }
        //[ForeignKey("ProjectDeveloperId")]
        //public virtual ICollection<Projects> ProjectsDeveloped { get; set; }
        
        //public virtual ICollection<Projects> ProjectsMemberOf { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
        [ForeignKey("TicketOwnerId")]
        public virtual ICollection<Ticket> TicketsOwned { get; set; }
        [ForeignKey("TicketAssigneeId")]
        public virtual ICollection<Ticket> TicketsAssignedTo { get; set; }
        //[ForeignKey("ProjectDeveloperId")]
        //public virtual ICollection<ApplicationUser> ProjectDevelopers { get; set; }
        //public string UserProjectId { get; set; }
        
        

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }

        
        
        
    }



    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {

        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistory> TicketHistories { get; set; }
        public DbSet<TicketNotification> TicketNotifications { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketPriority> TicketPriorities { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }


        public DbSet<Project> Projects { get; set; }
        

    }
}