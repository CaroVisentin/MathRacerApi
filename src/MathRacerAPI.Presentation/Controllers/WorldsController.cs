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

    public class WorldsController : ControllerBase
    {
        private readonly GetWorldsUseCase _getWorldsUseCase;

        public WorldsController(GetWorldsUseCase getWorldsUseCase)
        {
            _getWorldsUseCase = getWorldsUseCase;
        }

        [SwaggerOperation(
            Summary = "Obtiene todos los mundos del juego y el progreso del jugador autenticado",
            Description = "Este endpoint retorna todos los mundos del juego con sus configuraciones y el ID del último mundo al que el jugador tiene acceso. Requiere token de Firebase en el header Authorization.",
            OperationId = "GetPlayerWorlds",
            Tags = new[] { "Worlds - Mundos del juego" }
        )]
        [SwaggerResponse(200, "Operación exitosa. Retorna todos los mundos y el último mundo disponible.", typeof(PlayerWorldsResponseDto))]
        [SwaggerResponse(401, "No autorizado. Token inválido o faltante.")]
        [SwaggerResponse(404, "Jugador no encontrado.")]
        [SwaggerResponse(500, "Error interno del servidor.")]
        [HttpGet]
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
