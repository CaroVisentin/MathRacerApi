using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Services;
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
        /// Obtiene todos los mundos del juego y el progreso del jugador autenticado
        /// </summary>
        /// <returns>Todos los mundos disponibles y el último mundo accesible por el jugador</returns>
        /// <response code="200">Operación exitosa. Retorna todos los mundos y el último mundo disponible.</response>
        /// <response code="401">No autorizado. Token inválido o faltante.</response>
        /// <response code="404">Jugador no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// 
        ///     GET /api/worlds/player
        ///     Headers:
        ///       Authorization: Bearer {firebase-id-token}
        /// 
        /// **Descripción:**
        /// 
        /// Este endpoint retorna:
        /// - **Todos los mundos** del juego con sus configuraciones
        /// - **LastAvailableWorldId**: El ID del último mundo al que el jugador tiene acceso
        /// 
        /// **Seguridad:**
        /// - Requiere token de Firebase en el header `Authorization`
        /// - El endpoint identifica automáticamente al jugador por su UID de Firebase
        /// - No necesita pasar el playerId en la URL
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
        /// Error 401 (Sin token):
        /// 
        ///     {
        ///       "statusCode": 401,
        ///       "message": "Token de autenticación requerido."
        ///     }
        /// 
        /// Error 401 (Token inválido):
        /// 
        ///     {
        ///       "statusCode": 401,
        ///       "message": "Token de Firebase inválido."
        ///     }
        /// 
        /// Error 404 (NotFoundException):
        /// 
        ///     {
        ///       "statusCode": 404,
        ///       "message": "Jugador no encontrado. Por favor, regístrate primero."
        ///     }   
        ///     
        /// </remarks>

        [HttpGet]
        [ProducesResponseType(typeof(PlayerWorldsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPlayerWorlds()
        {
            // 1. Obtener el UID validado del contexto (ya validado por el middleware)
            var uid = HttpContext.Items["FirebaseUid"] as string;

            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
            }

            // 2. Ejecutar el caso de uso con el UID validado
            var playerWorlds = await _getWorldsUseCase.ExecuteByUidAsync(uid);

            // 3. Mapear respuesta
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