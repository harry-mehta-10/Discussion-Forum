using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussionForum.Data;
using DiscussionForum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DiscussionForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Add this attribute to restrict API access to authenticated users
    public class CommentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Add UserManager

        public CommentsApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/CommentsApi/GetDiscussionComments/5
        [HttpGet("GetDiscussionComments/{discussionId}")]
        public async Task<IActionResult> GetDiscussionComments(int discussionId)
        {
            var comments = await _context.Comments
                .Where(c => c.DiscussionId == discussionId)
                .OrderByDescending(c => c.CreateDate)
                .Select(c => new {
                    c.CommentId,
                    c.Content,
                    c.Author,
                    c.ApplicationUserId, // Include user ID
                    CreateDate = c.CreateDate.ToString("MMM dd, yyyy HH:mm"),
                    IsOwner = c.ApplicationUserId == _userManager.GetUserId(User) // Add property to check if current user is owner
                })
                .ToListAsync();
            return Ok(comments);
        }

        public class CommentCreateDto
        {
            public string Content { get; set; } = string.Empty;
            public int DiscussionId { get; set; }
        }

        // POST: api/CommentsApi/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CommentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Get current user
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                var comment = new Comment
                {
                    Content = dto.Content,
                    DiscussionId = dto.DiscussionId,
                    CreateDate = DateTime.Now,
                    Author = user.Name, // Set Author to user's name
                    ApplicationUserId = user.Id // Set ApplicationUserId
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    commentId = comment.CommentId,
                    message = "Comment created successfully",
                    author = user.Name,
                    createDate = comment.CreateDate.ToString("MMM dd, yyyy HH:mm")
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Error creating comment",
                    error = ex.Message
                });
            }
        }

        // DELETE: api/CommentsApi/Delete/5
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            // Check if current user is the owner
            var currentUserId = _userManager.GetUserId(User);
            if (comment.ApplicationUserId != currentUserId)
            {
                return Forbid(); // Return 403 Forbidden if user is not the owner
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Comment deleted successfully" });
        }
    }
}