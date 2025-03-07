using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussionForum.Data;
using DiscussionForum.Models;

namespace DiscussionForum.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // displays the list of discussions with relevant details like title, date, comment count, and author.
        public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussions
                .Select(d => new DiscussionViewModel
                {
                    DiscussionId = d.DiscussionId,
                    Title = d.Title,
                    CreateDate = d.CreateDate,
                    ImageFileName = d.ImageFileName,
                    // counts the number of comments associated with each discussion.
                    CommentCount = _context.Comments.Count(c => c.DiscussionId == d.DiscussionId),
                    Author = d.Author
                })
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();

            return View(discussions);
        }

        // displays a specific discussion along with its associated comments.
        public async Task<IActionResult> GetDiscussion(int id)
        {
            var discussion = await _context.Discussions
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(d => d.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            return View(discussion);
        }

        // handles errors and returns the error view.
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // viewModel to hold discussion details for display in the Index view.
    public class DiscussionViewModel
    {
        public int DiscussionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; }
        public string? ImageFileName { get; set; }
        // holds the count of comments for each discussion.
        public int CommentCount { get; set; }
        public string Author { get; set; } = string.Empty;
    }
}
