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

    // Regisztrációkor ezt küldi a kliens (POST api/users)
    public record RegisterRequest(string Name, string Password);

    // Módosításkor: új név, és opcionálisan új jelszó (PUT api/users/{id})
    public record UpdateUserRequest(string Name, string? Password);

    // Ezt kapja vissza a kliens, a jelszó sosem megy ki
    public record UserResponse(int Id, string Name);
}
