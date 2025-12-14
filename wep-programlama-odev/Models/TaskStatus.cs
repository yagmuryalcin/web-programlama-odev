using System.ComponentModel.DataAnnotations;

namespace wep_programlama_odev.Models
{
    public enum TaskStatus
    {
        [Display(Name = "To Do")]
        Todo = 0,

        [Display(Name = "In Progress")]
        InProgress = 1,

        [Display(Name = "Done")]
        Done = 2
    }
}
