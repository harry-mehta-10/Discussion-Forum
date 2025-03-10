using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscussionForum.Models
{
    public class Comment
    {
        public int CommentId { get; set; }

        [Required(ErrorMessage = "Comment content is required")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreateDate { get; set; } = DateTime.Now;

        [Required]
        public int DiscussionId { get; set; }

        // Keep Author for backward compatibility or display purposes
        public string Author { get; set; } = "Anonymous";

        // Make sure this is nullable
        public string? ApplicationUserId { get; set; }

        // Make sure navigation properties are nullable
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }

        public virtual Discussion Discussion { get; set; } = null!;
    }
}