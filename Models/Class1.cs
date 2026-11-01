namespace Models
{
    public enum PreferredGender
    {
        Male,
        Female,
        Any
    }
    public class CatGirl
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public double HeightCm {  get; set; }
        public double WeightKg { get; set; }
        public PreferredGender PreferredGender { get; set; }
    }
}
