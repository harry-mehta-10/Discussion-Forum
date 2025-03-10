using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DiscussionForum.Data;
using DiscussionForum.Models;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace DiscussionForum.Controllers
{
    [Authorize] // Add this attribute to restrict entire controller to authenticated users
    public class DiscussionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager; // Add UserManager

        public DiscussionsController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: Discussions/Index - List all discussions
        public async Task<IActionResult> Index()
        {
            var discussions = await _context.Discussions
                .Include(d => d.Comments)
                .Include(d => d.User) // Include the User navigation property
                .OrderByDescending(d => d.CreateDate)
                .ToListAsync();
            return View(discussions);
        }

        // GET: Discussions/Details/5 - View a specific discussion
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions
                .Include(d => d.Comments)
                .Include(d => d.User) // Include User
                .FirstOrDefaultAsync(m => m.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            // Order comments and include their Users
            discussion.Comments = discussion.Comments
                .OrderBy(c => c.CreateDate)
                .ToList();

            return View(discussion);
        }

        // GET: Discussions/Create - Display create form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Discussions/Create - Create a new discussion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,Category")] Discussion discussion, IFormFile? imageFile)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Get current user
                    var user = await _userManager.GetUserAsync(User);

                    discussion.CreateDate = DateTime.Now;
                    discussion.Author = user.Name; // Set the Author to the user's Name
                    discussion.ApplicationUserId = user.Id; // Set the ApplicationUserId

                    // Handle image upload
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

        // GET: Discussions/Edit/5 - Display edit form
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

            // Check if current user is the owner
            var currentUserId = _userManager.GetUserId(User);
            if (discussion.ApplicationUserId != currentUserId)
            {
                return Forbid(); // Return 403 Forbidden if user is not the owner
            }

            return View(discussion);
        }

        // POST: Discussions/Edit/5 - Update a discussion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DiscussionId,Title,Content,Category")] Discussion discussion)
        {
            if (id != discussion.DiscussionId)
            {
                return NotFound();
            }

            // Get the existing discussion to check ownership
            var existingDiscussion = await _context.Discussions.FindAsync(id);
            if (existingDiscussion == null)
            {
                return NotFound();
            }

            // Check if current user is the owner
            var currentUserId = _userManager.GetUserId(User);
            if (existingDiscussion.ApplicationUserId != currentUserId)
            {
                return Forbid(); // Return 403 Forbidden if user is not the owner
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update only allowed fields
                    existingDiscussion.Title = discussion.Title;
                    existingDiscussion.Content = discussion.Content;
                    existingDiscussion.Category = discussion.Category;

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

        // GET: Discussions/Delete/5 - Display delete confirmation page
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var discussion = await _context.Discussions
                .FirstOrDefaultAsync(m => m.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            // Check if current user is the owner
            var currentUserId = _userManager.GetUserId(User);
            if (discussion.ApplicationUserId != currentUserId)
            {
                return Forbid(); // Return 403 Forbidden if user is not the owner
            }

            return View(discussion);
        }

        // POST: Discussions/Delete/5 - Delete a discussion
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discussion = await _context.Discussions
                .Include(d => d.Comments)
                .FirstOrDefaultAsync(d => d.DiscussionId == id);

            if (discussion == null)
            {
                return NotFound();
            }

            // Check if current user is the owner
            var currentUserId = _userManager.GetUserId(User);
            if (discussion.ApplicationUserId != currentUserId)
            {
                return Forbid(); // Return 403 Forbidden if user is not the owner
            }

            // Remove associated comments and the discussion
            _context.Comments.RemoveRange(discussion.Comments);
            _context.Discussions.Remove(discussion);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Check if a discussion exists
        private bool DiscussionExists(int id)
        {
            return _context.Discussions.Any(e => e.DiscussionId == id);
        }

        // POST: Discussions/CreateComment - Create a new comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateComment(int id, string content)
        {
            if (ModelState.IsValid)
            {
                // Get current user
                var user = await _userManager.GetUserAsync(User);

                var comment = new Comment
                {
                    Content = content,
                    DiscussionId = id,
                    Author = user.Name, // Set Author to user's name
                    ApplicationUserId = user.Id, // Set ApplicationUserId
                    CreateDate = DateTime.Now
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { id });
            }

            return RedirectToAction("Details", new { id });
        }
    }
}