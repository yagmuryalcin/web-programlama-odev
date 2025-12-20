using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using wep_programlama_odev.Models;

namespace wep_programlama_odev.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Users
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var vm = new List<UserListItemVm>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                vm.Add(new UserListItemVm
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Roles = roles.ToList()
                });
            }

            return View(vm);
        }

        // GET: /Users/Create
        public async Task<IActionResult> Create()
        {
            await FillRoles();
            return View(new CreateUserVm());
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVm model)
        {
            await FillRoles(model.Role);

            if (!ModelState.IsValid)
                return View(model);

            // Email var mı kontrol et
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing != null)
            {
                ModelState.AddModelError(string.Empty, "Bu e-posta ile zaten bir kullanıcı var.");
                return View(model);
            }

            var user = new IdentityUser
            {
                UserName = model.Email,   // şimdilik email = username
                Email = model.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                foreach (var err in createResult.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);

                return View(model);
            }

            // Rol ekle
            if (!string.IsNullOrWhiteSpace(model.Role))
            {
                if (!await _roleManager.RoleExistsAsync(model.Role))
                {
                    ModelState.AddModelError(string.Empty, $"Rol bulunamadı: {model.Role}");
                    return View(model);
                }

                var roleResult = await _userManager.AddToRoleAsync(user, model.Role);
                if (!roleResult.Succeeded)
                {
                    foreach (var err in roleResult.Errors)
                        ModelState.AddModelError(string.Empty, err.Description);

                    return View(model);
                }
            }

            TempData["Success"] = "Kullanıcı başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        // ✅ GET: /Users/EditRole/{id}
        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var allRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(x => x)
                .ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);
            var selectedRole = userRoles.FirstOrDefault(); // tek rol mantığı

            var vm = new EditUserRoleVm
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                SelectedRole = selectedRole,
                AllRoles = allRoles
            };

            return View(vm);
        }

        // ✅ POST: /Users/EditRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(EditUserRoleVm model)
        {
            if (!ModelState.IsValid)
            {
                // dropdown boş kalmasın
                model.AllRoles = await _roleManager.Roles
                    .Select(r => r.Name!)
                    .OrderBy(x => x)
                    .ToListAsync();

                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();

            var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

            // kullanıcıdaki eski rolleri temizle (tek rol mantığı)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
            }

            // yeni rolü ekle
            if (!string.IsNullOrWhiteSpace(model.SelectedRole))
            {
                if (!allRoles.Contains(model.SelectedRole))
                {
                    ModelState.AddModelError(nameof(model.SelectedRole), "Seçilen rol geçersiz.");
                    model.AllRoles = allRoles.OrderBy(x => x).ToList();
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, model.SelectedRole);
            }

            TempData["Success"] = "Rol güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task FillRoles(string? selected = null)
        {
            var roles = await _roleManager.Roles
                .Select(r => r.Name)
                .OrderBy(x => x)
                .ToListAsync();

            ViewBag.Roles = new SelectList(roles, selected);
        }
    }
}
