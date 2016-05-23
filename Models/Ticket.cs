using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;

namespace BugTracker2.Models
{
    public class Ticket
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Updated { get; set; }
        public string ProjectId { get; set; }
        public string TicketTypeId { get; set; }
        public string TicketPriorityId { get; set; }
        public string TicketStatusId { get; set; }
        public string OwnerUserId { get; set; }
        public string AssignedToUserId { get; set; }
        public string TicketAssigneeId { get; set; }
        //public virtual ICollection<ApplicationUser> Users { get; set; }
        //This was reduntant.  List of ticket users not needed.  Just use project users
        //of the project that the ticket is for.
        public virtual ApplicationUser TicketOwner { get; set; }
        //public virtual ApplicationUser TicketAssignee { get; set; }
        public virtual Project TicketProject { get; set; }
        public virtual ICollection<TicketAttachment> TicketAttachments { get; set; }
        //[ForeignKey("Tickets_Id")]
        public virtual ICollection<TicketComment> TicketComments { get; set; }
        public virtual ICollection<TicketHistory> TicketHistories { get; set; }
        public virtual ICollection<TicketNotification> TicketNotifications { get; set; }
        public virtual ICollection<TicketPriority> TicketPriorities { get; set; }
        public virtual ICollection<TicketStatus> TicketStatuses { get; set; }
        public virtual ICollection<TicketType> TicketTypes { get; set; }
        public virtual ICollection<TicketAssignee> TicketAssignees { get; set; }
        public string TicketOwnerId { get; set; }
       

        public class TicketHistoryUpdater
        {

            //The "Update" function is no longer used in my code.  I have kept the code so I can 
            //refer to it in the future.  The "New" function is used instead so that I can call it
            //to create new ticket histories and do a single db.SaveChanges() inside the controller.

            public static bool Update(ApplicationDbContext db, Ticket ticket, string currentUserId, 
                                                   string property, string oldValue, string newValue)
            {
                TicketHistory ticketHistory = new TicketHistory();
                ticketHistory.TicketId = ticket.Id;
                ticketHistory.DateTime = DateTimeOffset.Now;
                ticketHistory.NewValue = newValue;
                ticketHistory.OldValue = oldValue;
                ticketHistory.Property = property;
                ticketHistory.UserId = currentUserId;
                ticketHistory.UserDisplayName = db.Users.Find(currentUserId).DisplayName;

                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                return true;
            }

            public static bool Update( ApplicationDbContext db, Ticket ticket, string currentUserId,
                                                    string property)
            {
                TicketHistory ticketHistory = new TicketHistory();
                ticketHistory.TicketId = ticket.Id;
                ticketHistory.DateTime = DateTimeOffset.Now;
                ticketHistory.Property = property;
                ticketHistory.UserId = currentUserId;
                ticketHistory.UserDisplayName = db.Users.Find(currentUserId).DisplayName;

                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                return true;
            }

            public static TicketHistory New(ApplicationDbContext db, Ticket ticket, string currentUserId,
                                                string property, string oldValue, string newValue)
            {
                TicketHistory ticketHistory = new TicketHistory();
                ticketHistory.TicketId = ticket.Id;
                ticketHistory.DateTime = DateTimeOffset.Now;
                ticketHistory.NewValue = newValue;
                ticketHistory.OldValue = oldValue;
                ticketHistory.Property = property;
                ticketHistory.UserId = currentUserId;
                ticketHistory.UserDisplayName = db.Users.Find(currentUserId).DisplayName;

                return ticketHistory;
            }

            public static TicketHistory New(ApplicationDbContext db, Ticket ticket, string currentUserId,
                                                string property)
            {
                TicketHistory ticketHistory = new TicketHistory();
                ticketHistory.TicketId = ticket.Id;
                ticketHistory.DateTime = DateTimeOffset.Now;
                ticketHistory.Property = property;
                ticketHistory.UserId = currentUserId;
                ticketHistory.UserDisplayName = db.Users.Find(currentUserId).DisplayName;

                return ticketHistory;
            }




        }



        public class UploadValidator
        {
            public static bool ValidateUpload(HttpPostedFileBase file)
            {
                if (file == null)
                    return false;

                //if (file.ContentLength > 2 * 1024 * 1024 || file.ContentLength < 1024)
                if (file.ContentLength > 4 * 1024 * 1024 || file.ContentLength < 4)
                    return false;

                var Extension = file.FileName.Substring(file.FileName.LastIndexOf('.') + 1).ToLower();

                switch(Extension)
                {
                    case "txt":
                    case "rtf":
                    case "pdf":
                    case "zip":
                    case "rar":
                    case "doc":
                    case "docx":
                    case "xls":
                    case "xlsx":
                    case "ppt":
                    case "pptx":
                    case "jpg":
                    case "jpeg":
                    case "png":
                    case "gif":
                    case "tif":
                    case "tiff":
                        return true;
                        break;
                    default:
                        return false;
                        break;
                }

                /*
                
                try
                {
                    using (var img = Image.FromStream(file.InputStream))
                    {
                        return ImageFormat.Jpeg.Equals(img.RawFormat) ||
                               ImageFormat.Png.Equals(img.RawFormat) ||
                               ImageFormat.Gif.Equals(img.RawFormat);
                    }
                }
                catch
                {
                    return false;
                }
                */
            }
        }
    }
}