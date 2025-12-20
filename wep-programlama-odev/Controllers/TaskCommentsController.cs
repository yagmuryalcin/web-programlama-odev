using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using wep_programlama_odev.Data;
using wep_programlama_odev.Models;

namespace wep_programlama_odev.Controllers
{
    [Authorize]
    public class TaskCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TaskCommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int taskItemId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["CommentError"] = "Yorum boş olamaz.";
                return RedirectToAction("Details", "TaskItems", new { id = taskItemId });
            }

            var comment = new TaskComment
            {
                TaskItemId = taskItemId,
                Text = text.Trim(),
                UserId = _userManager.GetUserId(User) ?? ""
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "TaskItems", new { id = taskItemId });
        }
    }
}
