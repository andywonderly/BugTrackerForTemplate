using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker2.Models;
using Microsoft.AspNet.Identity;
using BugTracker2.Models.Helpers;
using System.IO;
using static BugTracker2.Models.Ticket;
using SendGrid;
using System.Net.Mail;
using System.Configuration;

namespace BugTracker2.Controllers
{
    public class TicketsController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Tickets
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        public ActionResult Index()
        { 
            //This Index action is a navigation-only action based around a cookie.
            //It navigates the user to whichever Tickets tab they were previously on.

            HttpCookie TicketNav = Request.Cookies["TicketNav"];

            if(TicketNav != null)
            {
                string ticketNav = TicketNav.Values["TicketNav"].ToString();

                switch(ticketNav)
                {
                    case "MySubmittedTickets":
                        return RedirectToAction("MySubmittedTickets");
                        break;
                    case "AllTickets":
                        return RedirectToAction("AllTickets");
                        break;
                    case "MyProjectTickets":
                        return RedirectToAction("MyProjectTickets");
                        break;
                    default:
                        break;
                }
            }

            return RedirectToAction("MySubmittedTickets");
        }

        // GET: Tickets
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        public ActionResult MySubmittedTickets()
            {
                       
            //This controller action returns tickets that the current user has submitted.

            //private ApplicationDbContext db = new ApplicationDbContext();
            var userRolesHelper = new UserRolesHelper(db);
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

            ApplicationUser currentUser = db.Users.Find(currentUserId);
            List<Ticket> currentUserTickets = db.Tickets.Where(n => n.TicketOwnerId == currentUserId).ToList();

            //IList<string> currentUserRoles = new List<string>();
            //currentUserRoles = userRolesHelper.ListUserRoles(currentUserId).ToList();

            //Test whether the current user is a submitter and no other role.
            //bool submitterOnly = true;

            //foreach (var item in currentUserRoles)
            //    if (item == "Admin" || item == "Project Manager" || item == "Developer")
            //        submitterOnly = false;

            //First add tickets that the current user owns.  This will always happen
            //tickets = db.Tickets.Where(n => n.TicketOwnerId == currentUserId).ToList();

            //If user has more roles, also add tickets relating to projects they're on.
            // 1. Cycle through current user projects
            // 2. Cycle through tickets of those projects
            // 3. Cycle through current ticket list to check whether the ticket is already
            //    on the list

            //if (!submitterOnly) 

            /*

            //The following code doesn't work because in the last for loop,
            //currentUserTickets is being modified as it is being cycled through.
            //The tickets to add should be added to a new list, then that list added
            //to currentUserTickets after the cascading for loops are complete.

               foreach (var item in currentUser.Projects)
                    foreach (var item2 in item.ProjectTickets)
                        foreach (var item3 in currentUserTickets)
                            if (item2.Id != item3.Id)
                                currentUserTickets.Add(item2);

              */




            //var user = db.Users.Find(currentUserId);

            List<TicketViewModel> ticketsViewModel = new List<TicketViewModel>();


            foreach(var item in currentUserTickets)
            {
                TicketViewModel temp = new TicketViewModel();
                temp.Id = item.Id;
                temp.Title = item.Title;
                temp.Description = item.Description;
                temp.OwnerUserDisplayName = db.Users.Find(item.OwnerUserId).DisplayName;



                try
                {
                    temp.AssignedToUserDisplayName = db.Users.Find(item.AssignedToUserId).DisplayName;
                }
                catch
                {
                    temp.AssignedToUserDisplayName = "***";
                }

                if (item.AssignedToUserId != null)
                {
                    if (!userRolesHelper.IsUserInRole(item.AssignedToUserId, "Developer"))
                    {
                        temp.DevWarning = "Assigned user is no longer a developer.";
                    }
                }
                

                temp.TicketStatusName = db.TicketStatuses.FirstOrDefault(n => n.Id.ToString() == item.TicketStatusId).Name;
                temp.TicketPriorityName = db.TicketPriorities.FirstOrDefault(n => n.Id.ToString() == item.TicketPriorityId).Name;
                temp.TicketTypeName = db.TicketTypes.FirstOrDefault(n => n.Id.ToString() == item.TicketTypeId).Name;
                //var ticketTypeInt = 0;
                //Int32.TryParse(item.TicketTypeId, out ticketTypeInt);
                //temp.TicketTypeName = db.TicketTypes.Find(ticketTypeInt).Name;
                temp.ProjectName = item.TicketProject.Name;
                if (item.TicketProject.ProjectManagerUserId != null)
                    temp.ProjectManager = db.Users.Find(item.TicketProject.ProjectManagerUserId).DisplayName;
                temp.Created = item.Created;
                temp.Updated = item.Updated;
                //ADD CODE TO DISPLAY NOTIFICATION IF ASSIGNED USER IS NO LONGER A DEV
                //VIEWBAG.USERWARNING MAYBE

                if (currentUserId == item.OwnerUserId)
                    temp.CurrentUserIsOwner = true;

                if (currentUserId == item.AssignedToUserId)
                    temp.CurrentUserIsAssignedDev = true;

                if (currentUserId == item.TicketProject.ProjectManagerUserId)
                    temp.CurrentUserIsProjectManager = true;

                ticketsViewModel.Add(temp);

            }

            //waspViewBag.Message = "Hi";
            return View(ticketsViewModel);
            
        }

