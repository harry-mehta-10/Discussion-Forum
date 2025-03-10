using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscussionForum.Models
{
    public class Discussion
    {
        public int DiscussionId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreateDate { get; set; } = DateTime.Now;

        public string? ImageFileName { get; set; }

        // Keep Author for backward compatibility or display purposes
        [StringLength(100)]
        public string? Author { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        // Make sure this is nullable
        public string? ApplicationUserId { get; set; }

        // Navigation property for the User
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}