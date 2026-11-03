using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnterpriseUserManagement.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.Department)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedDate)
            .IsRequired();

        builder.Property(u => u.ModifiedDate)
            .IsRequired(false);

        // Seed sample enterprise users with fixed timestamps for deterministic EF Core migrations
        builder.HasData(GetSeedUsers());
    }

    private static IEnumerable<User> GetSeedUsers()
    {
        return new List<User>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Eleanor",
                LastName = "Vance",
                Email = "eleanor.vance@enterprise.com",
                Department = Department.Engineering,
                Role = UserRole.Admin,
                IsActive = true,
                CreatedDate = new DateTime(2025, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Marcus",
                LastName = "Brody",
                Email = "marcus.brody@enterprise.com",
                Department = Department.Engineering,
                Role = UserRole.Manager,
                IsActive = true,
                CreatedDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                FirstName = "Sophia",
                LastName = "Chen",
                Email = "sophia.chen@enterprise.com",
                Department = Department.Engineering,
                Role = UserRole.Employee,
                IsActive = true,
                CreatedDate = new DateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                FirstName = "David",
                LastName = "Miller",
                Email = "david.miller@enterprise.com",
                Department = Department.HR,
                Role = UserRole.Manager,
                IsActive = true,
                CreatedDate = new DateTime(2025, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                FirstName = "Rachel",
                LastName = "Green",
                Email = "rachel.green@enterprise.com",
                Department = Department.HR,
                Role = UserRole.Employee,
                IsActive = true,
                CreatedDate = new DateTime(2025, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                FirstName = "Alexander",
                LastName = "Hamilton",
                Email = "alexander.hamilton@enterprise.com",
                Department = Department.Finance,
                Role = UserRole.Manager,
                IsActive = true,
                CreatedDate = new DateTime(2025, 9, 15, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                FirstName = "Jordan",
                LastName = "Belfort",
                Email = "jordan.belfort@enterprise.com",
                Department = Department.Sales,
                Role = UserRole.Employee,
                IsActive = false,
                CreatedDate = new DateTime(2025, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = new DateTime(2025, 10, 20, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                FirstName = "Amelia",
                LastName = "Earhart",
                Email = "amelia.earhart@enterprise.com",
                Department = Department.Operations,
                Role = UserRole.Employee,
                IsActive = true,
                CreatedDate = new DateTime(2025, 10, 15, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                FirstName = "Lucas",
                LastName = "Scott",
                Email = "lucas.scott@enterprise.com",
                Department = Department.Support,
                Role = UserRole.Employee,
                IsActive = true,
                CreatedDate = new DateTime(2025, 10, 25, 0, 0, 0, DateTimeKind.Utc),
                ModifiedDate = null
            }
        };
    }
}

