using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controller para gestionar los wildcards (comodines) del jugador
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WildcardsController : ControllerBase
{
    private readonly GetPlayerWildcardsUseCase _getPlayerWildcardsUseCase;

    public WildcardsController(GetPlayerWildcardsUseCase getPlayerWildcardsUseCase)
    {
        _getPlayerWildcardsUseCase = getPlayerWildcardsUseCase ?? throw new ArgumentNullException(nameof(getPlayerWildcardsUseCase));
    }

    /// <summary>
    /// Obtiene la lista de wildcards (comodines) disponibles del jugador autenticado
    /// GET /api/wildcards
    /// Requiere token de Firebase en header Authorization
    /// </summary>
    /// <returns>Lista de wildcards del jugador con sus cantidades</returns>
    /// <response code="200">Lista de wildcards obtenida correctamente</response>
    /// <response code="401">No autorizado. Token inválido o faltante</response>
    /// <response code="404">Jugador no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/wildcards
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint:
    /// - Identifica automáticamente al jugador por su UID de Firebase
    /// - Obtiene todos los wildcards que tiene el jugador
    /// - Solo devuelve wildcards con cantidad mayor a 0
    /// - Incluye información del wildcard (nombre, descripción)
    /// 
    /// **Tipos de wildcards disponibles:**
    /// - **Matafuego**: Permite eliminar una opción incorrecta
    /// - **Cambio de rumbo**: Permite cambiar la ecuación
    /// - **Nitro**: La próxima ecuación contará como 2 ecuaciones correctas si se responde correctamente
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     [
    ///       {
    ///         "wildcardId": 1,
    ///         "name": "Matafuego",
    ///         "description": "Permite eliminar una opción incorrecta",
    ///         "quantity": 3
    ///       },
    ///       {
    ///         "wildcardId": 2,
    ///         "name": "Cambio de rumbo",
    ///         "description": "Permite cambiar la ecuación",
    ///         "quantity": 1
    ///       }
    ///     ]
    /// 
    /// **Escenarios comunes:**
    /// 
    /// 1. **Jugador con múltiples wildcards:**
    ///    ```json
    ///    [
    ///      { "wildcardId": 1, "name": "Matafuego", "description": "...", "quantity": 3 },
    ///      { "wildcardId": 2, "name": "Cambio de rumbo", "description": "...", "quantity": 1 }
    ///    ]
    ///    ```
    /// 
    /// 2. **Jugador sin wildcards (lista vacía):**
    ///    ```json
    ///    []
    ///    ```
    /// 
    /// 3. **Jugador nuevo (sin wildcards asignados):**
    ///    ```json
    ///    []
    ///    ```
    /// 
    /// **Flujo recomendado:**
    /// 
    /// 1. Usuario inicia sesión (POST /api/player/login)
    /// 2. Frontend consulta wildcards (GET /api/wildcards)
    /// 3. Frontend muestra UI con wildcards disponibles
    /// 4. Al iniciar nivel, mostrar opciones de wildcards
    /// 5. Al usar wildcard, llamar a POST /api/solo/use-wildcard
    /// 6. Actualizar cantidad localmente o volver a consultar
    /// 
    /// **Notas técnicas:**
    /// 
    /// - Los wildcards se obtienen de la tabla **PlayerWildcard**
    /// - Solo se devuelven wildcards con `Quantity > 0`
    /// - La información del wildcard viene de la tabla **Wildcard**
    /// - El sistema filtra automáticamente wildcards sin cantidad
    /// - Compatible con la lógica de uso de wildcards en partidas solo
    /// 
    /// **Integración con otros endpoints:**
    /// 
    /// - **POST /api/solo/use-wildcard**: Usa un wildcard y reduce cantidad
    /// - **POST /api/chest/open**: Puede otorgar wildcards al jugador
    /// - **GET /api/player/uid/{uid}**: Incluye información completa del jugador
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(List<PlayerWildcardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PlayerWildcardDto>>> GetMyWildcards()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var wildcards = await _getPlayerWildcardsUseCase.ExecuteByUidAsync(uid);

        var dtos = PlayerWildcardMapper.ToDtoList(wildcards);
        return Ok(dtos);
    }
}