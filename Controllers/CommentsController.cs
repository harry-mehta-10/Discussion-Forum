using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussionForum.Data;
using DiscussionForum.Models;

namespace DiscussionForum.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // displays the view to create a comment for a specific discussion.
        public IActionResult Create(int discussionId)
        {
            ViewBag.DiscussionId = discussionId;
            return View();
        }

        // retrieves and displays the comment to be edited.
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

            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // handles the actual edit operation for a comment.
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        // handles comment deletion and redirects to the discussion page.
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            int discussionId = comment.DiscussionId;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("GetDiscussion", "Home", new { id = discussionId });
        }

        // checks if a comment exists in the database.
        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.CommentId == id);
        }
    }
}
