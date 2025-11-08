using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controller para gestionar la energía de los jugadores
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EnergyController : ControllerBase
{
    private readonly GetPlayerEnergyStatusUseCase _getPlayerEnergyStatusUseCase;

    public EnergyController(GetPlayerEnergyStatusUseCase getPlayerEnergyStatusUseCase)
    {
        _getPlayerEnergyStatusUseCase = getPlayerEnergyStatusUseCase ?? throw new ArgumentNullException(nameof(getPlayerEnergyStatusUseCase));
    }

    /// <summary>
    /// Obtiene el estado actual de energía del jugador autenticado
    /// GET /api/energy
    /// Requiere token de Firebase en header Authorization
    /// </summary>
    /// <returns>Estado de energía con cantidad actual y tiempo para próxima recarga</returns>
    /// <response code="200">Devuelve el estado de energía del jugador</response>
    /// <response code="401">No autorizado. Token inválido o faltante</response>
    /// <response code="404">Jugador no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/energy
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint:
    /// - Identifica automáticamente al jugador por su UID de Firebase
    /// - Calcula la energía actual basándose en el tiempo transcurrido desde el último consumo
    /// - Recarga automáticamente 1 punto de energía cada 15 minutos (máximo 3)
    /// - Persiste los cambios en la base de datos si hubo recarga
    /// - Devuelve el tiempo restante en segundos para la próxima recarga
    /// - **Preserva el progreso de recarga** al consumir energía
    /// 
    /// **Lógica de recarga:**
    /// - **Energía máxima:** 3 puntos
    /// - **Tiempo de recarga:** 15 minutos (900 segundos) por punto
    /// - Si está al máximo: `secondsUntilNextRecharge` será `null`
    /// - Si está recargando: `secondsUntilNextRecharge` indica segundos restantes con precisión
    /// 
    /// **Preservación del progreso:**
    /// 
    /// Cuando consumes energía mientras estás recargando, el sistema preserva el progreso:
    /// 
    /// - Tienes 3 energía → pierdes un nivel → baja a 2 (timer: 15 min)
    /// - Esperas 7 minutos → (timer: 8 min restantes)
    /// - Pierdes otro nivel → baja a 1 (timer: 8 min restantes) ← **Progreso preservado**
    /// - El tiempo NO se resetea a 15 minutos
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "currentAmount": 2,
    ///       "maxAmount": 3,
    ///       "secondsUntilNextRecharge": 780
    ///     }
    /// 
    /// **Escenarios comunes:**
    /// 
    /// 1. **Energía completa (no necesita recarga):**
    ///    ```json
    ///    {
    ///      "currentAmount": 3,
    ///      "maxAmount": 3,
    ///      "secondsUntilNextRecharge": null
    ///    }
    ///    ```
    /// 
    /// 2. **Recargando - 7 minutos transcurridos de 15:**
    ///    ```json
    ///    {
    ///      "currentAmount": 2,
    ///      "maxAmount": 3,
    ///      "secondsUntilNextRecharge": 480
    ///    }
    ///    ```
    /// 
    /// 3. **Múltiples recargas - pasaron 35 minutos, tenía 1:**
    ///    ```json
    ///    {
    ///      "currentAmount": 3,
    ///      "maxAmount": 3,
    ///      "secondsUntilNextRecharge": null
    ///    }
    ///    ```
    /// 
    /// 4. **Recarga parcial - 5 minutos de progreso:**
    ///    ```json
    ///    {
    ///      "currentAmount": 1,
    ///      "maxAmount": 3,
    ///      "secondsUntilNextRecharge": 600
    ///    }
    ///    ```
    /// 
    /// **Flujo recomendado:**
    /// 
    /// 1. Usuario inicia sesión (POST /api/player/login)
    /// 2. Frontend consulta energía periódicamente (GET /api/energy)
    /// 3. Frontend muestra UI de energía con timer countdown
    /// 4. Si energía > 0, permite jugar niveles (esto se valida en el back)
    /// 5. Al perder un nivel, backend consume energía automáticamente
    /// 6. Frontend actualiza energía y muestra nuevo timer
    /// 
    /// **Notas técnicas:**
    /// 
    /// - La recarga es **automática y pasiva** (no requiere acción del usuario)
    /// - El cálculo se hace **en tiempo real** al consultar (no con timers)
    /// - Los cambios se **persisten en BD** solo cuando hay recarga
    /// - El tiempo es preciso hasta el **segundo**
    /// - Compatible con **zonas horarias** (usa UTC internamente)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(EnergyStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnergyStatusDto>> GetEnergyStatus()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var energyStatus = await _getPlayerEnergyStatusUseCase.ExecuteByUidAsync(uid);

        var dto = new EnergyStatusDto
        {
            CurrentAmount = energyStatus.CurrentAmount,
            MaxAmount = energyStatus.MaxAmount,
            SecondsUntilNextRecharge = energyStatus.SecondsUntilNextRecharge
        };

        return Ok(dto);
    }

}