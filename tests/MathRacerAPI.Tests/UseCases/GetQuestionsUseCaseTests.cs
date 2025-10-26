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