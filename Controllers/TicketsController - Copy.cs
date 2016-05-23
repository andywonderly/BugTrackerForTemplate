//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Entity;
//using System.Linq;
//using System.Net;
//using System.Web;
//using System.Web.Mvc;
//using BugTracker2.Models;
//using Microsoft.AspNet.Identity;
//using BugTracker2.Models.Helpers;

//namespace BugTracker2.Controllers
//{
//    public class TicketsController : Controller
//    {
//        private ApplicationDbContext db = new ApplicationDbContext();

//        // GET: Tickets
//        [Authorize]
//        public ActionResult Index()
//        {
            
//            var userRolesHelper = new UserRolesHelper(db);
//            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
//            //var user = db.Users.Find(currentUserId);
//            IList<string> currentUserRoles = userRolesHelper.ListUserRoles(currentUserId);

//            bool isAdmin = false;
//            //bool isPMorDeveloper = false;
//            //bool isSubmitter = false;

//            foreach(var item in currentUserRoles)
//            {
//                if (item == "Admin")
//                    isAdmin = true;

//                //if (item == "Project Manager" || item == "Developer")
//                //    isPMorDeveloper = true;

//                //if (item == "Submitter")
//                //    isSubmitter = true;
//            }

//            List<Tickets> tickets = new List<Tickets>();
//            //List<Projects> allProjects = db.Projects.ToList();
//            //List<Tickets> allTickets = db.Tickets.ToList();

//            var helper = new TicketUsersHelper();
//            var ticketsToAdd = new List<Tickets>();


//            if (isAdmin)
//            {
//                ticketsToAdd = db.Tickets.ToList(); //If admin, list all tickets
//            }
//            else
//            {

//                IEnumerable<Tickets> allTickets = db.Tickets.ToList();

//                foreach(var item in allTickets) //If not admin, cycle through all tickets and test the
//                {                               //corresponding project's users against the current user
//                    if (item.TicketProject != null)
//                    {
//                        foreach (var item2 in item.TicketProject.ProjectUsers)
//                            if (item2.Id == currentUserId)
//                                ticketsToAdd.Add(item);
//                    }
//                }
//            }


//            return View(ticketsToAdd);
            
//        }

//        [Authorize]
//        // GET: Tickets/Details/5
//        public ActionResult Details(int? id)
//        {

//            //CHECK FOR CURRENT USER'S PERMISSION ON THE TICKET?

//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            Tickets tickets = db.Tickets.Find(id);
//            if (tickets == null)
//            {
//                return HttpNotFound();
//            }
//            return View(tickets);
//        }

//        // GET: Tickets/Create
//        [Authorize]
//        public ActionResult CreateTicket()
//        {
//            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
//            var currentUser = db.Users.Find(currentUserId);

//            var userRolesHelper = new UserRolesHelper(db);

//            //If user is Admin, allow submitting to all projects, else only list projects
//            //that the current user is on.
//            if(userRolesHelper.IsUserInRole(currentUserId, "Admin"))
//            {
//                ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
//            } else
//            {
//                ICollection<Projects> userProjects = new List<Projects>();
//                List<Projects> allProjects = db.Projects.ToList();


//                foreach(var item in allProjects)
//                {
//                    foreach (var item2 in item.ProjectUsers)
//                        if (item2.Id == currentUserId)
//                            userProjects.Add(item);
//                }

//                ViewBag.ProjectId = new SelectList(userProjects, "Id", "Name");
//            }

//            //The rest of the SelectLists
//            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
//            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
//            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");

//            return View();
//        }

//        // POST: Tickets/Create
//        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
//        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [Authorize]
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult CreateTicket([Bind(Include = "Id, Title, Description, ProjectId, selected, TicketTypeId, TicketPriorityId, TicketStatusId")] TicketsViewModel ticket)
//        {

//            //CREATE VIEW NEEDS A LIST OF PROJECTS TO SELECT FROM
//            if (ModelState.IsValid)
//            {
//                var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

//                var ticketToAdd = new Tickets();

//                ticketToAdd.ProjectId = ticket.ProjectId.ToString();
//                ticketToAdd.Created = DateTimeOffset.Now;
//                ticketToAdd.Updated = DateTimeOffset.Now;
//                ticketToAdd.OwnerUserId = currentUserId;
//                ticketToAdd.Title = ticket.Title;
//                ticketToAdd.Description = ticket.Description;
//                ticketToAdd.TicketPriorityId = ticket.TicketPriorityId;
//                ticketToAdd.TicketStatusId = ticket.TicketStatusId;
//                ticketToAdd.TicketTypeId = ticket.TicketTypeId;

//                ticketToAdd.TicketOwner = db.Users.Find(currentUserId);

//                var projectIdInt = 0;
//                Int32.TryParse(ticketToAdd.ProjectId, out projectIdInt); 
                    
//                var ticketProject = db.Projects.Find(projectIdInt);
//                ticketToAdd.TicketProject = ticketProject;

//                db.Tickets.Add(ticketToAdd);
//                db.SaveChanges();
//                return RedirectToAction("Index");
//            }

//            return View(ticket);
//        }

//        // GET: Tickets/Edit/5
//        [Authorize(Roles ="Admin, Project Manager")]
//        public ActionResult EditTicket(int? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }

//            var id2 = id.GetValueOrDefault();

//            var helper = new TicketUsersHelper();
//            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

//            //If user is a submitter
//            if (this.User.IsInRole("Submitter") && helper.IsUserOnTicket(id2, currentUserId))
//            {
//                return RedirectToAction("Index");
//            }

//            Tickets ticket = db.Tickets.Find(id);
            
//            if (ticket == null)
//            {
//                return HttpNotFound();
//            }
//            return View(ticket);
//        }

