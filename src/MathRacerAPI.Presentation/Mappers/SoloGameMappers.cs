using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs.Solo;

namespace MathRacerAPI.Presentation.Mappers;

/// <summary>
/// Extensiones para mapear objetos del dominio Solo a DTOs
/// </summary>
public static class SoloGameMappers
{
    /// <summary>
    /// Mapea una Question del dominio a QuestionDto
    /// </summary>
    public static QuestionDto ToDto(this Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Equation = question.Equation,
            Options = question.Options,
            StartedAt = DateTime.UtcNow // Siempre NOW cuando se entrega la pregunta
        };
    }

    /// <summary>
    /// Mapea un PlayerProduct del dominio a ProductDto
    /// </summary>
    public static ProductDto ToDto(this PlayerProduct product)
    {
        return new ProductDto
        {
            ProductId = product.ProductId,
            Name = product.Name,
            Description = product.Description,
            ProductTypeId = product.ProductTypeId,
            ProductTypeName = product.ProductTypeName,
            RarityId = product.RarityId,
            RarityName = product.RarityName,
            RarityColor = product.RarityColor
        };
    }

    /// <summary>
    /// Mapea una lista de PlayerProducts a ProductDtos
    /// </summary>
    public static List<ProductDto> ToDtoList(this List<PlayerProduct> products)
    {
        return products.Select(p => p.ToDto()).ToList();
    }

    /// <summary>
    /// Mapea un SoloGame a StartSoloGameResponseDto
    /// </summary>
    public static StartSoloGameResponseDto ToStartGameDto(this SoloGame game)
    {
        var currentQuestion = game.Questions.FirstOrDefault();
        
        return new StartSoloGameResponseDto
        {
            GameId = game.Id,
            PlayerId = game.PlayerId,
            PlayerName = game.PlayerName,
            LevelId = game.LevelId,
            TotalQuestions = game.TotalQuestions,
            TimePerEquation = game.TimePerEquation,
            LivesRemaining = game.LivesRemaining,
            GameStartedAt = game.GameStartedAt,
            CurrentQuestion = currentQuestion?.ToDto() ?? new QuestionDto(),
            PlayerProducts = game.PlayerProducts.ToDtoList(),
            MachineProducts = game.MachineProducts.ToDtoList()
        };
    }

    /// <summary>
    /// Mapea un SoloGameStatusResult a SoloGameStatusResponseDto
    /// </summary>
    public static SoloGameStatusResponseDto ToStatusDto(this SoloGameStatusResult result)
    {
        var game = result.Game;
        
        QuestionDto? currentQuestion = null;
        if (game.CurrentQuestionIndex < game.Questions.Count)
        {
            var question = game.Questions[game.CurrentQuestionIndex];
            currentQuestion = question.ToDto(); // Timestamp NOW cuando se solicita
        }

        return new SoloGameStatusResponseDto
        {
            GameId = game.Id,
            Status = game.Status.ToString(),
            PlayerPosition = game.PlayerPosition,
            LivesRemaining = game.LivesRemaining,
            CorrectAnswers = game.CorrectAnswers,
            MachinePosition = game.MachinePosition,
            CurrentQuestion = currentQuestion,
            CurrentQuestionIndex = game.CurrentQuestionIndex,
            TotalQuestions = game.TotalQuestions,
            TimePerEquation = game.TimePerEquation,
            GameStartedAt = game.GameStartedAt,
            GameFinishedAt = game.GameFinishedAt,
            ElapsedTime = result.ElapsedTime
        };
    }

    /// <summary>
    /// Mapea un SoloAnswerResult a SubmitSoloAnswerResponseDto
    /// </summary>
    public static SubmitSoloAnswerResponseDto ToAnswerDto(this SoloAnswerResult result)
    {
        var game = result.Game;

        return new SubmitSoloAnswerResponseDto
        {
            // Feedback de la respuesta
            IsCorrect = result.IsCorrect,
            CorrectAnswer = result.CorrectAnswer,
            PlayerAnswer = result.PlayerAnswer,
            
            // Estado del juego
            Status = game.Status.ToString(),
            LivesRemaining = game.LivesRemaining,
            PlayerPosition = game.PlayerPosition,
            MachinePosition = game.MachinePosition,
            CorrectAnswers = game.CorrectAnswers,
            
            // Información de tiempo
            WaitTimeSeconds = game.ReviewTimeSeconds,
            AnsweredAt = game.LastAnswerTime ?? DateTime.UtcNow,
            
            CurrentQuestionIndex = game.CurrentQuestionIndex,
        };
    }
}