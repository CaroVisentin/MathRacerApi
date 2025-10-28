using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Controllers
{
    /// <summary>
    /// Controlador para gestionar los mundos del juego
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class WorldsController : ControllerBase
    {
        private readonly GetWorldsUseCase _getWorldsUseCase;

        public WorldsController(GetWorldsUseCase getWorldsUseCase)
        {
            _getWorldsUseCase = getWorldsUseCase;
        }

        /// <summary>
        /// Obtiene todos los mundos del juego y el progreso del jugador
        /// </summary>
        /// <param name="playerId">ID del jugador</param>
        /// <returns>Todos los mundos disponibles y el último mundo accesible por el jugador</returns>
        /// <response code="200">Operación exitosa. Retorna todos los mundos y el último mundo disponible.</response>
        /// <response code="400">Solicitud inválida. El ID del jugador debe ser mayor a 0.</response>
        /// <response code="404">Jugador no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// 
        ///     GET /api/worlds/player/1
        /// 
        /// **Descripción:**
        /// 
        /// Este endpoint retorna:
        /// - **Todos los mundos** del juego con sus configuraciones
        /// - **LastAvailableWorldId**: El ID del último mundo al que el jugador tiene acceso
        /// 
        /// **Ejemplo de respuesta exitosa (200):**
        /// 
        ///     {
        ///       "worlds": [
        ///         {
        ///           "id": 1,
        ///           "name": "Mundo 1",
        ///           "difficulty": "Fácil",
        ///           "timePerEquation": 10,
        ///           "operations": ["+", "-"],
        ///           "optionsCount": 2,
        ///         },
        ///         {
        ///           "id": 2,
        ///           "name": "Mundo 2",
        ///           "difficulty": "Medio",
        ///           "timePerEquation": 10,
        ///           "operations": ["*", "/"],
        ///           "optionsCount": 4,
        ///         }
        ///       ],
        ///       "lastAvailableWorldId": 1
        ///     }
        ///     
        /// **Posibles errores:**
        /// 
        /// Error 400 (ValidationException):
        /// 
        ///     {
        ///       "statusCode": 400,
        ///       "message": "El ID del jugador debe ser mayor a 0.",
        ///     }
        /// 
        /// Error 400 (BusinessException):
        /// 
        ///     {
        ///       "statusCode": 400,
        ///       "message": "No se encontraron mundos disponibles para el jugador Juan.",
        ///     }
        /// 
        /// Error 404 (NotFoundException):
        /// 
        ///     {
        ///       "statusCode": 404,
        ///       "message": "Jugador con ID 123 no fue encontrado.",
        ///     }   
        ///     
        /// 
        /// </remarks>
        [HttpGet("player/{playerId}")]
        [ProducesResponseType(typeof(PlayerWorldsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPlayerWorlds(int playerId)
        {
            var playerWorlds = await _getWorldsUseCase.ExecuteAsync(playerId);

            var response = new PlayerWorldsResponseDto
            {
                Worlds = playerWorlds.Worlds.Select(w => new WorldDto
                {
                    Id = w.Id,
                    Name = w.Name,
                    Difficulty = w.Difficulty,
                    TimePerEquation = w.TimePerEquation,
                    Operations = w.Operations,
                    OptionsCount = w.OptionsCount,
                }).ToList(),
                LastAvailableWorldId = playerWorlds.LastAvailableWorldId
            };

            return Ok(response);
        }
    }
}