using System.Text.RegularExpressions;
using Models;

namespace Alkalmazas
{
    public static class InputValidation
    {
        // Telefonszám: számjegyek, szóköz, kötőjel, zárójel és elöl egy + jel; 7-15 számjegy
        public static bool IsValidPhone(string phone) =>
            Regex.IsMatch(phone, @"^\+?[0-9\s\-()/]+$") && phone.Count(char.IsDigit) is >= 7 and <= 15;

        public static bool IsValidEmail(string email) =>
            email.Length <= 200 && Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // Hibaüzenetet ad vissza, vagy null-t, ha minden rendben van
        public static string? CheckCatGirl(CatGirlRequest request)
        {
            var name = request.Name?.Trim() ?? "";
            var phone = request.Phone?.Trim() ?? "";
            var email = request.Email?.Trim() ?? "";

            if (name.Length == 0) return "A név megadása kötelező.";
            if (name.Length > 100) return "A név legfeljebb 100 karakter lehet.";
            if (request.Age < 18 || request.Age > 40) return "Az életkor 18 és 40 év közé essen.";
            if (!double.IsFinite(request.HeightCm) || request.HeightCm < 0 || request.HeightCm > 300)
                return "A magasság 0 és 300 cm közé essen.";
            if (!double.IsFinite(request.WeightKg) || request.WeightKg < 0 || request.WeightKg > 500)
                return "A súly 0 és 500 kg közé essen.";
            if (phone.Length == 0) return "A telefonszám megadása kötelező.";
            if (!IsValidPhone(phone)) return "A telefonszám formátuma hibás (pl. +36 30 123 4567).";
            if (email.Length == 0) return "Az e-mail cím megadása kötelező.";
            if (!IsValidEmail(email)) return "Az e-mail cím formátuma hibás (pl. nev@pelda.hu).";
            if ((request.Description ?? "").Trim().Length > 1000) return "A leírás legfeljebb 1000 karakter lehet.";
            return null;
        }

        public static string? CheckOrder(DateTime orderDate, string? location)
        {
            if (orderDate < DateTime.Now) return "A rendelés időpontja nem lehet korábbi a mostaninál.";
            if ((location ?? "").Trim().Length > 200) return "A helyszín legfeljebb 200 karakter lehet.";
            return null;
        }
    }
}
