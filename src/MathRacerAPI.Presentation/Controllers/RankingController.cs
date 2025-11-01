using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/ranking")]
[Tags("Ranking")]
public class RankingController : ControllerBase
{
    private readonly IGetPlayerRankingUseCase _getPlayerRankingUseCase;

    public RankingController(IGetPlayerRankingUseCase getPlayerRankingUseCase)
    {
        _getPlayerRankingUseCase = getPlayerRankingUseCase;
    }

    /// <summary>
    /// Devuelve el top 10 de jugadores y la posición del jugador actual en el ranking.
    /// </summary>
    /// <remarks>
    /// **Ejemplo de solicitud:**
    ///
    ///     GET /api/ranking?playerId=123
    ///
    /// **Descripción:**
    ///
    /// Devuelve una lista con el top 10 de jugadores y la posición del jugador actual.
    ///
    /// **Ejemplo de respuesta exitosa (200):**
    ///
    ///     {
    ///       "top10": [
    ///         {
    ///           "position": 1,
    ///           "playerId": 123,
    ///           "name": "Juan",
    ///           "points": 150
    ///         },
    ///         {
    ///           "position": 2,
    ///           "playerId": 456,
    ///           "name": "Ana",
    ///           "points": 120
    ///         }
    ///       ],
    ///       "currentPlayerPosition": 5
    ///     }
    ///
    /// **Posibles errores:**
    ///
    /// Error 404 (Jugador no encontrado):
    ///
    ///     {
    ///       "message": "El jugador con id 999 no existe en el ranking."
    ///     }
    ///
    /// Error 500 (Error interno):
    ///
    ///     {
    ///       "statusCode": 500,
    ///       "message": "Error interno del servidor."
    ///     }
    /// </remarks>
    /// <response code="200">Top 10 y posición del jugador</response>
    /// <response code="404">Jugador no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(typeof(RankingTop10ResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
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
