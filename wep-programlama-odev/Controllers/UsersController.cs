using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    Roles = roles.ToList()
                });
            }

            return View(vm);
        }

        // GET: /Users/EditRole/{id}
        public async Task<IActionResult> EditRole(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var allRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(r => r)
                .ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);

            var vm = new EditUserRoleVm
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                AllRoles = allRoles,
                SelectedRole = userRoles.FirstOrDefault() // tek rol mantığıyla
            };

            return View(vm);
        }

        // POST: /Users/EditRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(EditUserRoleVm model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            // Role boşsa hata ver
            if (string.IsNullOrWhiteSpace(model.SelectedRole))
            {
                ModelState.AddModelError("", "Lütfen bir rol seçiniz.");
            }

            if (!ModelState.IsValid)
            {
                model.AllRoles = await _roleManager.Roles.Select(r => r.Name!).OrderBy(r => r).ToListAsync();
                return View(model);
            }

            // Kullanıcının mevcut rollerini sil
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            // Yeni rol ata
            await _userManager.AddToRoleAsync(user, model.SelectedRole!);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Users/Create
        public IActionResult Create()
        {
            return View(new CreateUserVm());
        }

        // POST: /Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError("", e.Description);

                return View(model);
            }

            // Rol seçildiyse ata
            if (!string.IsNullOrWhiteSpace(model.Role))
                await _userManager.AddToRoleAsync(user, model.Role);

            return RedirectToAction(nameof(Index));
        }
    }
}
