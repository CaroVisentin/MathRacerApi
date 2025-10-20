using MathRacerAPI.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Domain.UseCases
{
    public class GetQuestionsUseCase
    {
        public async Task<List<Question>> GetQuestions(EquationParams p, int count)
        {
            return await Task.Run(() =>
            {
                var equations = new List<Question>();
                for (int i = 0; i < count; i++)
                {
                    equations.Add(GenerateEquation(p));
                }
                return equations;
            });
        }

        private readonly Random _rand = new();

        public Question GenerateEquation(EquationParams p)
        {
            ValidateParams(p);

            List<string> terms = GenerateTerms(p.TermCount, p.VariableCount, p.Operations, p.NumberRangeMin, p.NumberRangeMax);

            string equationRight = JoinTermsWithOperations(terms, p.Operations);

            string equation = $"y = {equationRight}";

            List<int> opts = GenerateOptions(p.OptionsCount, p.OptionRangeMin, p.OptionRangeMax);

            int xMin = opts.First();
            int xMax = opts.Last();

            int correctX = FindCorrectOption(equationRight, xMin, xMax, p.ExpectedResult);

            opts = opts.OrderBy(_ => _rand.Next()).ToList();

            var equationReturn = new Question
            {
                Id = _rand.Next(1000, 9999),
                Equation = equation,
                Options = opts,
                CorrectAnswer = correctX
            };

            return equationReturn;
        }

        private void ValidateParams(EquationParams p)
        {
            if (p.TermCount < 2)
                p.TermCount = 2;

            if (p.VariableCount < 1)
                p.VariableCount = 1;

            if (p.VariableCount > p.TermCount)
                p.VariableCount = p.TermCount;

            if (p.OptionsCount < 2)
                p.OptionsCount = 2;

            if (p.OptionRangeMin >= p.OptionRangeMax)
            {
                p.OptionRangeMin = -10;
                p.OptionRangeMax = 10;
            }

            if (p.NumberRangeMin >= p.NumberRangeMax)
            {
                p.NumberRangeMin = -10;
                p.NumberRangeMax = 10;
            }

            var validOps = new HashSet<string> { "+", "-", "*", "/" };

            if (p.Operations == null || p.Operations.Count == 0)
            {
                p.Operations = new List<string> { "+", "-" };
            }
            else
            {
                p.Operations = p.Operations
                    .Where(op => validOps.Contains(op))
                    .Distinct()
                    .ToList();

                if (p.Operations.Count == 0)
                    p.Operations = new List<string> { "+", "-" };
            }
        }

        private List<string> GenerateTerms(int termCount, int variableCount, List<string> operations, int numMin, int numMax)
        {
            var terms = new List<string>();
            bool allowMult = operations.Contains("*");
            bool allowDiv = operations.Contains("/");

            for (int i = 0; i < termCount; i++)
            {
                if (variableCount > i)
                {
                    string coef = RandomNumberExceptZero(numMin, numMax);

                    if (coef.StartsWith("-"))
                        coef = $"({coef})";

                    if (allowMult && allowDiv)
                    {
                        terms.Add(_rand.Next(0, 2) == 0 ? $"{coef}*x" : _rand.Next(0, 2) == 0 ? $"x/{coef}" : $"{coef}/x");
                    }
                    else if (allowMult)
                    {
                        terms.Add(_rand.Next(0, 2) == 0 ? $"{coef}*x" : $"x*{coef}");

                    }
                    else if (allowDiv)
                    {
                        terms.Add(_rand.Next(0, 2) == 0 ? $"{coef}/x" : $"x/{coef}");
                    }
                    else
                    {
                        terms.Add(_rand.Next(0, 2) == 0 ? "x" : "-x");
                    }

                }
                else
                {
                    string num = RandomNumberExceptZero(numMin, numMax);
                    terms.Add(num);
                }
            }

            return terms.OrderBy(x => _rand.Next()).ToList();

        }

        private string JoinTermsWithOperations(List<string> terms, List<string> operations)
        {
            bool allowAdd = operations.Contains("+");
            bool allowSus = operations.Contains("-");

            string equationRight = terms[0];
            for (int i = 1; i < terms.Count; i++)
            {
                string op = "";

                if (allowAdd && allowSus || !allowAdd && !allowSus)
                {
                    op = _rand.Next(0, 2) == 0 ? "+" : "-";
                }
                else if (allowAdd)
                {
                    op = "+";
                }
                else if (allowSus)
                {
                    op = "-";
                }

                string term = terms[i];

                if (term.StartsWith("-"))
                    term = $"({term})";

                equationRight += $" {op} {term}";
            }

            return equationRight;
        }

        private List<int> GenerateOptions(int count, int min, int max)
        {
            var options = new HashSet<int>();
            while (options.Count < count)
                options.Add(_rand.Next(min, max + 1));
            var opts = options.ToList();
            opts.Sort();
            return opts;
        }

        private int FindCorrectOption(string equationRight, int xMin, int xMax, string expectedResult)
        {
            double yMin = EvaluateExpression(equationRight, xMin); //Resuelve la ecuación.
            double yMax = EvaluateExpression(equationRight, xMax);

            int correctX;

            string expected = (expectedResult ?? "MAYOR").ToUpperInvariant();

            if (expected != "MAYOR" && expected != "MENOR")
            {
                expected = "MAYOR";
            }

            if (expected == "MAYOR")
                correctX = yMax > yMin ? xMax : xMin; //Si yMax es mayor, xMax es la respuesta correcta.
            else
                correctX = yMax < yMin ? xMax : xMin;

            return correctX;

        }

        private double EvaluateExpression(string exprRight, int xValue)
        {
            try
            {
                string replaced = exprRight.Replace("x", xValue.ToString(CultureInfo.InvariantCulture));
                var result = new DataTable().Compute(replaced, "");     //Evalúa expresiones aritméticas como si fueran fórmulas de Excel. Entrega la ecuacion resuelta.
                return Convert.ToDouble(result, CultureInfo.InvariantCulture);
            }
            catch
            {
                return double.NaN;
            }
        }

        private string RandomNumberExceptZero(int min, int max)
        {
            int v;
            do { v = _rand.Next(min, max + 1); } while (v == 0);
            return v.ToString();
        }
    }



}
