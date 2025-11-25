using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Services;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Controllers
{
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

        [SwaggerOperation(
            Summary = "Obtiene todos los niveles de un mundo y el progreso del jugador",
            Description = "Retorna todos los niveles de un mundo específico junto con el ID del último nivel completado por el jugador autenticado. Solo permite acceso a mundos desbloqueados según el progreso del jugador.",
            OperationId = "GetWorldLevels",
            Tags = new[] { "Levels - Niveles del juego" }
        )]
        [SwaggerResponse(200, "Operación exitosa. Retorna todos los niveles del mundo y el progreso del jugador.", typeof(PlayerWorldLevelsResponseDto))]
        [SwaggerResponse(400, "Solicitud inválida. El ID del mundo debe ser mayor a 0 o el mundo no está desbloqueado.")]
        [SwaggerResponse(401, "No autorizado. Token de Firebase inválido o faltante.")]
        [SwaggerResponse(404, "Jugador o mundo no encontrado.")]
        [SwaggerResponse(500, "Error interno del servidor.")]
        [HttpGet("world/{worldId}")]
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
                    ResultType = l.ResultType,
                    IsCompleted = l.Id <= playerWorldLevels.LastCompletedLevelId
                }).ToList(),
                LastCompletedLevelId = playerWorldLevels.LastCompletedLevelId,
            };

            return Ok(response);
        }
    }
}
