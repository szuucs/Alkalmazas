namespace Cicalany.Wpf.Models;

public sealed class LoginResult
{
    public string Token { get; set; } = "";
    public string UserName { get; set; } = "";
    public int UserId { get; set; }
}
