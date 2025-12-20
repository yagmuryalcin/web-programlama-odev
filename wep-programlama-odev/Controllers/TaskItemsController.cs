using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wep_programlama_odev.Data;
using wep_programlama_odev.Models;

// ✅ Çakışmayı çözmek için alias:
using TaskStatusEnum = wep_programlama_odev.Models.TaskStatus;

namespace wep_programlama_odev.Controllers
{
    [Authorize]
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TaskItemsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: TaskItems
        // Admin: hepsini görür
        // Admin değilse: sadece kendine atananları görür
        public async Task<IActionResult> Index()
        {
            var query = _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .OrderByDescending(t => t.CreatedAt)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var myId = _userManager.GetUserId(User);
                query = query.Where(t => t.AssignedUserId == myId);
            }

            var taskItems = await query.ToListAsync();
            return View(taskItems);
        }

        // GET: TaskItems/Details/5
        // Admin: girer
        // Admin değilse: sadece kendine atanan göreve girebilir
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.TaskComments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (taskItem == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var myId = _userManager.GetUserId(User);
                if (taskItem.AssignedUserId != myId)
                    return Forbid();
            }

            // ✅ Eğer view'da ViewBag.Comments kullanıyorsan dursun:
            ViewBag.Comments = taskItem.TaskComments?
                .OrderByDescending(c => c.CreatedAt)
                .ToList() ?? new List<TaskComment>();

            return View(taskItem);
        }

        // GET: TaskItems/Create (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? projectId)
        {
            await FillDropdowns(projectId, null);

            var model = new TaskItem
            {
                ProjectId = projectId ?? 0,
                Status = TaskStatusEnum.Beklemede,
                CreatedAt = DateTime.Now
            };

            return View(model);
        }

        // POST: TaskItems/Create (SADECE ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Status,ProjectId,AssignedUserId")] TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                taskItem.CreatedAt = DateTime.Now;
                _context.Add(taskItem);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Projects", new { id = taskItem.ProjectId });
            }

            await FillDropdowns(taskItem.ProjectId, taskItem.AssignedUserId);
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5 (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null) return NotFound();

            await FillDropdowns(taskItem.ProjectId, taskItem.AssignedUserId);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5 (SADECE ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,ProjectId,CreatedAt,AssignedUserId")] TaskItem taskItem)
        {
            if (id != taskItem.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskItemExists(taskItem.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction("Details", "Projects", new { id = taskItem.ProjectId });
            }

            await FillDropdowns(taskItem.ProjectId, taskItem.AssignedUserId);
            return View(taskItem);
        }

        // GET: TaskItems/Delete/5 (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null) return NotFound();

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5 (SADECE ADMIN)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null) return RedirectToAction(nameof(Index));

            var projectId = taskItem.ProjectId;

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.Id == id);
        }

        // Dropdownlar: Project + Status + AssignedUser
        private async Task FillDropdowns(int? selectedProjectId, string? selectedAssignedUserId)
        {
            var projects = await _context.Projects
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name", selectedProjectId);

            // Status dropdown
            var statusList = Enum.GetValues(typeof(TaskStatusEnum))
                .Cast<TaskStatusEnum>()
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = StatusToText(s)
                })
                .ToList();

            ViewData["Status"] = statusList;

            // ÖNEMLİ: Dropdown boş kalmasın diye iki seçenek var:

            // 1) Sadece TeamMember rolündekiler:
            var teamMembers = await _userManager.GetUsersInRoleAsync("TeamMember");

            // Eğer teamMembers boşsa (rol atanmamış kullanıcılar yüzünden), fallback olarak tüm kullanıcıları göster:
            List<IdentityUser> assignableUsers;
            if (teamMembers != null && teamMembers.Count > 0)
            {
                assignableUsers = teamMembers.ToList();
            }
            else
            {
                assignableUsers = await _userManager.Users
                    .OrderBy(u => u.Email)
                    .ToListAsync();
            }

            // Dropdown’da Email daha anlamlı (istersen UserName yaparız)
            ViewData["AssignedUserId"] = new SelectList(assignableUsers, "Id", "UserName", selectedAssignedUserId);
        }

        private static string StatusToText(TaskStatusEnum s)
        {
            return s switch
            {
                TaskStatusEnum.Beklemede => "Yapılacak",
                TaskStatusEnum.DevamEdiyor => "Devam Ediyor",
                TaskStatusEnum.Tamamlandi => "Tamamlandı",
                _ => s.ToString()
            };
        }
    }
}
