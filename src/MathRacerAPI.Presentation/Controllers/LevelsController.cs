using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Services;
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
        private readonly IFirebaseService _firebaseService;

        public LevelsController(
            GetWorldLevelsUseCase getWorldLevelsUseCase,
            IFirebaseService firebaseService)
        {
            _getWorldLevelsUseCase = getWorldLevelsUseCase;
            _firebaseService = firebaseService;
        }

        /// <summary>
        /// Obtiene todos los niveles de un mundo y el último nivel completado por el jugador autenticado
        /// </summary>
        /// <param name="worldId">ID del mundo</param>
        /// <returns>Todos los niveles del mundo y el ID del último nivel completado</returns>
        /// <response code="200">Operación exitosa. Retorna todos los niveles y el último completado.</response>
        /// <response code="400">Solicitud inválida. El ID del mundo debe ser mayor a 0 o el mundo no está desbloqueado.</response>
        /// <response code="401">No autorizado. Token inválido o faltante.</response>
        /// <response code="404">Jugador o mundo no encontrado.</response>
        /// <response code="500">Error interno del servidor.</response>
        /// <remarks>
        /// Ejemplo de solicitud:
        /// 
        ///     GET /api/levels/world/1
        ///     Headers:
        ///       Authorization: Bearer {firebase-id-token}
        /// 
        /// **Descripción:**
        /// 
        /// Este endpoint retorna:
        /// - **WorldName**: Nombre del mundo consultado
        /// - **Levels**: Lista completa de niveles del mundo ordenados por número
        /// - **LastCompletedLevelId**: ID del último nivel completado por el jugador en este mundo
        /// 
        /// **Seguridad:**
        /// - Requiere token de Firebase en el header `Authorization`
        /// - El endpoint identifica automáticamente al jugador por su UID de Firebase
        /// - Solo se puede acceder a mundos desbloqueados según el progreso del jugador
        /// 
        /// **Restricciones de Acceso:**
        /// - Si el jugador está en el mundo 1, solo puede ver niveles del mundo 1
        /// - Si el jugador está en el mundo 2, puede ver mundos 1 y 2
        /// - No se puede acceder a mundos futuros que aún no han sido desbloqueados
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
        ///         }
        ///       ],
        ///       "lastCompletedLevelId": 1
        ///     }
        ///     
        /// **Posibles errores:**
        /// 
        /// Error 400 (BusinessException - mundo no desbloqueado):
        /// 
        ///     {
        ///       "statusCode": 400,
        ///       "message": "No tienes acceso al mundo 3. Completa los niveles del mundo 2 para desbloquearlo."
        ///     }
        /// 
        /// Error 400 (ValidationException - worldId inválido):
        /// 
        ///     {
        ///       "statusCode": 400,
        ///       "message": "El ID del mundo debe ser mayor a 0"
        ///     }
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
        /// Error 404 (NotFoundException - mundo):
        /// 
        ///     {
        ///       "statusCode": 404,
        ///       "message": "Mundo con ID 1 no fue encontrado"
        ///     }
        /// 
        /// Error 404 (NotFoundException - jugador):
        /// 
        ///     {
        ///       "statusCode": 404,
        ///       "message": "Jugador no encontrado. Por favor, regístrate primero."
        ///     }
        /// 
        /// Error 400 (BusinessException - sin niveles):
        /// 
        ///     {
        ///       "statusCode": 400,
        ///       "message": "El mundo 'Mundo 1' no tiene niveles configurados"
        ///     }
        /// 
        /// Error 500 (Error de base de datos):
        /// 
        ///     {
        ///       "statusCode": 500,
        ///       "message": "Ocurrió un error interno en el servidor."
        ///     }
        /// 
        /// </remarks>
        [HttpGet("world/{worldId}")]
        [ProducesResponseType(typeof(PlayerWorldLevelsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetWorldLevels(int worldId)
        {
            // 1. Obtener el UID validado del contexto (ya validado por el middleware)
            var uid = HttpContext.Items["FirebaseUid"] as string;
            
            if (string.IsNullOrEmpty(uid))
            {
                return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
            }

            // 2. Ejecutar el caso de uso con el UID validado
            var playerWorldLevels = await _getWorldLevelsUseCase.ExecuteByUidAsync(uid, worldId);

            // 3. Mapear respuesta
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
