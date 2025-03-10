using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussionForum.Data;
using DiscussionForum.Models;
using Microsoft.AspNetCore.Identity;

namespace DiscussionForum.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays the list of discussions with relevant details like title, date, comment count, and author.
        public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussions
                .Include(d => d.User)
                .Select(d => new DiscussionViewModel
                {
                    DiscussionId = d.DiscussionId,
                    Title = d.Title,
                    CreateDate = d.CreateDate,
                    ImageFileName = d.ImageFileName,
                    // Counts the number of comments associated with each discussion.
                    CommentCount = _context.Comments.Count(c => c.DiscussionId == d.DiscussionId),
                    Author = d.Author ?? "Anonymous",  // Default to "Anonymous" if Author is null
                    ApplicationUserId = d.ApplicationUserId,
                    UserName = d.User != null ? d.User.Name : null,
                    UserImageFilename = d.User != null ? d.User.ImageFilename : null
                })
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();

            return View(discussions);
        }

        // Displays a specific discussion along with its associated comments.
        public async Task<IActionResult> GetDiscussion(int id)
        {
            try
            {
                var discussion = await _context.Discussions
                    .Include(d => d.User)
                    .Include(d => d.Comments)
                        .ThenInclude(c => c.User)
                    .FirstOrDefaultAsync(d => d.DiscussionId == id);

                if (discussion == null)
                {
                    return NotFound();
                }

                // Make sure comments are ordered
                discussion.Comments = discussion.Comments.OrderBy(c => c.CreateDate).ToList();

                return View(discussion);
            }
            catch (Exception ex)
            {
                // Log the exception
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // Handles errors and returns the error view.
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // ViewModel to hold discussion details for display in the Index view.
    public class DiscussionViewModel
    {
        public int DiscussionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public string? ImageFileName { get; set; }
        // Holds the count of comments for each discussion.
        public int CommentCount { get; set; }
        public string Author { get; set; } = string.Empty;  // Default to empty string

        // User related information
        public string? ApplicationUserId { get; set; }
        public string? UserName { get; set; }
        public string? UserImageFilename { get; set; }
    }
}