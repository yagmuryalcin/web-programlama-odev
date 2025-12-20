using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace wep_programlama_odev.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

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

        // ✅ Navigation'lar formdan gelmez -> validation dışı bırak
        [ValidateNever]
        public Project? Project { get; set; }

        public string? AssignedUserId { get; set; }

        [ValidateNever]
        public IdentityUser? AssignedUser { get; set; }

        [ValidateNever]
        public ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
    }
}
