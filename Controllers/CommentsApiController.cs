using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussionForum.Data;
using DiscussionForum.Models;

namespace DiscussionForum.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CommentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // fetches all comments for a specific discussion and orders them by creation date.
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
                    CreateDate = c.CreateDate.ToString("MMM dd, yyyy HH:mm")
                })
                .ToListAsync();

            return Ok(comments);
        }

        public class CommentCreateDto
        {
            public string Content { get; set; } = string.Empty;
            public int DiscussionId { get; set; }
            public string? Author { get; set; }
        }

        // creates a new comment for a specific discussion.
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CommentCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var comment = new Comment
                {
                    Content = dto.Content,
                    DiscussionId = dto.DiscussionId,
                    CreateDate = DateTime.Now,
                    Author = string.IsNullOrWhiteSpace(dto.Author) ? "Anonymous" : dto.Author
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    commentId = comment.CommentId,
                    message = "Comment created successfully"
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
    }
}
