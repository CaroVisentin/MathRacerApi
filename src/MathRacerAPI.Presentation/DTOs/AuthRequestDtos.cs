namespace MathRacerAPI.Presentation.DTOs
{
    public class RegisterRequestDto
    {
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Uid { get; set; } // Opcional, para compatibilidad con Firebase
    }

    public class LoginRequestDto
    {
    public string? Email { get; set; }
    public string? Password { get; set; }
    }

    public class GoogleRequestDto
    {
    public string? IdToken { get; set; }
    public string? Email { get; set; } // Opcional, para crear usuario si no existe
    public string? Username { get; set; } // Opcional, para crear usuario si no existe
    }
}
