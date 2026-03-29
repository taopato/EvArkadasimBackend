// Domain/Entities/VerificationCode.cs
using System;

namespace Domain.Entities
{
    public class VerificationCode
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }
    }
}
