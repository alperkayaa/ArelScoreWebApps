using ArelScoreWebUI.Models;
using ArelScoreWebUI.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ArelScoreWebUI.Controllers
{
    [AllowAnonymous]
    public class ProjectController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<HomeController> _logger;

        public ProjectController(AppDbContext dbContext,
                                 ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }


        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> List()
        //{
        //    var projects = await _dbContext.Projects
        //        .Include(p => p.User)
        //        .Include(p => p.Votes)
        //        .Select(p => new ProjectListItem
        //        {
        //            Id = p.Id,
        //            Name = p.Name,
        //            Description = p.Description,
        //            UploadDate = p.UploadDate,
        //            ImageUrl = p.ImageUrl,
        //            TotalVotes = p.Votes.Count,
        //            Average = p.Votes.Any() ? p.Votes.Average(v => v.Rating) : 0,
        //            OwnerName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : "Anonim"
        //        })
        //        .ToListAsync();

        //    var model = new ProjectListViewModel
        //    {
        //        Projects = projects
        //    };

        //    return View(model);
        //}


        [HttpGet]
        public IActionResult Upload()
        {
            // Giriş yapılmamışsa giriş sayfasına yönlendirme:
            if (HttpContext.Session.GetString("UserId") == null)
            {
                TempData["Error"] = "Proje yüklemek için lütfen giriş yapınız";
                return RedirectToAction("Giris", "Giris");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload([FromForm] ProjectUploadViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Lütfen proje bilgilerini eksiksiz doldurunuz";
                    return RedirectToAction("Index", "Home");
                }

                var userId = HttpContext.Session.GetString("UserId");
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Kullanıcı bilgileri hatalı";
                    return RedirectToAction("Index", "Home");
                }

                string imagePath = null;
                if (model.ImageUrl != null && model.ImageUrl.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageUrl.FileName)}";
                    var filePath = Path.Combine("wwwroot/storage/uploads", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageUrl.CopyToAsync(stream);
                    }

                    imagePath = $"/storage/uploads/{fileName}";
                }

                var createProject = new Project
                {
                    Name = model.Name,
                    Description = model.Description,
                    SiteLink = model.SiteLink,
                    YoutubeLink = model.YoutubeLink,
                    GroupNo = model.GroupNo,
                    ImageUrl = imagePath,
                    UploadDate = DateTime.Now,
                    UserId = Guid.Parse(userId),
                };

                _dbContext.Projects.Add(createProject);
                var result = await _dbContext.SaveChangesAsync();
                if (result <= 0)
                {
                    TempData["Error"] = "Proje bilgileri yüklenemedi lütfen tekrar deneyiniz";
                    return RedirectToAction("Index", "Home");
                }

                TempData["Success"] = "Proje başarıyla yüklendi";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Proje kaydedilmesi sırasında beklenmedik bir hata oluştu. Lütfen tekrar deneyiniz. \n {{ex.Message.ToString()}}";
                return RedirectToAction("Index", "Home");
                throw;
            }
        }


        [HttpGet]
        public IActionResult Details(int id)
        {
            // Projeyi ve oylamaları getirme
            var project = _dbContext.Projects
                .Include(p => p.User)
                .Include(p => p.Votes)
                .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                TempData["Error"] = "Proje bilgileri yüklenemedi lütfen tekrar deneyiniz";
                return RedirectToAction("Details", "Project");
            }

            // Oylama bilgilerini hesaplama
            var totalVotes = project.Votes.Count;
            var averageRating = totalVotes > 0 ? project.Votes.Average(v => v.Rating) : 0;

            // Kullanıcının daha önce oylama yapıp yapmadığını kontrol etme
            var userRating = 0; // Varsayılan oy 0 (oy verilmemiş)
            if (User.Identity.IsAuthenticated)
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userVote = project.Votes.FirstOrDefault(v => v.UserId == userId);
                if (userVote != null)
                {
                    userRating = userVote.Rating;
                }
            }

            // ViewModel'e gerekli veriyi dönüştürme
            var viewModel = new ProjectVoteResultViewModel
            {
                ProjectId = project.Id,
                Name = project.Name,
                Description = project.Description,
                ImageUrl = project.ImageUrl,
                YoutubeLink = project.YoutubeLink,
                SiteLink = project.SiteLink,
                UploadDate = project.UploadDate,
                OwnerName = project.User.FirstName + " " + project.User.LastName,
                TotalVotes = totalVotes,
                AverageRating = averageRating,
                UserRating = userRating,
                GroupNo = project.GroupNo

            };

            return View(viewModel);
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Giriş yapmanız gerekiyor.";
                return RedirectToAction("Login", "Account");
            }

            var project = _dbContext.Projects.FirstOrDefault(p => p.Id == id && p.UserId == Guid.Parse(userId));
            if (project == null)
            {
                TempData["Error"] = "Proje bulunamadı veya bu projeye erişim yetkiniz yok.";
                return RedirectToAction("Ranking");
            }

            ViewBag.GetImageUrl = project.ImageUrl;
            var model = new ProjectEditViewModel
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                SiteLink = project.SiteLink,
                YoutubeLink = project.YoutubeLink,
                GroupNo = project.GroupNo,
                UploadDate = project.UploadDate,
                UserId = project.UserId,
                ImageUrl = project.ImageUrl
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit2(int id, ProjectEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Lütfen tüm alanları eksiksiz doldurun.";
                return View(model);
            }

            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Oturum bilgisi geçersiz.";
                return RedirectToAction("Login", "Account");
            }

            var project = _dbContext.Projects.FirstOrDefault(p => p.Id == id && p.UserId == Guid.Parse(userId));
            if (project == null)
            {
                TempData["Error"] = "Bu projeyi düzenleyemezsiniz.";
                return RedirectToAction("Ranking");
            }

            var projectDto = new Project()
            {
                Description = model.Description,
                SiteLink = model.SiteLink,
                YoutubeLink = model.YoutubeLink,
                GroupNo = model.GroupNo,
                ImageUrl = model.ImageUrl,
                Name = model.Name,
                UploadDate = DateTime.Now,
                UserId = Guid.Parse(userId),
            };

            try
            {
                if (model.Image != null && model.Image.Length > 0)
                {
                    // Uzantı kontrolü
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".tiff", ".bmp" };
                    var extension = Path.GetExtension(model.Image.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        TempData["Error"] = "Sadece resim dosyaları yükleyebilirsiniz.";
                        return View(model);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/storage/uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(stream);
                    }

                    // Eski resmi sil (varsa)
                    if (!string.IsNullOrEmpty(project.ImageUrl))
                    {
                        var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", projectDto.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    projectDto.ImageUrl = $"/storage/uploads/{fileName}";
                }
                else
                {
                    if (project.ImageUrl != null)
                    {
                        projectDto.ImageUrl = project.ImageUrl;

                    }
                    else
                    {
                        projectDto.ImageUrl = $"/storage/uploads/defautimage.png";
                    }
                }

                _dbContext.Projects.Update(projectDto);
                await _dbContext.SaveChangesAsync();
                TempData["Success"] = "Proje başarıyla güncellendi.";
                return RedirectToAction("Ranking");
            }
            catch (Exception ex)
            {
                var abc= ex.Message;
                TempData["Error"] = "Proje güncellenirken bir hata oluştu.";
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectEditViewModel model, string userId)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Geçersiz veri.";
                return View(model);
            }

            var project = await _dbContext.Projects
                                          .FirstOrDefaultAsync(p => p.Id == id && p.UserId == Guid.Parse(userId));

            if (project == null)
            {
                TempData["Error"] = "Bu projeyi düzenleyemezsiniz.";
                return RedirectToAction("Ranking");
            }

            // Var olan projeyi güncelle
            project.Description = model.Description;
            project.SiteLink = model.SiteLink;
            project.YoutubeLink = model.YoutubeLink;
            project.GroupNo = model.GroupNo;
            project.Name = model.Name;
            project.UploadDate = DateTime.Now;

            // Eğer yeni bir resim yüklenmişse
            if (model.Image != null && model.Image.Length > 0)
            {
                // Uzantı kontrolü
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".tiff", ".bmp" };
                var extension = Path.GetExtension(model.Image.FileName).ToLower();
                if (!allowedExtensions.Contains(extension))
                {
                    TempData["Error"] = "Sadece resim dosyaları yükleyebilirsiniz.";
                    return View(model);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/storage/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(stream);
                }

                // Eski resmi sil (varsa)
                if (!string.IsNullOrEmpty(project.ImageUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", project.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                project.ImageUrl = $"/storage/uploads/{fileName}";
            }
            else
            {
                // Eğer resim yüklenmemişse ve mevcut resim yoksa, varsayılan resim ekleyin
                if (string.IsNullOrEmpty(project.ImageUrl))
                {
                    project.ImageUrl = $"/storage/uploads/defaultimage.png";
                }
            }

            try
            {
                // DbContext zaten projeyi izliyor, bu yüzden Update yapmamıza gerek yok
                await _dbContext.SaveChangesAsync();
                TempData["Success"] = "Proje başarıyla güncellendi.";
                return RedirectToAction("Ranking");
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj göster
                TempData["Error"] = "Proje güncellenirken bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["Error"] = "Oturum bilgisi bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            var userId = Guid.Parse(userIdString);

            var project = _dbContext.Projects.FirstOrDefault(p => p.Id == id && p.UserId == userId);
            if (project == null)
            {
                TempData["Error"] = "Proje bulunamadı veya silme yetkiniz yok.";
                return RedirectToAction("Ranking");
            }

            // Projeye ait resim dosyasını sil (varsa)
            if (!string.IsNullOrEmpty(project.ImageUrl))
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", project.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            _dbContext.Projects.Remove(project);
            _dbContext.SaveChanges();

            TempData["Success"] = "Proje başarıyla silindi.";
            return RedirectToAction("Ranking");
        }


        [HttpGet]
        public IActionResult Ranking()
        {
            var projectListViewModel = new ProjectListViewModel
            {
                Projects = _dbContext.Projects
                    .Include(p => p.User)
                    .Include(p => p.Votes)
                    .Where(p => !string.IsNullOrEmpty(p.Name))
                    .Select(p => new ProjectListItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        GroupNo = p.GroupNo,
                        Description = p.Description,
                        UploadDate = p.UploadDate,
                        ImageUrl = p.ImageUrl,
                        TotalVotes = p.Votes.Count,
                        Average = p.Votes.Any() ? p.Votes.Average(v => v.Rating) : 0,
                        OwnerName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : "Anonim"
                    })
                    .OrderByDescending(p => p.Average)   // Önce ortalamaya göre sırala
                    .ThenByDescending(p => p.TotalVotes)    //Sonra en çok oy alan projeye göre sırala
                    .ToList()
            };

            return View(projectListViewModel);
        }


        [HttpGet]
        public IActionResult MyProject()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
            {
                TempData["Error"] = "Projelerinizi görmek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Account");
            }

            var userId = Guid.Parse(userIdString);

            var projectListViewModel = new ProjectListViewModel
            {
                Projects = _dbContext.Projects
                    .Include(p => p.User)
                    .Include(p => p.Votes)
                    .Where(p => p.UserId == userId && !string.IsNullOrEmpty(p.Name)) // sadece giriş yapan kullanıcının projeleri
                    .Select(p => new ProjectListItem
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        UploadDate = p.UploadDate,
                        ImageUrl = p.ImageUrl,
                        TotalVotes = p.Votes.Count,
                        Average = p.Votes.Any() ? p.Votes.Average(v => v.Rating) : 0,
                        OwnerName = p.User != null ? $"{p.User.FirstName} {p.User.LastName}" : "Anonim"
                    })
                    .OrderByDescending(p => p.TotalVotes)
                    .ThenByDescending(p => p.Average)
                    .ToList()
            };

            return View(projectListViewModel);
        }
    }
}
