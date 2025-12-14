using System.ComponentModel.DataAnnotations;

namespace wep_programlama_odev.Models
{
    public class Project
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 1 Project -> N Task
        public List<TaskItem> Tasks { get; set; } = new();
    }
}
