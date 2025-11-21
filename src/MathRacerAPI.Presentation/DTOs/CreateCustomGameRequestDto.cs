namespace MathRacerAPI.Presentation.DTOs;

public class CreateCustomGameRequestDto
{
    public string GameName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public string? Password { get; set; }
    public string Difficulty { get; set; } = "FACIL"; 
    public string ExpectedResult { get; set; } = "MAYOR"; 
}