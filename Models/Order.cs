using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        public DateTime OrderDate { get; set; }

        [MaxLength(200)]
        public string Location { get; set; } = "Online";

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public List<OrderLine> Lines { get; set; } = new();
    }

    // A rendelés tétele a rendeléskori adatokat őrzi meg (akkor is, ha a cicálányt később törlik)
    public class OrderLine
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int CatGirlId { get; set; }

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
        public double HeightCm { get; set; }
        public double WeightKg { get; set; }
    }

    // POST api/orders
    public record OrderRequest(int UserId, DateTime OrderDate, string? Location, List<int> CatGirlIds);

    // PUT api/orders/{id}
    public record OrderUpdateRequest(DateTime OrderDate, string? Location);

    public record OrderLineResponse(int CatGirlId, string Name, int Age, double HeightCm, double WeightKg);

    public record OrderResponse(int Id, int UserId, DateTime OrderDate, string Location,
        DateTime CreatedAtUtc, List<OrderLineResponse> Lines);
}
