using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [StringLength(100)]
        public string? Author { get; set; } 

        [StringLength(50)]
        public string? Category { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}