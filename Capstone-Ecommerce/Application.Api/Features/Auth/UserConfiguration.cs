using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Api.Features.Auth;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(15);

        // The Admin user is NOT seeded here: PasswordHasher.Hash uses a
        // random salt per call, so there's no static hash value that could
        // correctly go into HasData (which requires compile-time-fixed
        // data). It's seeded at application startup instead - see
        // DbSeeder in Common/Data, which computes a real hash at runtime.
    }
}
