using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs.Solo;
using MathRacerAPI.Presentation.DTOs.SignalR;
using System;
using System.Linq;
using MathRacerAPI.Domain.UseCases;

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
            ResultType = game.ResultType,
            GameStartedAt = game.GameStartedAt,
            CurrentQuestion = game.Questions.FirstOrDefault()?.ToSoloQuestionDto(),
            PlayerProducts = game.PlayerProducts.Select(p => p.ToSoloProductDto()).ToList(),
            MachineProducts = game.MachineProducts.Select(p => p.ToSoloProductDto()).ToList(),
            AvailableWildcards = game.AvailableWildcards.Select(w => w.ToWildcardDto()).ToList()
        };
    }

    /// <summary>
    /// Convierte un SoloGameStatusResult a SoloGameStatusResponseDto
    /// </summary>
    public static SoloGameStatusResponseDto ToStatusDto(this SoloGameStatusResult result)
    {
        var game = result.Game;
        
        // Obtener la pregunta actual basándose en el índice
        Question? currentQuestion = game.CurrentQuestionIndex < game.Questions.Count 
            ? game.Questions[game.CurrentQuestionIndex] 
            : null;

        QuestionDto? questionDto = null;
        
        if (currentQuestion != null)
        {
            questionDto = currentQuestion.ToQuestionDto();
            
            // Si hay opciones modificadas por el wildcard RemoveWrongOption, aplicarlas
            if (game.ModifiedOptions != null && game.ModifiedOptions.Count > 0)
            {
                questionDto.Options = game.ModifiedOptions;
            }
        }

        return new SoloGameStatusResponseDto
        {
            GameId = game.Id,
            Status = game.Status.ToString(),
            PlayerPosition = game.PlayerPosition,
            MachinePosition = game.MachinePosition,
            LivesRemaining = game.LivesRemaining,
            CorrectAnswers = game.CorrectAnswers,
            CurrentQuestion = questionDto,
            CurrentQuestionIndex = game.CurrentQuestionIndex,
            TotalQuestions = game.TotalQuestions,
            TimePerEquation = game.TimePerEquation,
            GameStartedAt = game.GameStartedAt,
            GameFinishedAt = game.GameFinishedAt,
            ElapsedTime = result.ElapsedTime,
            AvailableWildcards = game.AvailableWildcards.Select(w => w.ToWildcardDto()).ToList(),
            UsedWildcardTypes = game.UsedWildcardTypes.ToList(),
            HasDoubleProgressActive = game.HasDoubleProgressActive,
            ModifiedOptions = game.ModifiedOptions
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
            AnsweredAt = result.Game.LastAnswerTime ?? DateTime.UtcNow,
            CurrentQuestionIndex = result.Game.CurrentQuestionIndex,
            ShouldOpenWorldCompletionChest = result.ShouldOpenWorldCompletionChest,
            ProgressIncrement = result.ProgressIncrement
        };
    }

    /// <summary>
    /// Convierte un WildcardUsageResult a UseWildcardResponseDto
    /// </summary>
    public static UseWildcardResponseDto ToWildcardResponseDto(this WildcardUsageResult result)
    {
        SoloQuestionDto? newQuestion = null;

        // Si se cambió de pregunta (SkipQuestion), obtener la nueva pregunta
        if (result.NewQuestionIndex.HasValue && result.Game != null)
        {
            if (result.NewQuestionIndex.Value < result.Game.Questions.Count)
            {
                newQuestion = result.Game.Questions[result.NewQuestionIndex.Value].ToSoloQuestionDto();
            }
        }

        return new UseWildcardResponseDto
        {
            WildcardId = result.WildcardId,
            Success = result.Success,
            Message = result.Message,
            RemainingQuantity = result.RemainingQuantity,
            ModifiedOptions = result.ModifiedOptions,
            NewQuestionIndex = result.NewQuestionIndex,
            NewQuestion = newQuestion,
            DoubleProgressActive = result.DoubleProgressActive
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
            Options = new List<int>(question.Options), // Crear copia de la lista
            StartedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Convierte una Question a QuestionDto (usado en SignalR)
    /// </summary>
    private static QuestionDto ToQuestionDto(this Question question)
    {
        return new QuestionDto
        {
            Id = question.Id,
            Equation = question.Equation,
            Options = new List<int>(question.Options), // Crear copia de la lista
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

    /// <summary>
    /// Convierte un PlayerWildcard a WildcardDto
    /// </summary>
    private static WildcardDto ToWildcardDto(this PlayerWildcard wildcard)
    {
        return new WildcardDto
        {
            WildcardId = wildcard.WildcardId,
            Name = wildcard.Wildcard.Name,
            Description = wildcard.Wildcard.Description,
            Quantity = wildcard.Quantity
        };
    }
}