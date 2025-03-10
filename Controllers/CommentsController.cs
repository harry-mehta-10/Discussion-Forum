using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussionForum.Data;
using DiscussionForum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DiscussionForum.Controllers
{
    [Authorize] // Restrict access to authenticated users
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Displays the view to create a comment for a specific discussion.
        public IActionResult Create(int discussionId)
        {
            ViewBag.DiscussionId = discussionId; // Pass discussion ID to the view
            return View();
        }

        // Handles the actual creation of a comment.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int discussionId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return RedirectToAction("Details", "Discussions", new { id = discussionId });
            }

            // Get current user
            var user = await _userManager.GetUserAsync(User);

            var comment = new Comment
            {
                Content = content,
                DiscussionId = discussionId,
                CreateDate = DateTime.Now,
                Author = user.Name,
                ApplicationUserId = user.Id // Save User ID
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Discussions", new { id = discussionId });
        }

        // Retrieves and displays the comment to be edited.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.Discussion)
                .FirstOrDefaultAsync(c => c.CommentId == id);

            if (comment == null)
            {
                return NotFound();
            }

            // Check if the current user is the owner
            var currentUserId = _userManager.GetUserId(User);
            if (comment.ApplicationUserId != currentUserId)
            {
                return Forbid(); // Return 403 Forbidden if user is not the owner
            }

            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Handles the actual edit operation for a comment.
        public async Task<IActionResult> Edit(int id, [Bind("CommentId,Content,DiscussionId,Author,CreateDate")] Comment comment)
        {
            if (id != comment.CommentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingComment = await _context.Comments.FindAsync(id);
                    if (existingComment == null)
                    {
                        return NotFound();
                    }

                    // Check if the current user is the owner
                    var currentUserId = _userManager.GetUserId(User);
                    if (existingComment.ApplicationUserId != currentUserId)
                    {
                        return Forbid(); // Return 403 Forbidden if user is not the owner
                    }

                    existingComment.Content = comment.Content;
                    await _context.SaveChangesAsync();

                    return RedirectToAction("GetDiscussion", "Home", new { id = comment.DiscussionId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.CommentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(comment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // Handles comment deletion and redirects to the discussion page.
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Check if the current user is the owner
            var currentUserId = _userManager.GetUserId(User);
            if (comment.ApplicationUserId != currentUserId)
            {
                return Forbid(); // Return 403 Forbidden if user is not the owner
            }

            int discussionId = comment.DiscussionId;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Discussions", new { id = discussionId });
        }

        // Checks if a comment exists in the database.
        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}