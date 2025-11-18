namespace MathRacerAPI.Presentation.DTOs.Payment;

public class CoinPackageDto
{
    public int Id { get; set; }
    public int CoinAmount { get; set; }
    public double Price { get; set; }
    public string Description { get; set; } = string.Empty;
}
