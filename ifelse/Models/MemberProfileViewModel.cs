using System.ComponentModel.DataAnnotations;

namespace ifelse.Models
{
    public class MemberProfileViewModel
    {
        public int UserId { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        public string? Email { get; set; }
    }
}