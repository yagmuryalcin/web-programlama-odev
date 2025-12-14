using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace wep_programlama_odev.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Proje adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Proje adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir.")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}
