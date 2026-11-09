namespace PillSync.Client.Models;

public class LoginModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public class RegisterModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
}