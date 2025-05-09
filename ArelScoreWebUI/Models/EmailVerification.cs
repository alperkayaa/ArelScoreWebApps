using System.ComponentModel.DataAnnotations;

namespace ArelScoreWebUI.Models
{
    public class EmailVerification
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string VerificationCode { get; set; } = null!;
        public DateTime Expiration { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}
