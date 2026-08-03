using Microsoft.EntityFrameworkCore;
using WatchListScreening.Domain.Entities;
using WatchListScreening.Domain.Enums;

namespace WatchListScreening.Infrastructure.Data.Seed;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SanctionEntry>().HasData(

            new SanctionEntry
            {
                Id = 1,
                FullName = "John Alexander Smith",
                EntityType = EntityType.Person,
                Country = "United States",
                ListSource = "OFAC",
                DateOfBirth = new DateOnly(1975, 3, 15),
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 2,
                FullName = "Ali Hassan Mohammed",
                EntityType = EntityType.Person,
                Country = "Syria",
                ListSource = "UN",
                DateOfBirth = new DateOnly(1980, 7, 22),
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 3,
                FullName = "Petrolex Trading Corp",
                EntityType = EntityType.Organization,
                Country = "Russia",
                ListSource = "EU",
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 4,
                FullName = "Mehmet Yılmaz",
                EntityType = EntityType.Person,
                Country = "Turkey",
                ListSource = "MASAK",
                DateOfBirth = new DateOnly(1968, 11, 3),
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 5,
                FullName = "Golden Bridge Holdings Ltd",
                EntityType = EntityType.Organization,
                Country = "Iran",
                ListSource = "OFAC",
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 6,
                FullName = "Fatima Al-Rashid",
                EntityType = EntityType.Person,
                Country = "Iraq",
                ListSource = "UN",
                DateOfBirth = new DateOnly(1992, 4, 18),
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 7,
                FullName = "Nord Stream Finance GmbH",
                EntityType = EntityType.Organization,
                Country = "Germany",
                ListSource = "EU",
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 8,
                FullName = "Carlos Rodriguez Vega",
                EntityType = EntityType.Person,
                Country = "Venezuela",
                ListSource = "OFAC",
                DateOfBirth = new DateOnly(1971, 9, 30),
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 9,
                FullName = "Bright Star Logistics",
                EntityType = EntityType.Organization,
                Country = "North Korea",
                ListSource = "UN",
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            },

            new SanctionEntry
            {
                Id = 10,
                FullName = "Ayşe Demir",
                EntityType = EntityType.Person,
                Country = "Turkey",
                ListSource = "MASAK",
                DateOfBirth = new DateOnly(1985, 6, 14),
                IsActive = true,
                AddedAt = new DateTime(2024, 01, 01),
                CreatedAt = new DateTime(2024, 01, 01)
            }
        );
    }
}