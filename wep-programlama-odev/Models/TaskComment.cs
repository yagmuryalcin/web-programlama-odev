using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace wep_programlama_odev.Models
{
    public class TaskComment
    {
        public int Id { get; set; }

        // ✅ TEK ve NET Foreign Key
        [Required]
        public int TaskItemId { get; set; }

        // ✅ Navigation
        public TaskItem TaskItem { get; set; } = null!;

        // ✅ Yorumu yazan kullanıcı
        [Required]
        public string UserId { get; set; } = string.Empty;

        public IdentityUser User { get; set; } = null!;

        [Required]
        [StringLength(1000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
