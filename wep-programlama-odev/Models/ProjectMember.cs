using Microsoft.AspNetCore.Identity;

namespace wep_programlama_odev.Models
{
    public class ProjectMember
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public string UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }
    }
}