        // GET: MyProjectTickets
        [Authorize(Roles ="Admin, Project Manager, Developer, Admin Demo, Project Manager Demo, Developer Demo")]
        public ActionResult MyProjectTickets()
        {
            //HttpContext.Current.Response.Cookies.Remove("TicketNav");

            //Will the cookie Values.Add command overwrite the old cookie value
            //or add a new one?

            HttpCookie TicketNav = new HttpCookie("TicketNav");
            TicketNav["TicketNav"] = "MyProjectTickets";
            Response.Cookies.Add(TicketNav);

            var userRolesHelper = new UserRolesHelper(db);
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            var allTickets = db.Tickets.ToList();
            List<Ticket> tickets = new List<Ticket>();

            //As a pre-check, check to see if the current user is assigned to any
            //tickets as a developer.  It is possible that a user could have been assigned
            //to a ticket, but be removed from the dev role since the assignment.

            var currentUserRoles = userRolesHelper.ListUserRoles(currentUserId);
            bool isOnlySubmitter = true;

            foreach (var item in currentUserRoles)
                if (item == "Project Manager" || item == "Developer" || item == "Admin")
                    isOnlySubmitter = false;

            //If currentUser is indeed only a submitter, add owned and assigned tickets
            //to the list and return the list.  This ends the controller action

            //UPDATE:  This code should never be reached.  Users who are only in the submitter
            //role will not be able to access this controller action.  Furthermore, if this code
            //actually runs, it will return a runtime error because this code block returns a list
            //of class Ticket, and the view takes in a list of class TicketViewModel.
            if(isOnlySubmitter)
            {

                

                foreach(var item in allTickets)
                {
                    if (item.TicketOwnerId == currentUserId)
                    {
                        tickets.Add(item); //add ticket if currentUser is the owner
                    }
                    else if(item.AssignedToUserId == currentUserId)
                    {
                        tickets.Add(item); //if not owner, add if currentUser is the assigned dev
                    }
                }

                
                return View(tickets); //return tickets to view, ending the action
            }

            //If current user has roles other than submitter, the previous If block is skipped
            //and the controller action continues
            

            //Cycle through current user projects and the tickets of those projects.
            //Add the tickets to the tickets list
            foreach (var item in currentUser.Projects)
                foreach (var item2 in item.ProjectTickets)
                    tickets.Add(item2);

            //ADD CODE FOR ADDING TICKETS THAT THE CURRENT USER HAS BEEN ASSIGNED TO AS A DEVELOPER
            //REGARDLESS OF IF THEY'RE CURRENTLY A DEVELOPER


            //Make another ticket list to add tickets to which currentUser is assigned or has submitted
            List<Ticket> tickets2 = new List<Ticket>();

            foreach (var item in allTickets)
            {
                if (item.AssignedToUserId == currentUserId)
                {
                    tickets2.Add(item);
                }
                else if(item.OwnerUserId == currentUserId)
                {
                    tickets2.Add(item);
                }
            }

            //I KNOW THE ABOVE CODE IS A REPEAT.  IT CAN BE CONDENSED.  HOPEFULLY I WILL GET TO THAT.

            //Now remove any tickets from tickets2 that already exist in tickets.
            //First make a copy of tickets2 to foreach through.  This is because 
            //you cannot foreach through a list and modify it inside of the
            //foreach loop.

            var tickets2Copy = tickets2.ToList();

            foreach (var item in tickets2)
                foreach (var item2 in tickets)
                    if (item.Id != item2.Id)
                        tickets2Copy.Remove(item);


            //Add any tickets that remain in tickets2 to tickets.
            foreach (var item in tickets2Copy)
                tickets.Add(item);

            List<TicketViewModel> ticketViewModel = new List<TicketViewModel>();

            //Copy tickets properties to the view model.
            foreach (var item in tickets)
            {
                TicketViewModel temp = new TicketViewModel();
                temp.Id = item.Id;
                temp.Title = item.Title;
                temp.Description = item.Description;
                temp.OwnerUserDisplayName = db.Users.Find(item.OwnerUserId).DisplayName;

                try
                {
                    temp.AssignedToUserDisplayName = db.Users.Find(item.AssignedToUserId).DisplayName;
                }
                catch
                {
                    temp.AssignedToUserDisplayName = "***";
                }

                if (item.AssignedToUserId != null)
                {
                    if (!userRolesHelper.IsUserInRole(item.AssignedToUserId, "Developer"))
                    {
                        temp.DevWarning = "Assigned user is no longer a developer.";
                    }
                }


                temp.TicketStatusName = db.TicketStatuses.FirstOrDefault(n => n.Id.ToString() == item.TicketStatusId).Name;
                temp.TicketPriorityName = db.TicketPriorities.FirstOrDefault(n => n.Id.ToString() == item.TicketPriorityId).Name;
                temp.TicketTypeName = db.TicketTypes.FirstOrDefault(n => n.Id.ToString() == item.TicketTypeId).Name;
                temp.ProjectName = item.TicketProject.Name;
                if(item.TicketProject.ProjectManagerUserId != null)
                    temp.ProjectManager = db.Users.Find(item.TicketProject.ProjectManagerUserId).DisplayName;
                
                temp.Created = item.Created;
                temp.Updated = item.Updated;
                //ADD CODE TO DISPLAY NOTIFICATION IF ASSIGNED USER IS NO LONGER A DEV
                //VIEWBAG.USERWARNING MAYBE

                if (currentUserId == item.OwnerUserId)
                    temp.CurrentUserIsOwner = true;

                if (currentUserId == item.AssignedToUserId)
                    temp.CurrentUserIsAssignedDev = true;

                if (currentUserId == item.TicketProject.ProjectManagerUserId)
                    temp.CurrentUserIsProjectManager = true;

                ticketViewModel.Add(temp);

            }

            IEnumerable<TicketViewModel> enumTicketViewModel = ticketViewModel.ToList();

            return View(enumTicketViewModel);
        }

