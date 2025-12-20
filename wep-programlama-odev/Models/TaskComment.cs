using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace wep_programlama_odev.Models
{
    public class TaskComment
    {
        public int Id { get; set; }

        [Required]
        public int TaskItemId { get; set; }
        public TaskItem? TaskItem { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }

        [Required(ErrorMessage = "Yorum boş olamaz.")]
        [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
