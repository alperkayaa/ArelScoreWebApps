using ArelScoreWebUI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArelScoreWebUI.Migrations.Configuration
{
    public class VotingConfiguration : IEntityTypeConfiguration<Voting>
    {
        public void Configure(EntityTypeBuilder<Voting> builder)
        {
            builder.ToTable("Votings");
            builder.HasKey(t => t.Id);

            // User ile olan ilişkiyi belirtin
            builder.HasOne(v => v.User)
                   .WithMany(u => u.Votings)  // User sınıfındaki Votings koleksiyonuna işaret eder
                   .HasForeignKey(v => v.UserId)
                   .OnDelete(DeleteBehavior.Restrict);  // NoAction yerine Restrict de tercih edilebilir
        }
    }
}
