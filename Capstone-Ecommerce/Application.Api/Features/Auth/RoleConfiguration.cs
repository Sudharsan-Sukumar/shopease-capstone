using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Application.Api.Features.Auth;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.Property(r => r.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(r => r.Name).IsUnique();

        // Safe to seed via HasData - role names are static, no hashing involved.
        // SubAdmin/Supervisor/SupportAgent are staff tiers an Admin (or a
        // Sub Admin, for the lower two) provisions through the admin panel -
        // Customer is deliberately never assignable there, only via self-registration.
        builder.HasData(
            new Role { Id = 1, Name = RoleNames.Admin },
            new Role { Id = 2, Name = RoleNames.Customer },
            new Role { Id = 3, Name = RoleNames.SubAdmin },
            new Role { Id = 4, Name = RoleNames.Supervisor },
            new Role { Id = 5, Name = RoleNames.SupportAgent }
        );
    }
}
