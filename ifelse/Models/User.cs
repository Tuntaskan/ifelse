using System;

namespace ifelse.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string FullName { get; set; }

        public string Username { get; set; }

        public string PasswordHash { get; set; }

        public string? Phone { get; set; }

        public string? Email { get; set; }

        public int RoleId { get; set; }

        public bool? IsMember { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? Status { get; set; }
    }
}