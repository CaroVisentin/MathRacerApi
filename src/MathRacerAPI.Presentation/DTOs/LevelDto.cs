namespace MathRacerAPI.Presentation.DTOs
{
    /// <summary>
    /// DTO para representar un nivel
    /// </summary>
    public class LevelDto
    {
        public int Id { get; set; }
        public int WorldId { get; set; }
        public int Number { get; set; }
        public int TermsCount { get; set; }
        public int VariablesCount { get; set; }
        public string ResultType { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}
