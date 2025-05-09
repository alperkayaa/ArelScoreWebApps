using System.ComponentModel.DataAnnotations;

namespace ArelScoreWebUI.Models.ViewModel
{
    public class ProjectUploadViewModel
    {
        [Required(ErrorMessage = "Lütfen projenin adını giriniz.")]
        public string Name { get; set; }

        public string? SiteLink { get; set; }

        [Required(ErrorMessage = "Lütfen projenin YouTube bağlantısını giriniz.")]
        [Url(ErrorMessage = "Lütfen geçerli bir YouTube bağlantısı giriniz.(https://... )")]
        public string YoutubeLink { get; set; }

        [Required(ErrorMessage = "Lütfen bir proje görseli yükleyiniz.")]
        public IFormFile ImageUrl { get; set; }

        [Required(ErrorMessage = "Lütfen grup numarası giriniz.")]
        public string GroupNo { get; set; }

        [Required(ErrorMessage = "Lütfen proje hakkında bir açıklama giriniz.")]
        public string Description { get; set; }
    }
}
