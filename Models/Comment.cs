using System;
using System.ComponentModel.DataAnnotations;

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

        public string Author { get; set; } = "Anonymous";

        public virtual Discussion Discussion { get; set; } = null!;
    }
}