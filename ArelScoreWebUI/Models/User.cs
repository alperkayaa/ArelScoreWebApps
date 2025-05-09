using System.ComponentModel.DataAnnotations;

namespace ArelScoreWebUI.Models
{
    public class User
    {
        public Guid Id { get; set; } // <-- Bu satır eklendi, EF için gerekli.
        public string FirstName { get; set; }
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        public string RegistrationDate { get; set; }

        public bool EmailConfirmed { get; set; } = false;

        public ICollection<Voting> Votings { get; set; }
    }
}
