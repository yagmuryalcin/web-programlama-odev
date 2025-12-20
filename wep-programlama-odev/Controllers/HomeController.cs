using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wep_programlama_odev.Data;
using wep_programlama_odev.Models;

// ✅ TaskStatus çakışmasını çözmek için alias
using TaskStatusEnum = wep_programlama_odev.Models.TaskStatus;

namespace wep_programlama_odev.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // ✅ PROJE sayıları (Status'a göre)
            var projeToplam = await _context.Projects.CountAsync();
            var projeBaslanmadi = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Baslanmadi);
            var projeBeklemede = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Beklemede);
            var projeDevamEdiyor = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.DevamEdiyor);
            var projeTamamlandi = await _context.Projects.CountAsync(p => p.Status == ProjectStatus.Tamamlandi);

            // ✅ GÖREV sayıları (Status'a göre) - Türkçe enum isimleri
            var gorevToplam = await _context.TaskItems.CountAsync();
            var gorevBaslanmadi = await _context.TaskItems.CountAsync(t => t.Status == TaskStatusEnum.Baslanmadi);
            var gorevBeklemede = await _context.TaskItems.CountAsync(t => t.Status == TaskStatusEnum.Beklemede);
            var gorevDevamEdiyor = await _context.TaskItems.CountAsync(t => t.Status == TaskStatusEnum.DevamEdiyor);
            var gorevTamamlandi = await _context.TaskItems.CountAsync(t => t.Status == TaskStatusEnum.Tamamlandi);

            var model = new DashboardViewModel
            {
                ProjeBaslanmadi = projeBaslanmadi,
                ProjeBeklemede = projeBeklemede,
                ProjeDevamEdiyor = projeDevamEdiyor,
                ProjeTamamlandi = projeTamamlandi,
                ProjeToplam = projeToplam,

                GorevBeklemede = gorevBeklemede,
                GorevDevamEdiyor = gorevDevamEdiyor,
                GorevTamamlandi = gorevTamamlandi,
                GorevToplam = gorevToplam
            };

            // ✅ DashboardViewModel içinde GorevBaslanmadi varsa set et (yoksa hata verme)
            typeof(DashboardViewModel)
                .GetProperty("GorevBaslanmadi")
                ?.SetValue(model, gorevBaslanmadi);

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
