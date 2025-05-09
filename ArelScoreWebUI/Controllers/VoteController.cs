using ArelScoreWebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArelScoreWebUI.Controllers
{

    public class VoteController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<HomeController> _logger;

        public VoteController(AppDbContext dbContext,
                              ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Vote(int projectId, int rating)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                ModelState.AddModelError("Error", "Oy kullanmak için lütfen giriş yapınız");
                TempData["Error"] = "Oy kullanmak için lütfen giriş yapınız";
                return RedirectToAction("Giris", "Giris");
            }

            // 1. Oy aralığı kontrolü
            if (rating < 1 || rating > 5)
            {
                TempData["Error"] = "Lütfen 1 ile 5 arasında bir oy verin.";
                return RedirectToAction("Details", "Project", new { id = projectId });
            }

            // Kullanıcıyı getir
            var userId = Guid.Parse(userIdString);
            var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bilgisi bulunamadı";
                return RedirectToAction("Details", "Project", new { id = projectId });
            }

            if (user.EmailConfirmed == false)
            {
                TempData["Error"] = "E-posta adresinizi doğrulamanız gerekiyor.";
                return RedirectToAction("Details", "Project", new { id = projectId });
            }

            // 3. Domain kontrolü
            var allowedDomains = new List<string> { "@istanbularel.edu.tr", "@arel.edu.tr" };
            bool isValidDomain = allowedDomains.Any(domain => user.Email.EndsWith(domain, StringComparison.OrdinalIgnoreCase));
            if (!isValidDomain)
            {
                TempData["Error"] = "Sadece @istanbularel.edu.tr veya @arel.edu.tr uzantılı kullanıcılar oy kullanabilir.";
                return RedirectToAction("Details", "Project", new { id = projectId });
            }

            // 4. Proje ve oy kontrolü
            var project = _dbContext.Projects.Include(p => p.Votes).FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                TempData["Error"] = "Proje bilgisi bulunamadı";
                return RedirectToAction("Details", "Project", new { id = projectId });
            }

            //  Bu projeye daha önce oy verdiyse güncelle, vermediyse yeni oy ekle
            var existingVote = project.Votes.FirstOrDefault(v => v.UserId == userId);

            if (existingVote != null)
            {
                existingVote.Rating = rating;
                existingVote.CreatedAt = DateTime.Now;
            }
            else
            {
                var newVote = new Voting
                {
                    ProjectId = projectId,
                    UserId = userId,
                    Rating = rating,
                    CreatedAt = DateTime.Now
                };

                project.Votes.Add(newVote);
            }

            _dbContext.SaveChanges();

            TempData["Success"] = "✅ Oy kullanma işlemi başarılı bir şekilde gerçekleşti";
            return RedirectToAction("Details", "Project", new { id = projectId });
        }
    }
}
