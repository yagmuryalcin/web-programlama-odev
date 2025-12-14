using System.ComponentModel.DataAnnotations;

namespace wep_programlama_odev.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Todo;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // FK -> Project
        [Required]
        public int ProjectId { get; set; }

        public Project? Project { get; set; }
    }
}