        //GET:  Tickets/AllTickets
        [Authorize(Roles ="Admin")]
        public ActionResult AllTickets()
        {
            HttpCookie TicketNav = new HttpCookie("TicketNav");
            TicketNav["TicketNav"] = "AllTickets";
            Response.Cookies.Add(TicketNav);

            //var x = TicketNav;
            
            
            //Admin-only:  return all tickets in the system
            var tickets = db.Tickets.ToList();

            List<TicketViewModel> ticketsViewModel = new List<TicketViewModel>();

            UserRolesHelper userRolesHelper = new UserRolesHelper(db);

            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

            foreach (var item in tickets)
            {
                TicketViewModel temp = new TicketViewModel();
                temp.Id = item.Id;
                temp.Title = item.Title;
                temp.Description = item.Description;
                temp.OwnerUserDisplayName = db.Users.Find(item.OwnerUserId).DisplayName;

                try
                {
                    temp.AssignedToUserDisplayName = db.Users.Find(item.AssignedToUserId).DisplayName;
                }
                catch
                {
                    temp.AssignedToUserDisplayName = "***";
                }

                if (item.AssignedToUserId != null)
                {
                    if (!userRolesHelper.IsUserInRole(item.AssignedToUserId, "Developer"))
                    {
                        temp.DevWarning = "Assigned user is no longer a developer.";
                    }
                }

                temp.TicketStatusName = db.TicketStatuses.FirstOrDefault(n => n.Id.ToString() == item.TicketStatusId).Name;
                temp.TicketPriorityName = db.TicketPriorities.FirstOrDefault(n => n.Id.ToString() == item.TicketPriorityId).Name;
                temp.TicketTypeName = db.TicketTypes.FirstOrDefault(n => n.Id.ToString() == item.TicketTypeId).Name;
                temp.ProjectName = item.TicketProject.Name;
                if (item.TicketProject.ProjectManagerUserId != null)
                    temp.ProjectManager = db.Users.Find(item.TicketProject.ProjectManagerUserId).DisplayName;
                temp.Created = item.Created;
                temp.Updated = item.Updated;
                //ADD CODE TO DISPLAY NOTIFICATION IF ASSIGNED USER IS NO LONGER A DEV
                //VIEWBAG.USERWARNING MAYBE

                if (currentUserId == item.OwnerUserId)
                    temp.CurrentUserIsOwner = true;

                if (currentUserId == item.AssignedToUserId)
                    temp.CurrentUserIsAssignedDev = true;

                if (currentUserId == item.TicketProject.ProjectManagerUserId)
                    temp.CurrentUserIsProjectManager = true;

                ticketsViewModel.Add(temp);

            }

            return View(ticketsViewModel);
        }


        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {

            //CHECK FOR CURRENT USER'S PERMISSION ON THE TICKET?

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }


            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);


            //Check whether the user is a developer only, and if so, check if they're assigned to
            //the ticket.  If not, set ShowForms to false so that the comment and attachment forms will
            //be hidden on the details page.  This is for the user's own benefit of letting
            // them know they can't send comments or attachments -- it is not a security
            //measure.  If they somehow send form data anyway, the post action will recognize that they
            //are not the assigned dev and will not process whatever they sent.

            //I use viewbag here as a shortcut...the more robust solution would be to use a view model.

            TicketViewModel ticketViewModel = new TicketViewModel();

            if (this.User.IsInRole("Developer") && !this.User.IsInRole("Admin") && !this.User.IsInRole("Project Manager"))
            {
                if (currentUser.Id != ticket.AssignedToUserId)
                {
                    ViewBag.ShowForms = false;
                    //CurrentUserIsAssignedDev is currently not used for anything.  I am using
                    //the viewbag.ShowForms property instead
                    ticketViewModel.CurrentUserIsAssignedDev = true;
                }  
            }

            //If you submitted the ticket, the comment and attach forms will be shown
            if (currentUserId == ticket.OwnerUserId)
                ViewBag.ShowForms = true; 

            //Convert the status, priority, and type Ids to ints

            var type = 0;
            var status = 0;
            var priority = 0;

            Int32.TryParse(ticket.TicketTypeId, out type);
            Int32.TryParse(ticket.TicketStatusId, out status);
            Int32.TryParse(ticket.TicketPriorityId, out priority);

            ticketViewModel.Title = ticket.Title;
            ticketViewModel.Description = ticket.Description;
            ticketViewModel.TicketTypeName = db.TicketTypes.Find(type).Name;
            ticketViewModel.TicketStatusName = db.TicketStatuses.Find(status).Name;
            ticketViewModel.TicketPriorityName = db.TicketPriorities.Find(priority).Name;
            ticketViewModel.ProjectName = db.Projects.Find(ticket.TicketProject.Id).Name;
            ticketViewModel.OwnerUserDisplayName = db.Users.Find(ticket.OwnerUserId).DisplayName;
            ticketViewModel.Created = ticket.Created;
            ticketViewModel.Updated = ticket.Updated;

            ticketViewModel.TicketHistories = ticket.TicketHistories.ToList();
            ticketViewModel.TicketAttachments = ticket.TicketAttachments.ToList();
            ticketViewModel.TicketComments = ticket.TicketComments.ToList();
            ticketViewModel.TicketId = id.Value;

            if (ticket.AssignedToUserId != null)
            {
                ticketViewModel.AssignedToUserDisplayName = db.Users.First(n=> n.Id == ticket.AssignedToUserId).DisplayName;
            }
            else
            {
                ticketViewModel.AssignedToUserDisplayName = "***";
            }

            ProjectUsersHelper projectUsersHelper = new ProjectUsersHelper();

            if ( (this.User.IsInRole("Project Manager") && !(this.User.IsInRole("Admin")) && 
                projectUsersHelper.IsUserOnProject(ticket.TicketProject.Id, currentUser.Id)) || 
                (this.User.IsInRole("Developer") && !(this.User.IsInRole("Admin")) && 
                !(this.User.IsInRole("Project Manager")) && currentUser.Id == ticket.AssignedToUserId) ||
                this.User.IsInRole("Admin") )
            {
                ViewBag.ShowEditButton = true;
            }
            else
            {
                ViewBag.ShowEditButton = false;
            }

            //var x = ticketViewModel.AssignedToUserDisplayName;
            //var x = ticket.c.Count;
            return View(ticketViewModel);
        }

        // GET: Tickets/Create
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        public ActionResult CreateTicket()
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var currentUser = db.Users.Find(currentUserId);

            var userRolesHelper = new UserRolesHelper(db);

            //If user is Admin, allow submitting to all projects, else only list projects
            //that the current user is on.
            if(userRolesHelper.IsUserInRole(currentUserId, "Admin"))
            {
                ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Name");
            } else
            {
                ICollection<Project> userProjects = new List<Project>();
                List<Project> allProjects = db.Projects.ToList();


                foreach(var item in allProjects)
                {
                    foreach (var item2 in item.ProjectUsers)
                        if (item2.Id == currentUserId)
                            userProjects.Add(item);
                }

                ViewBag.ProjectId = new SelectList(userProjects, "Id", "Name");
            }

            //The rest of the SelectLists
            

            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", 2);
            //ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            //Status will be set to Submitted in the post action

            //var tt = db.TicketTypes;

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateTicket([Bind(Include = "Id, Title, Description, ProjectId, selected, TicketTypeId, TicketPriorityId")] TicketViewModel ticket)
        {

            //CREATE VIEW NEEDS A LIST OF PROJECTS TO SELECT FROM
            if (ModelState.IsValid)
            {
                var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

                var ticketToAdd = new Ticket() { TicketStatusId = "1" };

                ticketToAdd.ProjectId = ticket.ProjectId.ToString();
                ticketToAdd.Created = DateTimeOffset.Now;
                ticketToAdd.Updated = DateTimeOffset.Now;
                ticketToAdd.OwnerUserId = currentUserId;
                ticketToAdd.Title = ticket.Title;
                ticketToAdd.Description = ticket.Description;
                ticketToAdd.TicketPriorityId = ticket.TicketPriorityId;
                ticketToAdd.TicketTypeId = ticket.TicketTypeId;

                ticketToAdd.TicketOwner = db.Users.Find(currentUserId);

                var projectIdInt = 0;
                Int32.TryParse(ticketToAdd.ProjectId, out projectIdInt); 
                    
                var ticketProject = db.Projects.Find(projectIdInt);
                ticketToAdd.TicketProject = ticketProject;

                TicketHistory ticketHistory = TicketHistoryUpdater.New(db, ticketToAdd, currentUserId, "Ticket created");

                db.TicketHistories.Add(ticketHistory);
                db.Tickets.Add(ticketToAdd);

                db.SaveChanges();

                //TicketHistoryUpdater.Update(db, ticketToAdd, currentUserId, "Ticket created");

                //TicketHistory ticketHistory = new TicketHistory();
                //ticketHistory.TicketId = ticket.Id;
                //ticketHistory.DateTime = DateTimeOffset.Now;
                //ticketHistory.OldValue = "N/A";
                //ticketHistory.Property = "Created";

                //ticketHistory.UserId = currentUserId;

                //db.TicketHistories.Attach(ticketHistory);
                //db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize(Roles ="Admin, Project Manager, Developer")]
        public ActionResult EditTicket(int? id)
        {



            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var userRolesHelper = new UserRolesHelper(db);
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            

            var id2 = id.GetValueOrDefault();

            var ticketUsershelper = new TicketUsersHelper();

            //If user is a submitter
            if (this.User.IsInRole("Submitter") && ticketUsershelper.IsUserOnTicket(id2, currentUserId))
            {
                return RedirectToAction("Index");
            }

            

            Ticket ticket = db.Tickets.Find(id);
            TicketViewModel ticketsViewModel = new TicketViewModel();
            TicketsHelper ticketsHelper = new TicketsHelper();

            if (this.User.IsInRole("Developer") && !this.User.IsInRole("Admin") && !this.User.IsInRole("Project Manager"))
            {
                if(currentUser.Id != ticket.AssignedToUserId)
                    return RedirectToAction("Index");
            }

            ticketsViewModel.Description = ticket.Description;
            ticketsViewModel.Id = ticket.Id;
            ticketsViewModel.Title = ticket.Title;
            //ticketsViewModel.TicketPriorityId = ticket.TicketPriorityId;
            //ticketsViewModel.TicketStatusId = ticket.TicketStatusId;
            //ticketsViewModel.TicketTypeId = ticket.TicketTypeId;

            //List<SelectListItem> ticketTypeList = ticketsHelper.TicketTypesSelectList(ticket);
            //List<SelectListItem> ticketPriorityList = ticketsHelper.TicketPrioritiesSelectList(ticket);
            //List<SelectListItem> ticketStatusList = ticketsHelper.TicketStatusesSelectList(ticket);

            //SelectListItem item2 = new SelectListItem(); //blank SelectListItem for use in foreach loops

            /*   
             //USING HELPER FUNCTIONS INSTEAD   
                       foreach (var item in db.TicketTypes)
                       {
                           bool isSelected = false;

                           if (item.Id.ToString() == ticket.TicketTypeId)
                               isSelected = true;

                           item2 = new SelectListItem {
                               Selected = isSelected,
                               Text = item.Name,
                               Value = item.Id.ToString() };

                           ticketTypeList.Add(item2);

                       }

                       foreach (var item in db.TicketPriorities)
                       {
                           bool isSelected = false;

                           if (item.Id.ToString() == ticket.TicketPriorityId)
                               isSelected = true;

                           item2 = new SelectListItem
                           {
                               Selected = isSelected,
                               Text = item.Name,
                               Value = item.Id.ToString()
                           };

                           ticketPriorityList.Add(item2);

                       }

                       foreach (var item in db.TicketStatuses)
                       {
                           bool isSelected = false;

                           if (item.Id.ToString() == ticket.TicketStatusId)
                               isSelected = true;

                           item2 = new SelectListItem
                           {
                               Selected = isSelected,
                               Text = item.Name,
                               Value = item.Id.ToString()
                           };

                           ticketStatusList.Add(item2);

                       }
                       */

            var ticketTypeId = 0;
            Int32.TryParse(ticket.TicketTypeId, out ticketTypeId);

            var ticketTypeSelected = db.TicketTypes.Find(ticketTypeId).Id;
            SelectList TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticketTypeSelected);
            //foreach(var item in TicketTypeId)
            //{
            //    if (item.Value == ticket.TicketTypeId.ToString())
            //        item.Selected = true;
            //}

            ViewBag.TicketTypeId = TicketTypeId;
            var z = TicketTypeId.SelectedValue;

            var ticketStatusId = 0;
            Int32.TryParse(ticket.TicketStatusId, out ticketStatusId);

            var ticketStatusSelected = db.TicketStatuses.Find(ticketStatusId).Id;
            SelectList TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticketStatusSelected);
            //foreach (var item in TicketTypeId)
            //{
            //    if (item.Value == ticket.TicketStatusId.ToString())
            //        item.Selected = true;
            //}
            ViewBag.TicketStatusId = TicketStatusId;
            var y = TicketStatusId.SelectedValue;

            var ticketPriorityId = 0;
            Int32.TryParse(ticket.TicketPriorityId, out ticketPriorityId);

            var ticketPrioritySelected = db.TicketPriorities.Find(ticketPriorityId).Id;
            SelectList TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticketPrioritySelected);
            //foreach (var item in TicketPriorityId)
            //{
            //    if (item.Value == ticket.TicketPriorityId.ToString())
            //        item.Selected = true;
            //}
            var x = TicketPriorityId.SelectedValue;
            ViewBag.TicketPriorityId = TicketPriorityId;

            //SELECTLIST - code to where the first option is the one that is currently the selection
            //Maybe make a new list and have the first item added be the selected item

            if (ticket == null)
            {
                return HttpNotFound();
            }


            return View(ticketsViewModel);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles ="Admin, Project Manager, Developer, Admin Demo, Project Manager Demo, Developer Demo")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTicket([Bind(Include = "Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId")] TicketViewModel ticketViewModel)
        {

            //ticket is declared before ModelState.IsValid because we will be returning to the
            //ticket details screen whether the ModelState check succeeds

            Ticket ticket = db.Tickets.Find(ticketViewModel.Id);
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);

            if (ModelState.IsValid)
            {
                
                //var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                //List<string[]> updatedFields = new List<string[]>();
                //string[] propertyUpdate = new string[3];

                List<TicketHistory> ticketChanges = new List<TicketHistory>();

                if (ticketViewModel.Title != ticket.Title)
                    ticketChanges.Add(TicketHistoryUpdater.New(db, ticket, currentUserId, "Title", ticket.Title, ticketViewModel.Title));

                if (ticketViewModel.Description != ticket.Description)
                    ticketChanges.Add(TicketHistoryUpdater.New(db, ticket, currentUserId, "Description", ticket.Description, ticketViewModel.Description));

                if (ticketViewModel.TicketTypeId != ticket.TicketTypeId)
                    ticketChanges.Add(TicketHistoryUpdater.New(db, ticket, currentUserId, "TicketTypeId", ticket.TicketTypeId, ticketViewModel.TicketTypeId));

                if (ticketViewModel.TicketStatusId != ticket.TicketStatusId)
                    ticketChanges.Add(TicketHistoryUpdater.New(db, ticket, currentUserId, "TicketStatusId", ticket.TicketStatusId, ticketViewModel.TicketStatusId));

                if (ticketViewModel.TicketPriorityId != ticket.TicketPriorityId)
                    ticketChanges.Add(TicketHistoryUpdater.New(db, ticket, currentUserId, "TicketPriorityId", ticket.TicketPriorityId, ticketViewModel.TicketPriorityId));

                ticket.Title = ticketViewModel.Title;
                ticket.Description = ticketViewModel.Description;
                ticket.TicketTypeId = ticketViewModel.TicketTypeId;
                ticket.TicketPriorityId = ticketViewModel.TicketPriorityId;
                ticket.TicketStatusId = ticketViewModel.TicketStatusId;
                ticket.Updated = DateTimeOffset.Now;

                db.Entry(ticket).State = EntityState.Modified;

                foreach (var item in ticketChanges)
                    db.TicketHistories.Add(item);

                db.SaveChanges();

                if (ticket.AssignedToUserId != null)
                {
                    ApplicationUser dev = db.Users.Find(ticket.AssignedToUserId);

                    SendGridMessage myMessage = new SendGridMessage();

                    myMessage.AddTo(dev.Email);
                    myMessage.From = new MailAddress("andywonderly@gmail.com");
                    myMessage.Subject = ("A project to which you are assigned has been modified.");
                    myMessage.Html = "Your assigned project " + ticket.TicketProject.Name + " has had one or more fields modified by user " + currentUser.DisplayName + ".";
                    var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                    transportWeb.DeliverAsync(myMessage);
                }

                /*
                if (ticketViewModel.Description != ticket.Description)
                    TicketHistoryUpdater.Update(db, ticket, currentUserId, "Description");

                if (ticketViewModel.Title != ticket.Title)
                    TicketHistoryUpdater.Update(db, ticket, currentUserId, "Title");

                if (ticketViewModel.TicketTypeId != ticket.TicketTypeId)
                    TicketHistoryUpdater.Update(db, ticket, currentUserId, "TicketTypeId");

                if (ticketViewModel.TicketStatusId != ticket.TicketStatusId)
                    TicketHistoryUpdater.Update(db, ticket, currentUserId, "TicketStatusId");

                if (ticketViewModel.TicketPriorityId != ticket.TicketPriorityId)
                    TicketHistoryUpdater.Update(db, ticket, currentUserId, "TicketPriorityId");
                */

                //return RedirectToAction("Index");
            }

            return RedirectToAction("Details", new { id = ticket.Id });
        }

        // GET: Tickets/Delete/5
        [Authorize(Roles = "Admin, Project Manager, Developer, Admin Demo, Project Manager Demo, Developer Demo")]
        public ActionResult DeleteTicket(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket tickets = db.Tickets.Find(id);
            if (tickets == null)
            {
                return HttpNotFound();
            }
            return View(tickets);
        }

        // POST: Tickets/Delete/5
        [Authorize(Roles = "Admin, Project Manager, Developer")]
        [HttpPost, ActionName("DeleteTicket")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Ticket tickets = db.Tickets.Find(id);
            db.Tickets.Remove(tickets);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //GET: Tickets/AssignTicket
        [Authorize(Roles ="Admin, Project Manager, Admin Demo, Project Manager Demo")]
        public ActionResult AssignTicket(int id)
        {
            Ticket ticket = db.Tickets.Find(id);
            var currentUser = db.Users.Find(System.Web.HttpContext.Current.User.Identity.GetUserId());
            UserRolesHelper userRolesHelper = new UserRolesHelper(db);

            bool currentUserIsOnProject = false;

            //If current user != Admin, check to see if current user is on the project.
            //Theoretically, this check should never fail because PMs viewing this page
            //should only be able to see projects that they are on.
            if (!userRolesHelper.IsUserInRole(currentUser.Id, "Admin"))
            {
                foreach (var item in ticket.TicketProject.ProjectUsers)
                    if (item.Id == currentUser.Id)
                        currentUserIsOnProject = true;
            }

            //If the current user is non-Admin and they were not found to be on the project in the previous
            //if/foreach loop, then kick back to Index
            if (!currentUserIsOnProject && !userRolesHelper.IsUserInRole(currentUser.Id, "Admin"))
                return RedirectToAction("Index");

            bool projectHasAtLeastOneDeveloper = false;

            foreach (var item in ticket.TicketProject.ProjectUsers) //Cycle through users, add any developers to ProjectDevelopers
            {
                if (userRolesHelper.IsUserInRole(item.Id, "Developer"))
                {
                    //ticket.TicketProject.ProjectDevelopers.Add(item);
                    ticket.TicketProject.ProjectDeveloperId = item.Id;
                    projectHasAtLeastOneDeveloper = true;
                }
            }

            

            if (!projectHasAtLeastOneDeveloper) //If no PMs were found, kick back to Index
                return RedirectToAction("Index");

            AssignTicketViewModel assignTicketViewModel = new AssignTicketViewModel();
            assignTicketViewModel.TicketId = id;

            //***FOLLOWING 2 LINES COMMENTED OUT TO TRY TO RUN SOMETHING ELSE
            //assignTicketViewModel.ProjectDevelopers = ticket.TicketProject.ProjectDevelopers;

            ProjectsHelper projectsHelper = new ProjectsHelper();

            ICollection<ApplicationUser> projectDevs = new List<ApplicationUser>();
            List<ApplicationUser> allUsers = db.Users.ToList();
            foreach(var item in allUsers)
            {
                if (userRolesHelper.IsUserInRole(item.Id, "Developer") && projectsHelper.IsUserOnProject(item.Id, ticket.TicketProject.Id))
                    projectDevs.Add(item);
            }

            ViewBag.AssignedToUserId = new SelectList(projectDevs, "Id", "DisplayName");

            //Copy ticket project Devs to view model project Devs
            //foreach (var item in ticket.TicketProject.ProjectDevelopers)
            //    assignTicketViewModel.ProjectDevelopers.Add(item);

            //"Selected" is not currently a part of the controller portion of the selectList, so the following
            //if statement not have an effect.

            if (ticket.AssignedToUserId != null) // If a dev was already assigned, make it the selected list item
                assignTicketViewModel.selected = db.Users.Find(ticket.AssignedToUserId).DisplayName;

            //foreach(var item in ticket.TicketProject.Users) //If the current user is a project user, return
            //the view model
            //{
            //if (item.Id == currentUser.Id)

            assignTicketViewModel.TicketStatusId = ticket.TicketStatusId;
            
                    
            return View(assignTicketViewModel);
            //}

            //ViewBag.ProjectManagers = new SelectList(ticket.TicketProject.Users.Where(r => r.Roles), "Id", "Name");

            //return RedirectToAction("Index"); //You should only get here if you're not on the ticket's project
        }

        [Authorize(Roles = "Admin, Project Manager, Admin Demo, Project Manager Demo")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignTicket([Bind(Include = "selected,AssignedToUserId,TicketStatusId,TicketId")] AssignTicketViewModel ticket)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            var currentUser = db.Users.Find(currentUserId);
            Ticket ticketToAssign = db.Tickets.Find(ticket.TicketId);

            //If the incoming AssignedToUserId is different, update the history
            if (ticketToAssign.AssignedToUserId != ticket.AssignedToUserId)
                TicketHistoryUpdater.Update(db, ticketToAssign, currentUserId, "AssignedToUserId",
                                            ticketToAssign.AssignedToUserId, ticket.AssignedToUserId);

            //Update the ticket itself
            ticketToAssign.AssignedToUserId = ticket.AssignedToUserId;

            if (ModelState.IsValid)
            {
                //Update the history before we change the ticket status
                if (ticketToAssign.TicketStatusId != "2")
                    TicketHistoryUpdater.Update(db, ticketToAssign, currentUserId, "TicketStatusId",
                                                ticketToAssign.TicketStatusId, "2");

                ticketToAssign.TicketStatusId = "2";
                db.Tickets.Attach(ticketToAssign);
                db.Entry(ticketToAssign).Property("TicketStatusId").IsModified = true;
                db.Entry(ticketToAssign).Property("AssignedToUserId").IsModified = true;
                db.SaveChanges();

                ApplicationUser dev = db.Users.Find(ticket.AssignedToUserId);

                SendGridMessage myMessage = new SendGridMessage();

                myMessage.AddTo(dev.Email);
                myMessage.From = new MailAddress("andywonderly@gmail.com");
                myMessage.Subject = ("You have been assigned to a project.");
                myMessage.Html = "You have been assigned to the project " + ticketToAssign.TicketProject.Name + " by project manager " + currentUser.DisplayName + ".";
                var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                transportWeb.DeliverAsync(myMessage);

            }
            return RedirectToAction("Index");
        }

        //GET: CreateComment
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        public ActionResult TicketComment()
        {
            return View();
        }

        //POST:  CreateComment
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        [HttpPost]
        public ActionResult TicketComment([Bind(Include = "Id,CommentId,Body,TicketId")] TicketComment comment)
        {
            var userRolesHelper = new UserRolesHelper(db);
            var projectUsersHelper = new ProjectUsersHelper();
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
            ApplicationUser currentUser = db.Users.Find(currentUserId);
            var ticketOwnerId = db.Tickets.Find(comment.TicketId).OwnerUserId;
            var assignedToUserId = db.Tickets.Find(comment.TicketId).AssignedToUserId;
            var ticket = db.Tickets.Find(comment.TicketId);
            //Ticket comment permission checks:

            //If the user is a submitter only, and their Id doesn't match the ticket owner Id, kick back.
            if ((userRolesHelper.IsUserInRole(currentUserId, "Submitter"))
                && (!userRolesHelper.IsUserInRole(currentUserId, "Developer")
                && !userRolesHelper.IsUserInRole(currentUserId, "Project Manager")
                && !userRolesHelper.IsUserInRole(currentUserId, "Admin")))
                    if (ticketOwnerId.ToString() != currentUserId)
                        return RedirectToAction("Details", new { id = comment.TicketId });

            //If user is a PM and not an admin, and they are not on the project, kick back.
            if (userRolesHelper.IsUserInRole(currentUserId, "Project Manager") && !userRolesHelper.IsUserInRole(currentUserId, "Admin"))
            {
                var projectIdInt = 0;
                Int32.TryParse(ticket.ProjectId, out projectIdInt);

                if (!projectUsersHelper.IsUserOnProject(projectIdInt, currentUserId))
                    return RedirectToAction("Details", new { id = comment.TicketId });
            }

            //If user is a developer and not an admin or PM, and their Id doesn't match the ticket assigned id,
            //kick back.
            if (userRolesHelper.IsUserInRole(currentUserId, "Developer")
                && !userRolesHelper.IsUserInRole(currentUserId, "Admin")
                && !userRolesHelper.IsUserInRole(currentUserId, "Project Manager"))
                    if (assignedToUserId != currentUserId)
                        return RedirectToAction("Details", new { id = comment.TicketId });

            if (ModelState.IsValid)
            {
                //var ticket = db.Tickets.Find(comment.TicketId);

                //Make sure comment box isn't empty
                if (string.IsNullOrWhiteSpace(comment.Body))
                {
                    return RedirectToAction("Details", new { id = comment.TicketId });
                }

                //Limit comment to 1000 characters
                if(comment.Body.Length > 1000)
                {
                    return RedirectToAction("Details", new { id = comment.TicketId });
                }

                comment.UserId = User.Identity.GetUserId();
                comment.UserDisplayName = db.Users.Find(comment.UserId).DisplayName;
                comment.Created = DateTimeOffset.Now;
                comment.TicketId = ticket.Id;
                db.TicketComments.Add(comment);
                db.SaveChanges();

                TicketHistoryUpdater.Update(db, comment.Ticket, currentUserId, "New comment");

                if (assignedToUserId != null)
                {
                    ApplicationUser dev = db.Users.Find(ticket.AssignedToUserId);

                    SendGridMessage myMessage = new SendGridMessage();

                    myMessage.AddTo(dev.Email);
                    myMessage.From = new MailAddress("andywonderly@gmail.com");
                    myMessage.Subject = ("A project to which you are assigned has received a comment.");
                    myMessage.Html = "Your assigned project " + ticket.TicketProject.Name + " has received the comment " + comment.Body + " from user " + currentUser.DisplayName + ".";
                    var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                    transportWeb.DeliverAsync(myMessage);
                }
            }

            var ticket2 = db.Tickets.Find(comment.TicketId);
            return RedirectToAction("Details", new { id = comment.TicketId });
        }

        //GET: TicketAttachment
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        public ActionResult TicketAttachment()
        {


            return View();
        }

        //POST:  TicketAttachment
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter, Admin Demo, Project Manager Demo, Developer Demo, Submitter Demo")]
        [HttpPost]
        public ActionResult TicketAttachment([Bind(Include = "Id,TicketId,FilePath,Description")] TicketAttachment attachment, HttpPostedFileBase file)
        {
            //Permissions code copied from ticket comments

            if (ModelState.IsValid)
            {

                var x = attachment.TicketId;
                var userRolesHelper = new UserRolesHelper(db);
                var projectUsersHelper = new ProjectUsersHelper();
                var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                ApplicationUser currentUser = db.Users.Find(currentUserId);

                var assignedToUserId = db.Tickets.Find(attachment.TicketId).AssignedToUserId;
                var ticket = db.Tickets.Find(attachment.TicketId);

                var ticketOwnerId = "";

                if(db.Tickets.Find(attachment.TicketId).OwnerUserId != null)
                    ticketOwnerId = db.Tickets.Find(attachment.TicketId).OwnerUserId;


            //Ticket comment permission checks:

            //If the user is a submitter only, and their Id doesn't match the ticket owner Id, kick back.
            if ((userRolesHelper.IsUserInRole(currentUserId, "Submitter"))
                && (!userRolesHelper.IsUserInRole(currentUserId, "Developer")
                && !userRolesHelper.IsUserInRole(currentUserId, "Project Manager")
                && !userRolesHelper.IsUserInRole(currentUserId, "Admin")))
                
                if (ticketOwnerId.ToString() != currentUserId)
                    return RedirectToAction("Details", new { id = attachment.TicketId });

            //If user is a PM and not an admin, and they are not on the project, kick back.
            if (userRolesHelper.IsUserInRole(currentUserId, "Project Manager") && !userRolesHelper.IsUserInRole(currentUserId, "Admin"))
            {
                var projectIdInt = 0;
                Int32.TryParse(ticket.ProjectId, out projectIdInt);
                if (!projectUsersHelper.IsUserOnProject(projectIdInt, currentUserId))
                    return RedirectToAction("Details", new { id = attachment.TicketId });
            }

            //If user is a developer and not an admin or PM, and their Id doesn't match the ticket assigned id,
            //kick back.
            if (userRolesHelper.IsUserInRole(currentUserId, "Developer")
                && !userRolesHelper.IsUserInRole(currentUserId, "Admin")
                && !userRolesHelper.IsUserInRole(currentUserId, "Project Manager"))
                if (assignedToUserId != currentUserId)
                    return RedirectToAction("Details", new { id = attachment.TicketId });

            

            
                if (UploadValidator.ValidateUpload(file))
                {
                    
                    var fileName = Path.GetFileName(file.FileName);
                    file.SaveAs(Path.Combine(Server.MapPath("~/Uploads"), fileName));
                    attachment.FileUrl = "~/Uploads/" + fileName;

                    attachment.FileName = fileName;
                    attachment.Created = DateTimeOffset.Now;
                    attachment.UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();
                    attachment.UploaderDisplayName = db.Users.Find(attachment.UserId).DisplayName;
                    //The attachment's TicketId should be passed through the view
                    db.TicketAttachments.Add(attachment);
                    db.SaveChanges();

                    TicketHistoryUpdater.Update(db, attachment.Ticket, currentUserId, "New attachment");

                    if (assignedToUserId != null)
                    {
                        ApplicationUser dev = db.Users.Find(ticket.AssignedToUserId);

                        SendGridMessage myMessage = new SendGridMessage();

                        myMessage.AddTo(dev.Email);
                        myMessage.From = new MailAddress("andywonderly@gmail.com");
                        myMessage.Subject = ("A project to which you are assigned has received an attachment upload.");
                        myMessage.Html = "Your assigned project " + ticket.TicketProject.Name + " has received the upload " + file.FileName + " from user " + currentUser.DisplayName +".";
                        var transportWeb = new Web(ConfigurationManager.AppSettings["SendGridAPIKey"]);
                        transportWeb.DeliverAsync(myMessage);
                    }

                }
                else
                {
                    ViewBag.SystemMessage = "Upload invalid.";
                    //Ticket ticket = db.Tickets.Find(attachment.TicketId);
                    
                    TicketHistoryUpdater.Update(db, ticket, currentUserId, "Invalid upload attempt");
                    return View(ticket);
                }
            }


            
            return RedirectToAction("Details", new { id = attachment.TicketId });
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
