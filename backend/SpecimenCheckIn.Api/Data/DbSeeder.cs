using Microsoft.EntityFrameworkCore;
using SpecimenCheckIn.Api.Models;

namespace SpecimenCheckIn.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Labs.AnyAsync())
            return;

        var lab1 = new Lab
        {
            Name = "Lab Alpha"
        };

        var lab2 = new Lab
        {
            Name = "Lab Beta"
        };

        var manifest1 = new Manifest
        {
            Code = "MF-2026-001",
            Status = "Open",
            SentAt = DateTime.UtcNow,
            Lab = lab1,

            Specimens = new List<Specimen>
            {
                new() { Code = "SP-1001", Patient = "Sarah Lin" },
                new() { Code = "SP-1002", Patient = "David Lee" },
                new() { Code = "SP-1003", Patient = "Marcus Reed" },
                new() { Code = "SP-1004", Patient = "Priya Shah" },
                new() { Code = "SP-1005", Patient = "Tom Alvarez" },
                new() { Code = "SP-1006", Patient = "Jane Doe" },
                new() { Code = "SP-1007", Patient = "John Smith" }
            }
        };

        var manifest2 = new Manifest
        {
            Code = "MF-2026-002",
            Status = "Open",
            SentAt = DateTime.UtcNow,
            Lab = lab1,

            Specimens = new List<Specimen>
            {
                new() { Code = "SP-2001", Patient = "Alex Brown" },
                new() { Code = "SP-2002", Patient = "Mary Jones" }
            }
        };

        var manifest3 = new Manifest
        {
            Code = "MF-2026-003",
            Status = "Open",
            SentAt = DateTime.UtcNow,
            Lab = lab2,

            Specimens = new List<Specimen>
            {
                new() { Code = "SP-3001", Patient = "Peter Wilson" },
                new() { Code = "SP-3002", Patient = "Chris Martin" }
            }
        };

        context.Manifests.AddRange(
            manifest1,
            manifest2,
            manifest3
        );

        await context.SaveChangesAsync();
    }
}