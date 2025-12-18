using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wep_programlama_odev.Data;
using wep_programlama_odev.Models;
using Microsoft.AspNetCore.Authorization;

// ✅ Çakışmayı çözmek için alias:
using TaskStatusEnum = wep_programlama_odev.Models.TaskStatus;

namespace wep_programlama_odev.Controllers
{
    // Login olan herkes görebilsin (Index/Details)
    [Authorize]
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskItems  (HER LOGIN OLAN GÖREBİLİR)
        public async Task<IActionResult> Index()
        {
            var taskItems = await _context.TaskItems
                .Include(t => t.Project)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(taskItems);
        }

        // GET: TaskItems/Details/5  (HER LOGIN OLAN GÖREBİLİR)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null) return NotFound();

            return View(taskItem);
        }

        // GET: TaskItems/Create  (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(int? projectId)
        {
            await FillDropdowns(projectId);

            var model = new TaskItem
            {
                ProjectId = projectId ?? 0,
                Status = TaskStatusEnum.Todo,
                CreatedAt = DateTime.Now
            };

            return View(model);
        }

        // POST: TaskItems/Create  (SADECE ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Status,ProjectId")] TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                taskItem.CreatedAt = DateTime.Now;
                _context.Add(taskItem);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", "Projects", new { id = taskItem.ProjectId });
            }

            await FillDropdowns(taskItem.ProjectId);
            return View(taskItem);
        }

        // GET: TaskItems/Edit/5  (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null) return NotFound();

            await FillDropdowns(taskItem.ProjectId);
            return View(taskItem);
        }

        // POST: TaskItems/Edit/5  (SADECE ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,ProjectId,CreatedAt")] TaskItem taskItem)
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

            await FillDropdowns(taskItem.ProjectId);
            return View(taskItem);
        }

        // GET: TaskItems/Delete/5  (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null) return NotFound();

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5  (SADECE ADMIN)
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

        private async Task FillDropdowns(int? selectedProjectId)
        {
            var projects = await _context.Projects
                .OrderBy(p => p.Name)
                .ToListAsync();

            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name", selectedProjectId);

            var statusList = Enum.GetValues(typeof(TaskStatusEnum))
                .Cast<TaskStatusEnum>()
                .Select(s => new SelectListItem
                {
                    Value = s.ToString(),
                    Text = s.ToString()
                })
                .ToList();

            ViewData["Status"] = statusList;
        }
    }
}
