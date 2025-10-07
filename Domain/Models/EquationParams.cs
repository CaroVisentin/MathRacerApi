namespace MathRacerAPI.Domain.Models
{
    public class EquationParams
    {
        public int TermCount { get; set; } = 2;
        public int VariableCount { get; set; } = 1;
        public List<string> Operations { get; set; } = new() { "+", "-" };
        public string ExpectedResult { get; set; } = "MAYOR";
        public int OptionsCount { get; set; } = 3;
        public int OptionRangeMin { get; set; } = -10;
        public int OptionRangeMax { get; set; } = 10;
        public int NumberRangeMin { get; set; } = -10;
        public int NumberRangeMax { get; set; } = 10;
        public int TimePerEquation { get; set; } = 10;
    }
}
