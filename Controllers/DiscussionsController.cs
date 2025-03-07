using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussionForum.Data;
using DiscussionForum.Models;

namespace DiscussionForum.Controllers
{
    public class DiscussionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DiscussionsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // displays a list of all discussions, ordered by creation date, including their associated comments.
        public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussions
                .Include(d => d.Comments)
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();
            return View(discussions);
        }

        // displays detailed information for a specific discussion, including all comments.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(m => m.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            return View(discussion);
        }

        // displays the view to create a new discussion.
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // handles the creation of a new discussion, including handling image file upload.
        public async Task<IActionResult> Create([Bind("Title,Content,Category,Author")] Discussion discussion, IFormFile? imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    discussion.CreateDate = DateTime.Now;
                    discussion.Author = string.IsNullOrWhiteSpace(discussion.Author) ? "Anonymous" : discussion.Author;

                    // if an image file is provided, validate and save it to the server.
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        Directory.CreateDirectory(uploadsFolder);

                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ImageFile", "Invalid file type. Only jpg, jpeg, png, and gif are allowed.");
                            return View(discussion);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }
                        discussion.ImageFileName = uniqueFileName;
                    }

                    _context.Add(discussion);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating the discussion. Please try again.");
            }
            return View(discussion);
        }

        // displays the view to edit an existing discussion.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions.FindAsync(id);
            if (discussion == null)
            {
                return NotFound();
            }
            return View(discussion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // handles updating an existing discussion's information.
        public async Task<IActionResult> Edit(int id, [Bind("DiscussionId,Title,Content,Category,Author")] Discussion discussion)
        {
            if (id != discussion.DiscussionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDiscussion = await _context.Discussions.FindAsync(id);
                    if (existingDiscussion == null)
                    {
                        return NotFound();
                    }

                    existingDiscussion.Title = discussion.Title;
                    existingDiscussion.Content = discussion.Content;
                    existingDiscussion.Category = discussion.Category;
                    existingDiscussion.Author = discussion.Author;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscussionExists(discussion.DiscussionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(discussion);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        // handles the deletion of a discussion along with its associated comments.
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discussion = await _context.Discussions
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(d => d.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            // removes all associated comments before deleting the discussion.
            _context.Comments.RemoveRange(discussion.Comments);
            _context.Discussions.Remove(discussion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // checks if a discussion exists in the database.
        private bool DiscussionExists(int id)
        {
            return _context.Discussions.Any(e => e.DiscussionId == id);
        }
    }
}
