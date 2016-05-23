using BugTracker2;
using BugTracker2.Models;
//using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker2.Models
{
    public class Project
    {

 


        public int Id { get; set; }
        public string Name { get; set; }
        //public MultiSelectList users { get; set; }
        //public string selected { get; set; }

        [ForeignKey("Projects_Id")]
        public virtual ICollection<ApplicationUser> ProjectUsers { get; set; }
        public virtual ICollection<Ticket> ProjectTickets { get; set; }
        //public virtual ICollection<ApplicationUser> ProjectDevelopers { get; set; }
        public string ProjectManagerUserId { get; set; }
        //public MultiSelectList users { get; set; }
        //public string selected { get; set; }

        public string ProjectDeveloperId { get; set; }
        public string ProjectUserId { get; set; }
        //public virtual Tickets Ticket { get; set; }






    }
}