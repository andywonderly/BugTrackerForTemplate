using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker2.Models
{
    public class ProjectViewModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public IEnumerable<SelectListItem> users { get; set; }
        public string selected { get; set; }
        public string Name { get; set; }
        public string ProjectManagerUserId { get; set; }
    }


}