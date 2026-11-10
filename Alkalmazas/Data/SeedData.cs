using Microsoft.EntityFrameworkCore;
using Models;

namespace Alkalmazas.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db, string imageFolder)
    {
        db.Database.Migrate();
        Directory.CreateDirectory(imageFolder);
        foreach (var file in Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "StarterImages"), "*.jpg"))
        {
            var destination = Path.Combine(imageFolder, Path.GetFileName(file));
            if (!File.Exists(destination)) File.Copy(file, destination);
        }

        if (db.CatGirls.Any()) return;
        var starters = new[]
        {
            ("Mirci", 22, 163, 52), ("Szellő", 24, 170, 56),
            ("Pamacs", 19, 158, 48), ("Bandita", 27, 172, 61),
            ("Kozmosz", 25, 168, 54), ("Cirmi", 21, 160, 50),
            ("Hópehely", 23, 165, 51), ("Luna", 20, 161, 49)
        };
        for (var i = 0; i < starters.Length; i++)
        {
            var (name, age, height, weight) = starters[i];
            db.CatGirls.Add(new CatGirl
            {
                Name = name, Age = age, HeightCm = height, WeightKg = weight,
                Phone = "+36 30 123 4567", Email = "demo@example.com",
                ImagePath = $"/images/catgirl{i + 1}.jpg"
            });
        }
        db.SaveChanges();
    }
}
