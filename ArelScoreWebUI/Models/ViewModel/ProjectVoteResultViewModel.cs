namespace ArelScoreWebUI.Models.ViewModel
{
    public class ProjectVoteResultViewModel
    {
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SiteLink { get; set; }
        public string ImageUrl { get; set; }
        public string YoutubeLink { get; set; }
        public DateTime UploadDate { get; set; }
        public string OwnerName { get; set; }

        public string GroupNo { get; set; }


        public int TotalVotes { get; set; }
        public double AverageRating { get; set; }
        public int UserRating { get; set; } // Kullanıcının daha önce verdiği oy varsa
    }
}