//        // POST: Tickets/Edit/5
//        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
//        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
//        [Authorize(Roles ="Admin, Project Manager")]
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult EditTicket([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedToUserId")] Tickets tickets)
//        {
//            if (ModelState.IsValid)
//            {
//                db.Entry(tickets).State = EntityState.Modified;
//                db.SaveChanges();
//                return RedirectToAction("Index");
//            }
//            return View(tickets);
//        }

//        // GET: Tickets/Delete/5
//        public ActionResult DeleteTicket(int? id)
//        {
//            if (id == null)
//            {
//                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
//            }
//            Tickets tickets = db.Tickets.Find(id);
//            if (tickets == null)
//            {
//                return HttpNotFound();
//            }
//            return View(tickets);
//        }

//        // POST: Tickets/Delete/5
//        [HttpPost, ActionName("DeleteTicket")]
//        [ValidateAntiForgeryToken]
//        public ActionResult DeleteConfirmed(int id)
//        {
//            Tickets tickets = db.Tickets.Find(id);
//            db.Tickets.Remove(tickets);
//            db.SaveChanges();
//            return RedirectToAction("Index");
//        }

//        //GET: Tickets/AssignTicket
//        [Authorize(Roles ="Admin, Project Manager")]
//        public ActionResult AssignTicket(int id)
//        {
//            Tickets ticket = db.Tickets.Find(id);
//            var currentUser = db.Users.Find(System.Web.HttpContext.Current.User.Identity.GetUserId());
//            UserRolesHelper userRolesHelper = new UserRolesHelper(db);

//            bool currentUserIsOnProject = false;

//            //If current user != Admin, check to see if current user is on the project.
//            //Theoretically, this check should never fail because PMs viewing this page
//            //should only be able to see projects that they are on.
//            if (!userRolesHelper.IsUserInRole(currentUser.Id, "Admin"))
//            {
//                foreach (var item in ticket.TicketProject.ProjectUsers)
//                    if (item.Id == currentUser.Id)
//                        currentUserIsOnProject = true;
//            }

//            //If the current user is non-Admin and they were not found to be on the project in the previous
//            //if/foreach loop, then kick back to Index
//            if (!currentUserIsOnProject && !userRolesHelper.IsUserInRole(currentUser.Id, "Admin"))
//                return RedirectToAction("Index");

//            bool projectHasAtLeastOneDeveloper = false;

//            foreach (var item in ticket.TicketProject.ProjectUsers) //Cycle through users, add any developers to ProjectDevelopers
//            {
//                if (userRolesHelper.IsUserInRole(item.Id, "Developer"))
//                {
//                    //ticket.TicketProject.ProjectDevelopers.Add(item);
//                    ticket.TicketProject.ProjectDeveloperId = item.Id;
//                    projectHasAtLeastOneDeveloper = true;
//                }
//            }

            

//            if (!projectHasAtLeastOneDeveloper) //If no PMs were found, kick back to Index
//                return RedirectToAction("Index");
                


//            AssignTicketViewModel assignTicketViewModel = new AssignTicketViewModel();
//            assignTicketViewModel.TicketId = id;

//            //***FOLLOWING 2 LINES COMMENTED OUT TO TRY TO RUN SOMETHING ELSE
//            //assignTicketViewModel.ProjectDevelopers = ticket.TicketProject.ProjectDevelopers;

//            ProjectsHelper projectsHelper = new ProjectsHelper();

//            ICollection<ApplicationUser> projectDevs = new List<ApplicationUser>();
//            List<ApplicationUser> allUsers = db.Users.ToList();
//            foreach(var item in allUsers)
//            {
//                if (userRolesHelper.IsUserInRole(item.Id, "Developer") && projectsHelper.IsUserOnProject(item.Id, ticket.TicketProject.Id))
//                    projectDevs.Add(item);
//            }

//            ViewBag.AssignedToUserId = new SelectList(projectDevs, "Id", "DisplayName");

//            //Copy ticket project Devs to view model project Devs
//            //foreach (var item in ticket.TicketProject.ProjectDevelopers)
//            //    assignTicketViewModel.ProjectDevelopers.Add(item);

//            //"Selected" is not currently a part of the controller portion of the selectList, so the following
//            //if statement not have an effect.

//            if (ticket.AssignedToUserId != null) // If a dev was already assigned, make it the selected list item
//                assignTicketViewModel.selected = db.Users.Find(ticket.AssignedToUserId).DisplayName;

//            //foreach(var item in ticket.TicketProject.Users) //If the current user is a project user, return
//                                                            //the view model
//            //{
//                //if (item.Id == currentUser.Id)
//                    return View(assignTicketViewModel);
//            //}

//            //ViewBag.ProjectManagers = new SelectList(ticket.TicketProject.Users.Where(r => r.Roles), "Id", "Name");

//            //return RedirectToAction("Index"); //You should only get here if you're not on the ticket's project
//        }

//        [Authorize(Roles = "Admin, Project Manager")]
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public ActionResult AssignTicket([Bind(Include = "selected,AssignedToUserId,TicketId")] AssignTicketViewModel ticket)
//        {

//            Tickets ticketToAssign = db.Tickets.Find(ticket.TicketId);
//            ticketToAssign.AssignedToUserId = ticket.AssignedToUserId;

//            if (ModelState.IsValid)
//            {
//                db.Tickets.Attach(ticketToAssign);
//                db.Entry(ticketToAssign).Property("AssignedToUserId").IsModified = true;
//                db.SaveChanges();
//            }
//            return RedirectToAction("Index");
//        }

//        protected override void Dispose(bool disposing)
//        {
//            if (disposing)
//            {
//                db.Dispose();
//            }
//            base.Dispose(disposing);
//        }
//    }
//}
