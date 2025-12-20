using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace wep_programlama_odev.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();


        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Durum seçiniz.")]
        public TaskStatus Status { get; set; } = TaskStatus.Beklemede;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Proje seçiniz.")]
        public int ProjectId { get; set; }

        // Görev atanan kullanıcı (Team Member)
        public string? AssignedUserId { get; set; }
        public IdentityUser? AssignedUser { get; set; }

        // Navigation
        public Project? Project { get; set; }
    }
}
