﻿using BugTracker2.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using System.Web;
using System.Collections.Generic;
using System.Web.Security;

namespace BugTracker2.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private string AdminDemoId = "6f14712f-952b-4d85-8981-c1fea486113b";
        private string ProjectManagerDemoId = "c95a057d-4bf5-4f87-b498-dc5a7a7fa975";
        private string DeveloperDemoId = "48e9707c-91b6-4d6c-bdec-416caba21fa8";
        private string SubmitterDemoId = "f5969cac-aac0-4e0f-ac4e-1741387eb71f";

        // GET: Admin
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            
            var user = db.Users;
            return View(user);
        }

        //public ActionResult EditUser(string id = "59d21208-39c4-404c-bcca-cb563b2428b3")
        [Authorize(Roles = "Admin")]
        public ActionResult EditUser(string id)
        {

            if (id == null)
            {
                RedirectToAction("Index");
            }



            var user = db.Users.Find(id);
            UserRolesHelper helper = new UserRolesHelper(db);
            var model = new AdminUserViewModel();

            /*
            List<Projects> userProjects = new List<Projects>();

            foreach (var item in user.Projects)
            {
                userProjects.Add(item);
            }
            */

            model.Name = user.DisplayName;
            model.Id = user.Id;
            model.selected = helper.ListUserRoles(id).ToArray();
            model.roles = new MultiSelectList(db.Roles, "Name", "Name", model.selected);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUser([Bind(Include="selected, Id, Name, roles")] AdminUserViewModel model)
        {
            var currentUserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

            if (currentUserId == AdminDemoId || currentUserId == ProjectManagerDemoId || currentUserId == DeveloperDemoId
                || currentUserId == SubmitterDemoId)
            {
                return RedirectToAction("Index");
            }


            if (ModelState.IsValid)
            {
                //Declare variable of the application context
                //var context = HttpContext.GetOwinContext().Get<ApplicationDbContext>();
                var context = this.db;

                //Create new roleManager to allow access to helper functions
                var roleManager = new UserRolesHelper(context);

                //List of roles to be added
                List<string> rolesToAdd = new List<string>();

                //List of roles to be removed.  Starts out as all roles
                //FUTURE UPDATE - get roles from role list rather than typing them all out.
                List<string> rolesToRemove = new List<string> { "Admin", "Developer", "Project Manager", "Submitter" };

                //Add the roles to be added to rolesToAdd list
                foreach (var item in model.selected)
                    rolesToAdd.Add(item);

                //remove roles to be added from rolesToRemove list
                foreach (var item in rolesToAdd)
                {
                    rolesToRemove.Remove(item);
                }

                //Add user to role if user is not in the role already
                foreach (var item in rolesToAdd)
                {
                    if (!roleManager.IsUserInRole(model.Id, item))
                        roleManager.AddUserToRole(model.Id, item);
                }

                //Remove user from role if the role was not selected but they are in the role
                foreach (var item in rolesToRemove)
                {
                    if (roleManager.IsUserInRole(model.Id, item))
                        roleManager.RemoveUserFromRole(model.Id, item);
                }
                
                
                db.SaveChanges();
                
            }

            return RedirectToAction("Index");
        }


    }
}