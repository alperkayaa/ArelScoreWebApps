using System.ComponentModel.DataAnnotations;

namespace ArelScoreWebUI.Models
{
    public class ChangePassword
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [MinLength(8, ErrorMessage = "Yeni şifre en az 8 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre tekrarı gereklidir.")]
        [Compare("NewPassword", ErrorMessage = "Yeni şifreler uyuşmuyor.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }
}
