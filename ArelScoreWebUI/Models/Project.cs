using System.ComponentModel.DataAnnotations;

namespace ArelScoreWebUI.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public string? SiteLink { get; set; }
        public string? YoutubeLink { get; set; }
        public string ImageUrl { get; set; }
        public string GroupNo { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public Guid UserId { get; set; }

        public virtual User User { get; set; }
        public ICollection<Voting> Votes { get; set; }
    }
}
