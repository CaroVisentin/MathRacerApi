namespace MathRacerAPI.Domain.Models
{
    public class Equation
    {
        public int Id { get; set; }
        public string EquationString { get; set; } = string.Empty;
        public List<int> Options { get; set; } = new();
        public int CorrectAnswer { get; set; }
    }
}

