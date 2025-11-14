using MathRacerAPI.Domain.Models;
using MathRacerAPI.Presentation.DTOs.Infinite;

namespace MathRacerAPI.Presentation.Mappers;

public static class InfiniteGameMapper
{
    public static StartInfiniteGameResponseDto ToStartResponseDto(InfiniteGame game)
    {
        return new StartInfiniteGameResponseDto
        {
            GameId = game.Id,
            PlayerName = game.PlayerName,
            Questions = game.Questions.Select(q => new InfiniteQuestionDto
            {
                QuestionId = q.Id,
                Equation = q.Equation,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                ExpectedResult = q.ExpectedResult
            }).ToList(),
            TotalCorrectAnswers = game.CorrectAnswers,
            CurrentBatch = game.CurrentBatch
        };
    }

    public static SubmitInfiniteAnswerResponseDto ToSubmitAnswerResponseDto(InfiniteAnswerResult result)
    {
        return new SubmitInfiniteAnswerResponseDto
        {
            IsCorrect = result.IsCorrect,
            CorrectAnswer = result.CorrectAnswer,
            TotalCorrectAnswers = result.TotalCorrectAnswers,
            CurrentQuestionIndex = result.CurrentQuestionIndex,
            NeedsNewBatch = result.NeedsNewBatch
        };
    }

    public static LoadNextBatchResponseDto ToLoadNextBatchResponseDto(InfiniteGame game)
    {
        return new LoadNextBatchResponseDto
        {
            GameId = game.Id,
            Questions = game.Questions.Select(q => new InfiniteQuestionDto
            {
                QuestionId = q.Id,
                Equation = q.Equation,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                ExpectedResult = q.ExpectedResult
            }).ToList(),
            CurrentBatch = game.CurrentBatch,
            TotalCorrectAnswers = game.CorrectAnswers
        };
    }

    public static InfiniteGameStatusResponseDto ToStatusResponseDto(InfiniteGame game)
    {
        return new InfiniteGameStatusResponseDto
        {
            GameId = game.Id,
            PlayerName = game.PlayerName,
            TotalCorrectAnswers = game.CorrectAnswers,
            CurrentQuestionIndex = game.CurrentQuestionIndex,
            CurrentBatch = game.CurrentBatch,
            IsActive = game.IsActive,
            GameStartedAt = game.GameStartedAt,
            AbandonedAt = game.AbandonedAt
        };
    }
}