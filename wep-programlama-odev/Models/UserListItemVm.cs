using System.Collections.Generic;

namespace wep_programlama_odev.Models
{
    public class UserListItemVm
    {
        public string Id { get; set; } = default!;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
