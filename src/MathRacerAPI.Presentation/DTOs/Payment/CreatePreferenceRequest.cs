namespace MathRacerAPI.Presentation.DTOs.Payment
{
    public class CreatePreferenceRequest
    {
        public int PlayerId { get; set; }
        public int CoinPackageId { get; set; }

        public string SuccessUrl { get; set; } = "";
        public string FailureUrl { get; set; } = "";
        public string PendingUrl { get; set; } = "";
    }
}
