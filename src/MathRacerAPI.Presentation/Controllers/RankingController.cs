using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/ranking")]

public class RankingController : ControllerBase
{
    private readonly IGetPlayerRankingUseCase _getPlayerRankingUseCase;

    public RankingController(IGetPlayerRankingUseCase getPlayerRankingUseCase)
    {
        _getPlayerRankingUseCase = getPlayerRankingUseCase;
    }

    [SwaggerOperation(
        Summary = "Obtiene el top 10 del ranking y la posición del jugador especificado",
        Description = "Retorna la clasificación de los 10 mejores jugadores por puntos junto con la posición actual del jugador consultado en el ranking global.",
        OperationId = "GetRanking",
        Tags = new[] { "Ranking - Clasificaciones" }
    )]
    [SwaggerResponse(200, "Ranking obtenido exitosamente.", typeof(RankingTop10ResponseDto))]
    [SwaggerResponse(404, "Jugador no encontrado en el ranking.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpGet]
    public async Task<ActionResult<RankingTop10ResponseDto>> GetRanking([FromQuery] int playerId)
    {
        var (top10, position) = await _getPlayerRankingUseCase.ExecuteAsync(playerId);
        if (position <= 0)
        {
            return NotFound(new { message = $"El jugador con id {playerId} no existe en el ranking." });
        }
        var dto = new RankingTop10ResponseDto
        {
            Top10 = top10.Select((p, i) => new PlayerRankingDto
            {
                Position = i + 1,
                PlayerId = p.Id,
                Name = p.Name,
                Points = p.Points
            }).ToList(),
            CurrentPlayerPosition = position
        };
        return Ok(dto);
    }
}
