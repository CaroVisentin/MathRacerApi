using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Controllers
{
    /// <summary>
    /// Controlador para gestionar los niveles del juego
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LevelsController : ControllerBase
    {
        private readonly GetWorldLevelsUseCase _getWorldLevelsUseCase;

        public LevelsController(GetWorldLevelsUseCase getWorldLevelsUseCase)
        {
            _getWorldLevelsUseCase = getWorldLevelsUseCase;
        }

        /// <summary>
        /// Obtiene todos los niveles de un mundo y el último nivel completado por el jugador
        /// </summary>
        /// <param name="worldId">ID del mundo</param>
        /// <param name="playerId">ID del jugador</param>
        /// <returns>Todos los niveles del mundo y el ID del último nivel completado</returns>
        /// <response code="200">Operación exitosa. Retorna todos los niveles y el último completado.</response>
        /// <response code="400">Solicitud inválida. IDs deben ser mayores a 0.</response>
        /// <response code="404">Jugador o mundo no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// 
        ///     GET /api/levels/world/1/player/123
        /// 
        /// **Descripción:**
        /// 
        /// Este endpoint retorna:
        /// - **WorldName**: Nombre del mundo consultado
        /// - **Levels**: Lista completa de niveles del mundo ordenados por número
        /// - **LastCompletedLevelId**: ID del último nivel completado por el jugador en este mundo (0 si no ha completado ninguno)
        /// 
        /// **Ejemplo de respuesta exitosa (200):**
        /// 
        ///     {
        ///       "worldName": "Mundo 1",
        ///       "levels": [
        ///         {
        ///           "id": 1,
        ///           "worldId": 1,
        ///           "number": 1,
        ///           "termsCount": 2,
        ///           "variablesCount": 1,
        ///           "resultType": "Mayor"
        ///         },
        ///         {
        ///           "id": 2,
        ///           "worldId": 1,
        ///           "number": 2,
        ///           "termsCount": 2,
        ///           "variablesCount": 1,
        ///           "resultType": "Menor"
        ///         },
        ///         {
        ///           "id": 3,
        ///           "worldId": 1,
        ///           "number": 3,
        ///           "termsCount": 2,
        ///           "variablesCount": 1,
        ///           "resultType": "Mayor"
        ///         }
        ///       ],
        ///       "lastCompletedLevelId": 1
        ///     }
        ///     
        /// **Posibles errores:**
        /// 
        /// Error 400 (ValidationException - worldId inválido):
        /// 
        ///     {
        ///       "statusCode": 400,
        ///       "message": "El ID del mundo debe ser mayor a 0",
        ///     }
        /// 
        /// Error 400 (ValidationException - playerId inválido):
        /// 
        ///     {
        ///       "statusCode": 400,
        ///       "message": "El ID del jugador debe ser mayor a 0",
        ///     }
        /// 
        /// Error 404 (NotFoundException - jugador):
        /// 
        ///     {
        ///       "statusCode": 404,
        ///       "message": "Jugador con ID 999 no fue encontrado",
        ///     }
        /// 
        /// Error 404 (NotFoundException - mundo):
        /// 
        ///     {
        ///       "statusCode": 404,
        ///       "message": "Mundo con ID 999 no fue encontrado",
        ///     }
        /// 
        /// Error 400 (BusinessException - sin niveles):
        /// 
        ///     {
        ///       "statusCode": 400,
        ///       "message": "El mundo 'Mundo Test' no tiene niveles configurados",
        ///     }
        /// 
        /// Error 500 (Error de base de datos):
        /// 
        ///     {
        ///       "statusCode": 500,
        ///       "message": "Ocurrió un error interno en el servidor.",
        ///     }
        /// 
        /// </remarks>
        [HttpGet("world/{worldId}/player/{playerId}")]
        [ProducesResponseType(typeof(PlayerWorldLevelsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorldLevels(int worldId, int playerId)
        {
            var playerWorldLevels = await _getWorldLevelsUseCase.ExecuteAsync(playerId, worldId);

            var response = new PlayerWorldLevelsResponseDto
            {
                WorldName = playerWorldLevels.WorldName,
                Levels = playerWorldLevels.Levels.Select(l => new LevelDto
                {
                    Id = l.Id,
                    WorldId = l.WorldId,
                    Number = l.Number,
                    TermsCount = l.TermsCount,
                    VariablesCount = l.VariablesCount,
                    ResultType = l.ResultType
                }).ToList(),
                LastCompletedLevelId = playerWorldLevels.LastCompletedLevelId,
            };

            return Ok(response);
        }
    }
}
