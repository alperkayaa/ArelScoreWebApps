using System.ComponentModel.DataAnnotations;

namespace ArelScoreWebUI.Models
{
    public class Voting
    {
        [Key]
        public int Id { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } // 1 ile 5 arasında oy

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        /// <summary>
        ///  EF CORE için gerekli.
        /// </summary>
        public Guid UserId { get; set; }
        public int ProjectId { get; set; }
        public virtual User User { get; set; }
        public virtual Project Project { get; set; }
    }
}
