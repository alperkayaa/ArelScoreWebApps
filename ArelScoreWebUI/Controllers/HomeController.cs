using ArelScoreWebUI.Models;
using ArelScoreWebUI.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ArelScoreWebUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly AppDbContext _dbContext;

        public HomeController(AppDbContext dbContext,
                                 ILogger<HomeController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var projects = await _dbContext.Projects
                .Include(p => p.User)
                .Include(p => p.Votes)
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
                .ToListAsync();

            var model = new ProjectListViewModel
            {
                Projects = projects
            };

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Profil()
        {
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
