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

        // ✅ GET: TaskItems (liste)
        // Admin/Manager: hepsini görür
        // TeamMember: sadece kendine atananları görür
        public async Task<IActionResult> Index()
        {
            var query = _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .OrderByDescending(t => t.CreatedAt)
                .AsQueryable();

            if (User.IsInRole("TeamMember"))
            {
                var myId = _userManager.GetUserId(User);
                query = query.Where(t => t.AssignedUserId == myId);
            }

            var taskItems = await query.ToListAsync();
            return View(taskItems);
        }

        // ✅ KANBAN: GET TaskItems/Board
        // Admin/Manager: hepsi
        // TeamMember: sadece kendisi
        public async Task<IActionResult> Board()
        {
            var query = _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .OrderByDescending(t => t.CreatedAt)
                .AsQueryable();

            if (User.IsInRole("TeamMember"))
            {
                var myId = _userManager.GetUserId(User);
                query = query.Where(t => t.AssignedUserId == myId);
            }

            var all = await query.ToListAsync();

            var vm = new KanbanBoardVm
            {
                Beklemede = all.Where(x => x.Status == TaskStatusEnum.Beklemede).ToList(),
                DevamEdiyor = all.Where(x => x.Status == TaskStatusEnum.DevamEdiyor).ToList(),
                Tamamlandi = all.Where(x => x.Status == TaskStatusEnum.Tamamlandi).ToList()
            };

            return View(vm);
        }

        // ✅ KANBAN: Durum Güncelle
        // Admin/Manager: herkesin görevini güncelleyebilir
        // TeamMember: sadece kendi görevi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, TaskStatusEnum status)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
            if (task == null) return NotFound();

            if (User.IsInRole("TeamMember"))
            {
                var myId = _userManager.GetUserId(User);
                if (task.AssignedUserId != myId)
                    return Forbid();
            }

            task.Status = status;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Board));
        }

        // ✅ GET: TaskItems/Details/5
        // TeamMember: sadece kendine atanan göreve girebilir
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

            if (User.IsInRole("TeamMember"))
            {
                var myId = _userManager.GetUserId(User);
                if (taskItem.AssignedUserId != myId)
                    return Forbid();
            }

            var comments = await _context.TaskComments
                .Where(c => c.TaskItemId == taskItem.Id)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Comments = comments;

            return View(taskItem);
        }

        // ✅ GET: TaskItems/Create (SADECE ADMIN)
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

        // ✅ POST: TaskItems/Create (SADECE ADMIN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Description,Status,ProjectId,AssignedUserId")] TaskItem taskItem)
        {
            if (!ModelState.IsValid)
            {
                await FillDropdowns(taskItem.ProjectId, taskItem.AssignedUserId);
                return View(taskItem);
            }

            taskItem.CreatedAt = DateTime.Now;

            // "" gelirse null yap (Atama yok)
            if (string.IsNullOrWhiteSpace(taskItem.AssignedUserId))
                taskItem.AssignedUserId = null;

            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = taskItem.ProjectId });
        }

        // ✅ GET: TaskItems/Edit/5 (SADECE ADMIN)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taskItem = await _context.TaskItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (taskItem == null) return NotFound();

            await FillDropdowns(taskItem.ProjectId, taskItem.AssignedUserId);
            return View(taskItem);
        }

        // ✅ POST: TaskItems/Edit/5 (SADECE ADMIN)
        // 🔥 EN ÖNEMLİ DÜZELTME: _context.Update(taskItem) YOK
        // DB'den entity çekip sadece alanları güncelliyoruz.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Status,ProjectId,AssignedUserId")] TaskItem formModel)
        {
            if (id != formModel.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                await FillDropdowns(formModel.ProjectId, formModel.AssignedUserId);
                return View(formModel);
            }

            var taskInDb = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
            if (taskInDb == null) return NotFound();

            taskInDb.Title = formModel.Title;
            taskInDb.Description = formModel.Description;
            taskInDb.Status = formModel.Status;
            taskInDb.ProjectId = formModel.ProjectId;

            // "" gelirse null yap
            taskInDb.AssignedUserId = string.IsNullOrWhiteSpace(formModel.AssignedUserId)
                ? null
                : formModel.AssignedUserId;

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = taskInDb.ProjectId });
        }

        // ✅ GET: TaskItems/Delete/5 (SADECE ADMIN)
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

        // ✅ POST: TaskItems/Delete/5 (SADECE ADMIN)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == id);
            if (taskItem == null) return RedirectToAction(nameof(Index));

            var projectId = taskItem.ProjectId;

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Projects", new { id = projectId });
        }

        private async Task FillDropdowns(int? selectedProjectId, string? selectedAssignedUserId)
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
                    Text = StatusToText(s)
                })
                .ToList();

            ViewData["Status"] = statusList;

            // ✅ Dropdown'da Value = Id olmalı (kayıt doğru gitsin)
            // Text: Email varsa Email, yoksa UserName
            var teamMembers = await _userManager.GetUsersInRoleAsync("TeamMember");
            var managers = await _userManager.GetUsersInRoleAsync("Manager");

            var assignableUsers = teamMembers
                .Concat(managers)
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .OrderBy(u => u.Email ?? u.UserName)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = string.IsNullOrWhiteSpace(u.Email) ? u.UserName : u.Email
                })
                .ToList();

            // "Atama yok" seçeneği
            assignableUsers.Insert(0, new SelectListItem { Value = "", Text = "— Atama yok —" });

            ViewData["AssignedUserId"] = new SelectList(assignableUsers, "Value", "Text", selectedAssignedUserId);
        }

        private static string StatusToText(TaskStatusEnum s)
        {
            return s switch
            {
                TaskStatusEnum.Beklemede => "Beklemede",
                TaskStatusEnum.DevamEdiyor => "Devam Ediyor",
                TaskStatusEnum.Tamamlandi => "Tamamlandı",
                TaskStatusEnum.Baslanmadi => "Başlanmadı",
                
                _ => s.ToString()
            };
        }
    }

    // ✅ Kanban ViewModel (aynı dosyada dursun şimdilik)
    public class KanbanBoardVm
    {
        public List<TaskItem> Beklemede { get; set; } = new();
        public List<TaskItem> DevamEdiyor { get; set; } = new();
        public List<TaskItem> Tamamlandi { get; set; } = new();
    }
}
