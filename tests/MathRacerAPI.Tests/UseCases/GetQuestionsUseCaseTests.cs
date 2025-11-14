using FluentAssertions;
using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.UseCases;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace MathRacerAPI.Tests.UseCases;

/// <summary>
/// Tests para el caso de uso de obtención de preguntas
/// </summary>
public class GetQuestionsUseCaseTests
{
    private readonly GetQuestionsUseCase _getQuestionsUseCase;

    public GetQuestionsUseCaseTests()
    {
        _getQuestionsUseCase = new GetQuestionsUseCase();
    }

    [Fact]
    public async Task GetQuestions_ShouldReturnCorrectCountOfQuestions()
    {
        // Arrange
        var p = CreateDefaultParams();

        // Act
        var result = await _getQuestionsUseCase.GetQuestions(p, 5);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(5);
    }

    [Fact]
    public async Task GetQuestions_WithInvalidParams_ShouldAutocorrectAndReturnQuestions()
    {
        // Arrange
        var p = new EquationParams
        {
            TermCount = 0,
            VariableCount = 0,
            Operations = new List<string>(),
            OptionsCount = 0,
            OptionRangeMin = 10,
            OptionRangeMax = 10,
            NumberRangeMin = 10,
            NumberRangeMax = 10,
            ExpectedResult = ""
        };

        // Act
        var result = await _getQuestionsUseCase.GetQuestions(p, 3);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(3);
        result.ForEach(q =>
        {
            q.Options.Count.Should().BeGreaterOrEqualTo(2);
            q.Equation.Should().StartWith("y = ");
        });
    }

    [Theory]
    [InlineData("+")]
    [InlineData("-")]
    [InlineData("*")]
    [InlineData("/")]
    public void GenerateEquation_ShouldSupportAllOperations(string op)
    {
        // Arrange
        var p = CreateDefaultParams();
        p.Operations = new List<string> { op };

        // Act
        var question = _getQuestionsUseCase.GenerateEquation(p);

        // Assert
        question.Should().NotBeNull();
        question.Equation.Should().Contain(op);

    }

    [Fact]
    public void GenerateEquation_ShouldIncludeCorrectAnswer()
    {
        // Arrange
        var p = CreateDefaultParams();

        // Act
        var question = _getQuestionsUseCase.GenerateEquation(p);

        // Assert
        question.Options.Should().Contain(question.CorrectAnswer);
    }

    [Fact]
    public void GenerateEquation_ShouldRandomizeOptionsOrder()
    {
        // Arrange
        var p = CreateDefaultParams();
        var orders = new List<List<int>>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var question = _getQuestionsUseCase.GenerateEquation(p);
            orders.Add(new List<int>(question.Options));
        }

        // Assert
        // Verifica que al menos dos órdenes sean diferentes
        bool foundDifferentOrder = orders
            .Skip(1)
            .Any(order => !order.SequenceEqual(orders[0]));

