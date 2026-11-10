using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class CatGirl
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
        public double HeightCm { get; set; }
        public double WeightKg { get; set; }

        [MaxLength(300)]
        public string? ImagePath { get; set; }

        [MaxLength(30)]
        public string Phone { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }

    // Létrehozáskor és módosításkor ezt küldi a kliens (a leírás nem kötelező)
    public record CatGirlRequest(string Name, int Age, double HeightCm, double WeightKg,
        string Phone, string Email, string? Description);

    // GET api/catgirls/stats válasza
    public record CatGirlStats(int Count, double AverageAge, double AverageHeightCm, double AverageWeightKg);
}
