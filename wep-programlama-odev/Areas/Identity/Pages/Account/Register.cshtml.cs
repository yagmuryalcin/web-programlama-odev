// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace wep_programlama_odev.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        // Bu sayfayı devre dışı bırakıyoruz (Admin kullanıcı ekleyecek)
        // O yüzden artık buradaki servislerin hiçbirine ihtiyaç yok.

        public Task<IActionResult> OnGetAsync(string returnUrl = null)
        {
            // Register sayfasına geleni Login'e yönlendir
            return Task.FromResult<IActionResult>(RedirectToPage("/Account/Login", new { area = "Identity" }));
        }

        public Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            // Bir şekilde post atılsa bile yine Login'e yönlendir
            return Task.FromResult<IActionResult>(RedirectToPage("/Account/Login", new { area = "Identity" }));
        }
    }
}
