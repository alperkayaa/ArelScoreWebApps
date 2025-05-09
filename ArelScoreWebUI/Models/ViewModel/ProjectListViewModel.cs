namespace ArelScoreWebUI.Models.ViewModel
{
    public class ProjectListViewModel
    {
        public List<ProjectListItem> Projects { get; set; }
    }

    public class ProjectListItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime UploadDate { get; set; } // artık DateTime!
        public string ImageUrl { get; set; }
        public string GroupNo { get; set; }

        public int TotalVotes { get; set; }
        public double Average { get; set; }
        public string OwnerName { get; set; } // View'da kullanıcı adı buradan gelecek
    }
}
