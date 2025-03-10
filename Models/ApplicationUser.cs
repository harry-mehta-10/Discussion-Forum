using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiscussionForum.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Name")]
        public string? Name { get; set; }

        [Display(Name = "Location")]
        public string? Location { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ImageFilename { get; set; }

        [NotMapped]
        [Display(Name = "Upload Profile Picture")]
        public IFormFile? ImageFile { get; set; }

        // Navigation properties
        public virtual ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}