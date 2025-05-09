using ArelScoreWebUI.Migrations.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ArelScoreWebUI.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Yeni bir tablo geldiğinde buraya dbset olarak eklersen eğer EFCore buraya eklediğin databasetini otoatik olarak tabloya dönüştürür 
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Voting> Votings { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ProjectConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new VotingConfiguration());
            modelBuilder.ApplyConfiguration(new EmailVerificationConfiguration());
        }
    }
}
