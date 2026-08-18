using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Api.Features.Auth;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(rt => rt.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(rt => rt.TokenHash).IsUnique();

        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId);

        // IsActive is computed (RevokedAtUtc/ExpiresAtUtc), not a stored column.
        builder.Ignore(rt => rt.IsActive);
    }
}
