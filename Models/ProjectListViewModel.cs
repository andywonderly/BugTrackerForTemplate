using BugTracker2.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker2.Models
{
    public class ProjectListViewModel
    {
            public int Id { get; set; }
            public string Name { get; set; }
            public MultiSelectList users { get; set; }
            public string[] selected { get; set; }
            public string ProjectName { get; set; }
            public int ProjectId { get; set; }
            public List<ApplicationUser> AllUsers { get; set; }
            public List<Ticket> AllTickets { get; set; }
            public List<Project> AllProjects { get; set; }
            public ProjectUsersHelper ProjectUsersHelper { get; set; }
    }


}