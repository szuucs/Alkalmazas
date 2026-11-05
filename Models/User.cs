using System.ComponentModel.DataAnnotations;

namespace Models
{
    public class User
    {
        public int Id { get; set; }

        [MaxLength(64)]
        public string Name { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
    }
    public record RegisterRequest(string Name, string Password);
    public record UserResponse(int Id, string Name);
}
