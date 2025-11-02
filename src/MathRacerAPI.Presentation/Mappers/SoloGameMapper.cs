using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs.Solo;
using MathRacerAPI.Presentation.DTOs.SignalR;

namespace MathRacerAPI.Presentation.Mappers;

/// <summary>
/// Extensiones para mapear modelos de dominio del modo individual a DTOs de respuesta
/// </summary>
public static class SoloGameMapper
{
    /// <summary>
    /// Convierte un SoloGame a StartSoloGameResponseDto
    /// </summary>
    public static StartSoloGameResponseDto ToStartGameDto(this SoloGame game)
    {
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
            CurrentQuestion = game.Questions.FirstOrDefault()?.ToSoloQuestionDto(),
            PlayerProducts = game.PlayerProducts.Select(p => p.ToSoloProductDto()).ToList(),
            MachineProducts = game.MachineProducts.Select(p => p.ToSoloProductDto()).ToList()
        };
    }

    /// <summary>
    /// Convierte un SoloGameStatusResult a SoloGameStatusResponseDto
    /// </summary>
    public static SoloGameStatusResponseDto ToStatusDto(this SoloGameStatusResult result)
    {
        return result.Game.ToStatusDto();
    }

    /// <summary>
    /// Convierte un SoloGame a SoloGameStatusResponseDto
    /// </summary>
    public static SoloGameStatusResponseDto ToStatusDto(this SoloGame game)
    {
        // Obtener la pregunta actual basándose en el índice
        Question? currentQuestion = game.CurrentQuestionIndex < game.Questions.Count 
            ? game.Questions[game.CurrentQuestionIndex] 
            : null;

        return new SoloGameStatusResponseDto
        {
            GameId = game.Id,
            Status = game.Status.ToString(),
            PlayerPosition = game.PlayerPosition,
            MachinePosition = game.MachinePosition,
            LivesRemaining = game.LivesRemaining,
            CorrectAnswers = game.CorrectAnswers,
            CurrentQuestion = currentQuestion?.ToQuestionDto(),
            CurrentQuestionIndex = game.CurrentQuestionIndex,
            TotalQuestions = game.TotalQuestions,
            TimePerEquation = game.TimePerEquation,
            GameStartedAt = game.GameStartedAt,
            GameFinishedAt = game.GameFinishedAt,
            ElapsedTime = (DateTime.UtcNow - game.GameStartedAt).TotalSeconds
        };
    }

    /// <summary>
    /// Convierte un SoloAnswerResult a SubmitSoloAnswerResponseDto
    /// </summary>
    public static SubmitSoloAnswerResponseDto ToAnswerDto(this SoloAnswerResult result)
    {
        return new SubmitSoloAnswerResponseDto
        {
            IsCorrect = result.IsCorrect,
            CorrectAnswer = result.CorrectAnswer,
            PlayerAnswer = result.PlayerAnswer,
            Status = result.Game.Status.ToString(),
            LivesRemaining = result.Game.LivesRemaining,
            PlayerPosition = result.Game.PlayerPosition,
            MachinePosition = result.Game.MachinePosition,
            CorrectAnswers = result.Game.CorrectAnswers,
            WaitTimeSeconds = result.Game.ReviewTimeSeconds,
            AnsweredAt = DateTime.UtcNow,
            CurrentQuestionIndex = result.Game.CurrentQuestionIndex,
            ShouldOpenWorldCompletionChest = result.ShouldOpenWorldCompletionChest
        };
    }

    /// <summary>
    /// Convierte una Question a SoloQuestionDto (sin respuesta correcta)
    /// </summary>
    private static SoloQuestionDto ToSoloQuestionDto(this Question question)
    {
        return new SoloQuestionDto
        {
            Id = question.Id,
            Equation = question.Equation,
            Options = question.Options,
            StartedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Convierte una Question a QuestionDto (sin enviar la respuesta correcta al frontend)
    /// </summary>
    private static QuestionDto ToQuestionDto(this Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Equation = question.Equation,
            Options = question.Options,
            CorrectAnswer = 0 // No se envía la respuesta correcta al frontend
        };
    }

    /// <summary>
    /// Convierte un PlayerProduct a SoloProductDto
    /// </summary>
    private static SoloProductDto ToSoloProductDto(this PlayerProduct product)
    {
        return new SoloProductDto
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
}