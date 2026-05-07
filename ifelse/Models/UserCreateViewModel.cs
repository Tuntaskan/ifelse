using System.ComponentModel.DataAnnotations;

namespace ifelse.Models
{
    public class UserCreateViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public bool IsMember { get; set; }
    }
}