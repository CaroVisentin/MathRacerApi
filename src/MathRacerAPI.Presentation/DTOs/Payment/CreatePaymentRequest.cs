namespace MathRacerAPI.Presentation.DTOs.Payment
{
    public class CreatePaymentRequest
    {
         public int PlayerId { get; set; }
        public int CoinPackageId { get; set; }
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
