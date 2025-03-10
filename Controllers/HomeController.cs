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

        // Displays the list of discussions with relevant details like title, date, comment count, and author.
        public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussions
                .Select(d => new DiscussionViewModel
                {
                    DiscussionId = d.DiscussionId,
                    Title = d.Title,
                    CreateDate = d.CreateDate,
                    ImageFileName = d.ImageFileName,
                    // Counts the number of comments associated with each discussion.
                    CommentCount = _context.Comments.Count(c => c.DiscussionId == d.DiscussionId),
                    Author = d.Author ?? "Anonymous"  // Default to "Anonymous" if Author is null
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
                    .Where(d => d.DiscussionId == id)
                    .Select(d => new
                    {
                        d.DiscussionId,
                        d.Title,
                        d.Content,
                        d.CreateDate,
                        d.ImageFileName,
                        Author = d.Author ?? "Anonymous",  // Default to "Anonymous" if Author is null
                        d.Category,
                        Comments = d.Comments.Select(c => new
                        {
                            c.CommentId,
                            c.Content,
                            c.CreateDate,
                            Author = c.Author ?? "Anonymous"  // Default to "Anonymous" if Comment Author is null
                        }).ToList()
                    })
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (discussion == null)
                {
                    return NotFound();
                }

                // Map the anonymous type to your Discussion model
                var discussionModel = new Discussion
                {
                    DiscussionId = discussion.DiscussionId,
                    Title = discussion.Title,
                    Content = discussion.Content,
                    CreateDate = discussion.CreateDate,
                    ImageFileName = discussion.ImageFileName,
                    Author = discussion.Author,
                    Category = discussion.Category,
                    Comments = discussion.Comments.Select(c => new Comment
                    {
                        CommentId = c.CommentId,
                        Content = c.Content,
                        CreateDate = c.CreateDate,
                        Author = c.Author,
                        DiscussionId = id
                    }).ToList()
                };

                return View(discussionModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                // Consider using a logging framework here
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
    }
}