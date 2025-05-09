namespace ArelScoreWebUI.Models.ViewModel
{
    public class ProjectEditViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string? SiteLink { get; set; }
        public string YoutubeLink { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImageUrl { get; set; }
        public string GroupNo { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.Now;
        public Guid UserId { get; set; }
    }
}
