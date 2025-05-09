using System.ComponentModel.DataAnnotations;

namespace ArelScoreWebUI.Models.ViewModel
{
    public class UserLoginViewModel
    {
        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
