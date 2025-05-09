using ArelScoreWebUI.Middleware;
using ArelScoreWebUI.Models;
using ArelScoreWebUI.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ArelScoreWebUI.Controllers
{
    public class GirisController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public GirisController(AppDbContext dbContext, IEmailService emailService, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _emailService = emailService;
            _configuration = configuration;
        }


        /// <summary>
        /// Doğrulama Linkini Tüketen Endpoint
        /// </summary>
        /// <param name="email"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("verify222")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string code)
        {
            var record = await _dbContext.EmailVerifications
                .FirstOrDefaultAsync(v => v.Email == email && v.VerificationCode == code && !v.IsUsed);

            if (record == null || record.Expiration < DateTime.UtcNow)
            {
                return BadRequest("Bağlantı geçersiz veya süresi dolmuş.");
            }

            record.IsUsed = true;
            await _dbContext.SaveChangesAsync();

            // TODO: Kullanıcının EmailConfirmed alanı güncellenmeli
            return Content("E-posta adresiniz başarıyla doğrulandı.");
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Verify([FromQuery] string email, [FromQuery] string code)
        {
            var record = _dbContext.EmailVerifications
                .FirstOrDefault(v => v.Email == email && v.VerificationCode == code && !v.IsUsed);

            if (record == null || record.Expiration < DateTime.UtcNow)
            {
                TempData["Error"] = "Doğrulama bağlantısı geçersiz veya süresi dolmuş.";
                return RedirectToAction("Index", "Home");
            }

            record.IsUsed = true;
            _dbContext.EmailVerifications.Update(record);

            var user = _dbContext.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            user.EmailConfirmed = true;
            _dbContext.Users.Update(user);

            _dbContext.SaveChangesAsync();

            TempData["Success"] = "E-posta başarıyla doğrulandı.";
            return RedirectToAction("Index", "Home");
        }

        private string GenerateRandomCode()
        {
            var rng = new Random();
            return rng.Next(100000, 999999).ToString();
        }


        #region Login Register

        [HttpGet]
        public IActionResult Giris()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Giris(UserLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
                if (user != null)
                {
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Email", model.Email); // Kullanıcıyı Session'a kaydet
                    HttpContext.Session.SetString("Username", user.Username);
                    HttpContext.Session.SetString("User", model.Email); // Kullanıcıyı Session'a

                    TempData["Success"] = "Arel Score Uygulamasına Hoş Geldiniz";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    TempData["Error"] = "Geçersiz kullanıcı adı veya parola.";
                    ModelState.AddModelError("", "Geçersiz kullanıcı adı veya parola.");
                    return View();
                }
            }

            TempData["Error"] = "Geçersiz kullanıcı adı veya parola.";
            ModelState.AddModelError("", "Geçersiz kullanıcı adı veya parola.");
            return View();
        }


        [HttpGet]
        public IActionResult Kayit()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Kayit(User model)
        {
            if (!_emailService.IsValidEmail(model.Email))
            {
                TempData["Error"] = "Lütfen Geçerli Bir Mail Adresi Giriniz.";
                ModelState.AddModelError("Error", "Lütfen Geçerli Bir Mail Adresi Giriniz.");
                return View(model);
            }

            model.Username = model.Email;
            model.Email = model.Email;
            model.RegistrationDate = DateTime.Now.ToLongDateString();

            var existingUser = _dbContext.Users.FirstOrDefault(u => u.Email == model.Email);
            if (existingUser != null)
            {
                TempData["Error"] = "Bu e-posta adresi daha önce sisteme kayıt edilmiştir";
                ModelState.AddModelError("", "Bu e-posta zaten kullanılıyor.");
                return View(model);
            }

            try
            {
                model.Id = Guid.NewGuid();
                model.Username = model.Email;
                model.Email = model.Email;
                model.Password = model.Password;
                model.FirstName = model.FirstName;
                model.LastName = model.LastName;

                model.RegistrationDate = DateTime.Now.ToLongDateString();
                model.EmailConfirmed = false;

                _dbContext.Users.Add(model);
                await _dbContext.SaveChangesAsync();

                // 6 haneli doğrulama kodu üret
                var code = GenerateRandomCode();

                var verification = new EmailVerification
                {
                    Email = model.Email,
                    VerificationCode = code,
                    Expiration = DateTime.UtcNow.AddMinutes(30),
                    IsUsed = false
                };

                // Doğrulama kodu veritabanına kaydediliyor
                _dbContext.EmailVerifications.Add(verification);
                await _dbContext.SaveChangesAsync();

                // Doğrulama linki oluştur ve gönder
                var baseUrl = _configuration["Site:BaseUrl"];
                var verifyUrl = $"{baseUrl}/giris/verify?email={Uri.EscapeDataString(model.Email)}&code={code}";

                var subject = "E-posta Doğrulama";
                var body = $@"<p>Merhaba {model.FirstName},</p>
                      <p>Hesabınızı doğrulamak için <a href='{verifyUrl}'>buraya tıklayın</a>.</p>
                      <p>Bu bağlantı 30 dakika boyunca geçerlidir.</p>";

                await _emailService.SendEmailAsync(model.Email, subject, body);

                TempData["Success"] = "Arel Score uygulamasına başarılı bir şekilde kayıt oldunuz. Lütfen e-posta adresinize gelen doğrulama linki ile e-posta adresinizi doğrulayınız";
                return RedirectToAction("Giris");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Kayıt sırasında bir hata oluştu. Lütfen tekrar deneyiniz";
                ModelState.AddModelError("", $"Kayıt sırasında bir hata oluştu: {ex.Message}");
                return View(model);
            }
        }


        [HttpGet]
        public IActionResult Cikis()
        {
            // ASP.NET Core'da oturumdan çıkış böyle olur
            HttpContext.Session.Clear();
            TempData["Success"] = "Hesabınızdan güvenli bir şekilde çıkış yaptınız";
            return RedirectToAction("Giris", "Giris");
        }


        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }


        public bool VerifyPassword(string enteredPassword, string storedHashedPassword)
        {
            string enteredPasswordHash = ComputeSha256Hash(enteredPassword);
            return enteredPasswordHash == storedHashedPassword;
        }


        #endregion
    }
}
