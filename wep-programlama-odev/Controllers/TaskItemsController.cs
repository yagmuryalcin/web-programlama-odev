using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wep_programlama_odev.Data;
using wep_programlama_odev.Models;

namespace wep_programlama_odev.Controllers
{
    public class TaskItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TaskItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TaskItems
                .Include(t => t.Project);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TaskItems/Create
        public IActionResult Create()
        {
            ViewData["ProjectId"] =
                new SelectList(_context.Projects, "Id", "Name");

            ViewData["Status"] =
                new SelectList(Enum.GetValues(typeof(wep_programlama_odev.Models.TaskStatus)));

            return View();
        }

        // POST: TaskItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                taskItem.CreatedAt = DateTime.Now;

                _context.TaskItems.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] =
                new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);

            ViewData["Status"] =
                new SelectList(Enum.GetValues(typeof(wep_programlama_odev.Models.TaskStatus)), taskItem.Status);

            return View(taskItem);
        }

        // GET: TaskItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
                return NotFound();

            ViewData["ProjectId"] =
                new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);

            ViewData["Status"] =
                new SelectList(Enum.GetValues(typeof(wep_programlama_odev.Models.TaskStatus)), taskItem.Status);

            return View(taskItem);
        }

        // POST: TaskItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskItem taskItem)
        {
            if (id != taskItem.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] =
                new SelectList(_context.Projects, "Id", "Name", taskItem.ProjectId);

            ViewData["Status"] =
                new SelectList(Enum.GetValues(typeof(wep_programlama_odev.Models.TaskStatus)), taskItem.Status);

            return View(taskItem);
        }

        // GET: TaskItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var taskItem = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (taskItem == null)
                return NotFound();

            return View(taskItem);
        }

        // POST: TaskItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem != null)
            {
                _context.TaskItems.Remove(taskItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
