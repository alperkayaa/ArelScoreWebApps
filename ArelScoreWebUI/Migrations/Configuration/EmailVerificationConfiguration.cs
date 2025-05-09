using ArelScoreWebUI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArelScoreWebUI.Migrations.Configuration
{
    public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
    {
        public void Configure(EntityTypeBuilder<EmailVerification> builder)
        {
            builder.ToTable("EmailVerifications");
            builder.HasKey(t => t.Id);
        }
    }
}