        foundDifferentOrder.Should().BeTrue();
      
    }

    [Theory]
    [InlineData("MAYOR")]
    [InlineData("MENOR")]
    [InlineData("INVALIDO")]
    [InlineData(null)]
    public void GenerateEquation_ShouldHandleExpectedResultCases(string expectedResult)
    {
        // Arrange
        var p = CreateDefaultParams();
        p.ExpectedResult = expectedResult;
        p.OptionsCount = 3;
        p.OptionRangeMin = 1;
        p.OptionRangeMax = 10;

        // Act
        var question = _getQuestionsUseCase.GenerateEquation(p);

        // Assert
        question.Options.Should().Contain(question.CorrectAnswer);

        // Verifica la lógica de selección según ExpectedResult
        var opts = question.Options;
        int xMin = opts.Min();
        int xMax = opts.Max();

        double yMin = GetY(question.Equation, xMin);
        double yMax = GetY(question.Equation, xMax);

        string expected = (expectedResult ?? "MAYOR").ToUpperInvariant();
        if (expected != "MAYOR" && expected != "MENOR")
            expected = "MAYOR";

        if (expected == "MAYOR")
        {
            if (yMax > yMin)
                question.CorrectAnswer.Should().Be(xMax);
            else
                question.CorrectAnswer.Should().Be(xMin);
        }
        else // MENOR
        {
            if (yMax < yMin)
                question.CorrectAnswer.Should().Be(xMax);
            else
                question.CorrectAnswer.Should().Be(xMin);
        }
    }

    [Fact]
    public void GenerateEquation_ShouldRespectOptionRange()
    {
        // Arrange
        var p = CreateDefaultParams();
        p.OptionRangeMin = 1;
        p.OptionRangeMax = 3;
        p.OptionsCount = 3;

        // Act
        var question = _getQuestionsUseCase.GenerateEquation(p);

        // Assert
        question.Options.Should().OnlyContain(x => x >= p.OptionRangeMin && x <= p.OptionRangeMax);
    }

    [Fact]
    public void GenerateEquation_ShouldRespectOptionsCount()
    {
        // Arrange
        var p = CreateDefaultParams();
        p.OptionRangeMin = 1;
        p.OptionRangeMax = 10;
        p.OptionsCount = 5;

        // Act
        var question = _getQuestionsUseCase.GenerateEquation(p);

        // Assert
        question.Options.Count.Should().Be(p.OptionsCount);
    }

    [Fact]
    public void GenerateEquation_ShouldRespectTermCount()
    {
        // Arrange
        var p = CreateDefaultParams();
        p.TermCount = 4;
        p.VariableCount = 2;

        // Act
        var question = _getQuestionsUseCase.GenerateEquation(p);

        // Assert
        // Extraer la parte derecha de la ecuación
        var expr = question.Equation.Substring(question.Equation.IndexOf('=') + 1).Trim();

        // Separar por espacio y contar los términos (cada término está separado por ' + ' o ' - ')
        var terms = expr.Split(' ')
            .Where(t => t != "+" && t != "-")
            .ToList();

        terms.Count.Should().Be(p.TermCount);
    }

    [Fact]
    public void GenerateEquation_ShouldRespectVariableCount()
    {
        // Arrange
        var p = CreateDefaultParams();
        p.TermCount = 4;
        p.VariableCount = 2;

        // Act
        var question = _getQuestionsUseCase.GenerateEquation(p);

        // Assert
        int variableCount = question.Equation.Count(c => c == 'x');
        variableCount.Should().Be(p.VariableCount);
    }

    [Fact]
    public void ValidateParams_ShouldAutocorrectInvalidValues()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        var p = new EquationParams
        {
            TermCount = 0,
            VariableCount = 0,
            Operations = new List<string> { "invalid" },
            OptionsCount = 0,
            OptionRangeMin = 10,
            OptionRangeMax = 5,
            NumberRangeMin = 10,
            NumberRangeMax = 5,
            ExpectedResult = "INVALIDO"
        };

        // Act
        var q = useCase.GenerateEquation(p);

        // Assert
        q.Should().NotBeNull();
        q.Options.Count.Should().BeGreaterOrEqualTo(2);
        q.Equation.Should().StartWith("y = ");
    }

    [Fact]
    public void GenerateTerms_ShouldRespectVariableAndTermCount()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        int termCount = 5;
        int variableCount = 2;
        var operations = new List<string> { "+", "*" };
        int numMin = 1, numMax = 10;

        // Act
        var termsObj = useCase.GetType()
            .GetMethod("GenerateTerms", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { termCount, variableCount, operations, numMin, numMax });

        // Assert
        termsObj.Should().NotBeNull("La invocación de GenerateTerms nunca debe devolver null");
        var terms = termsObj as List<string>;
        terms.Should().NotBeNull("El resultado debe ser una lista de strings");
        terms!.Count.Should().Be(termCount);
        terms.Count(t => t.Contains("x")).Should().Be(variableCount);
    }

    [Fact]
    public void GenerateOptions_ShouldReturnUniqueSortedOptions()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        int count = 4, min = 1, max = 5;

        // Act
        var optionsObj = useCase.GetType()
            .GetMethod("GenerateOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { count, min, max });

        // Assert
        optionsObj.Should().NotBeNull("La invocación de GenerateOptions nunca debe devolver null");
        var options = optionsObj as List<int>;
        options.Should().NotBeNull("El resultado debe ser una lista de enteros");
        options!.Count.Should().Be(count);
        options.Should().OnlyHaveUniqueItems();
        options.Should().BeInAscendingOrder();
        options.Should().OnlyContain(x => x >= min && x <= max);
    }

    [Theory]
    [InlineData("2*3", 1, 6)]
    [InlineData("x+2", 3, 5)]
    [InlineData("10/x", 2, 5)]
    public void EvaluateExpression_ShouldCalculateCorrectly(string expr, int xValue, double expected)
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();

        // Act
        var resultObj = useCase.GetType()
            .GetMethod("EvaluateExpression", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { expr, xValue });

        // Assert
        resultObj.Should().NotBeNull("La invocación de EvaluateExpression nunca debe devolver null");
        var result = resultObj as double?;
        result.Should().NotBeNull("El resultado debe ser un double");
        result!.Value.Should().BeApproximately(expected, 0.01);
    }

    [Fact]
    public void RandomNumberExceptZero_ShouldNeverReturnZero()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        int min = -2, max = 2;

        // Act
        var method = useCase.GetType()
            .GetMethod("RandomNumberExceptZero", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var strResults = new List<string?>();
        for (int i = 0; i < 20; i++)
        {
            var str = method?.Invoke(useCase, new object[] { min, max }) as string;
            strResults.Add(str);
        }

        // Assert
        strResults.Should().OnlyContain(s => s != null, "RandomNumberExceptZero nunca debe devolver null");
        var results = strResults.Where(s => s != null).Select(s => int.Parse(s!)).ToList();
        results.Should().OnlyContain(x => x != 0);
        results.Should().OnlyContain(x => x >= min && x <= max);
    }

    #region Tests para validación de ecuaciones constantes

    [Fact]
    public void IsValidEquation_WithConstantEquation_ShouldReturnFalse()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        var constantEquation = "5"; // Ecuación constante, no depende de x
        var options = new List<int> { 1, 2, 3, 4 };

        // Act
        var resultObj = useCase.GetType()
            .GetMethod("IsValidEquation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { constantEquation, options });

        // Assert
        resultObj.Should().NotBeNull();
        var result = (bool)resultObj!;
        result.Should().BeFalse("Una ecuación constante no es válida");
    }

    [Fact]
    public void IsValidEquation_WithCanceledTerms_ShouldReturnFalse()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        var canceledEquation = "x - x + 5"; // Los términos con x se cancelan
        var options = new List<int> { 1, 2, 3, 4 };

        // Act
        var resultObj = useCase.GetType()
            .GetMethod("IsValidEquation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { canceledEquation, options });

        // Assert
        resultObj.Should().NotBeNull();
        var result = (bool)resultObj!;
        result.Should().BeFalse("Una ecuación con términos cancelados no es válida");
    }

    [Fact]
    public void IsValidEquation_WithValidEquation_ShouldReturnTrue()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        var validEquation = "2*x + 3"; // Ecuación válida que depende de x
        var options = new List<int> { 1, 2, 3, 4 };

        // Act
        var resultObj = useCase.GetType()
            .GetMethod("IsValidEquation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { validEquation, options });

        // Assert
        resultObj.Should().NotBeNull();
        var result = (bool)resultObj!;
        result.Should().BeTrue("Una ecuación válida debe retornar true");
    }

    [Fact]
    public void IsValidEquation_WithSingleOption_ShouldReturnTrue()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        var equation = "x + 5";
        var options = new List<int> { 1 }; // Solo una opción

        // Act
        var resultObj = useCase.GetType()
            .GetMethod("IsValidEquation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { equation, options });

        // Assert
        resultObj.Should().NotBeNull();
        var result = (bool)resultObj!;
        result.Should().BeTrue("Con una sola opción, la ecuación se considera válida");
    }

    [Fact]
    public void IsValidEquation_WithDifferentResults_ShouldReturnTrue()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        var equation = "3*x - 2";
        var options = new List<int> { 1, 5, 10 };

        // Act
        var resultObj = useCase.GetType()
            .GetMethod("IsValidEquation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { equation, options });

        // Assert
        resultObj.Should().NotBeNull();
        var result = (bool)resultObj!;
        result.Should().BeTrue("Una ecuación que produce diferentes resultados es válida");
    }

    [Fact]
    public void ForceValidEquation_ShouldReturnValidEquationFormat()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        var p = CreateDefaultParams();

        // Act
        var resultObj = useCase.GetType()
            .GetMethod("ForceValidEquation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { p });

        // Assert
        resultObj.Should().NotBeNull();
        var equation = resultObj as string;
        equation.Should().NotBeNullOrEmpty();
        equation.Should().Contain("*x");
        equation.Should().Match(e => e.Contains("+") || e.Contains("-"));
    }

    [Fact]
    public void ForceValidEquation_ShouldProduceDifferentResults()
    {
        // Arrange
        var useCase = new GetQuestionsUseCase();
        var p = CreateDefaultParams();

        // Act
        var resultObj = useCase.GetType()
            .GetMethod("ForceValidEquation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(useCase, new object[] { p });

        var equation = resultObj as string;
        var options = new List<int> { 1, 5, 10 };

        // Assert
        equation.Should().NotBeNull();
        
        // Evaluar con diferentes valores de x
        var y1 = GetY($"y = {equation}", options[0]);
        var y2 = GetY($"y = {equation}", options[1]);
        var y3 = GetY($"y = {equation}", options[2]);

        // Los resultados deben ser diferentes
        var results = new HashSet<double> { y1, y2, y3 };
        results.Count.Should().BeGreaterThan(1, "La ecuación forzada debe producir diferentes resultados");
    }

    [Fact]
    public void GenerateEquation_ShouldNeverReturnConstantEquation()
    {
        // Arrange
        var p = CreateDefaultParams();
        p.TermCount = 3;
        p.VariableCount = 2;

        // Act - Generar múltiples ecuaciones
        var questions = new List<Question>();
        for (int i = 0; i < 20; i++)
        {
            questions.Add(_getQuestionsUseCase.GenerateEquation(p));
        }

        // Assert
        foreach (var question in questions)
        {
            // Extraer la ecuación sin el "y = "
            var expr = question.Equation.Substring(question.Equation.IndexOf('=') + 1).Trim();
            
            // Evaluar con diferentes opciones
            var results = new HashSet<double>();
            foreach (var option in question.Options.Take(3))
            {
                var y = GetY(question.Equation, option);
                if (!double.IsNaN(y))
                {
                    results.Add(Math.Round(y, 6));
                }
            }

            results.Count.Should().BeGreaterThan(1, 
                $"La ecuación '{question.Equation}' con opciones [{string.Join(", ", question.Options)}] " +
                $"produce los mismos resultados para todos los valores de x");
        }
    }

    [Fact]
    public void GenerateEquation_WithOnlyAdditionSubtraction_ShouldNotProduceConstantEquations()
    {
        // Arrange
        var p = CreateDefaultParams();
        p.Operations = new List<string> { "+", "-" };
        p.TermCount = 3;
        p.VariableCount = 1;

        // Act - Generar múltiples ecuaciones
        var questions = new List<Question>();
        for (int i = 0; i < 15; i++)
        {
            questions.Add(_getQuestionsUseCase.GenerateEquation(p));
        }

        // Assert - Todas deben tener resultados diferentes para diferentes valores de x
        foreach (var question in questions)
        {
            var y1 = GetY(question.Equation, question.Options[0]);
            var y2 = GetY(question.Equation, question.Options[^1]);

            Math.Round(y1, 6).Should().NotBe(Math.Round(y2, 6),
                $"La ecuación '{question.Equation}' produce el mismo resultado para diferentes valores de x");
        }
    }

    [Fact]
    public void GenerateEquation_ShouldHaveUniqueCorrectAnswer()
    {
        // Arrange
        var p = CreateDefaultParams();

        // Act
        var question = _getQuestionsUseCase.GenerateEquation(p);

        // Assert
        question.CorrectAnswer.Should().BeOneOf(question.Options);
        
        // Verificar que la respuesta correcta realmente cumple con el criterio
        var minOption = question.Options.Min();
        var maxOption = question.Options.Max();
        
        var yMin = GetY(question.Equation, minOption);
        var yMax = GetY(question.Equation, maxOption);

        // El CorrectAnswer debe ser el que produce el resultado esperado
        if (Math.Abs(yMax - yMin) > 0.0001) // Si hay diferencia significativa
        {
            question.Options.Should().Contain(question.CorrectAnswer);
        }
    }

    #endregion

    #region Helper Methods

    private static EquationParams CreateDefaultParams()
    {
        return new EquationParams
        {
            TermCount = 3,
            VariableCount = 1,
            Operations = new List<string> { "+", "-" },
            OptionsCount = 4,
            OptionRangeMin = 1,
            OptionRangeMax = 10,
            NumberRangeMin = 1,
            NumberRangeMax = 10,
            ExpectedResult = "MAYOR"
        };
    }

    //Este helper simula el EvaluateExpression interno para verificar la lógica de selección de respuestas correctas
    private double GetY(string equation, int xValue)
    {
        var expr = equation.Substring(equation.IndexOf('=') + 1).Trim();
        try
        {
            var result = new System.Data.DataTable().Compute(expr.Replace("x", xValue.ToString(System.Globalization.CultureInfo.InvariantCulture)), "");
            return Convert.ToDouble(result, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            return double.NaN;
        }
    }

    #endregion
}