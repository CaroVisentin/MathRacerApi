using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controlador para gestionar jugadores
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Player")]
public class PlayerController : ControllerBase
{
    private readonly CreatePlayerUseCase _createPlayerUseCase;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;

    public PlayerController(
        CreatePlayerUseCase createPlayerUseCase,
        GetPlayerByIdUseCase getPlayerByIdUseCase)
    {
        _createPlayerUseCase = createPlayerUseCase;
        _getPlayerByIdUseCase = getPlayerByIdUseCase;
    }

    /// <summary>
    /// Crea un nuevo jugador en el sistema
    /// </summary>
    /// <param name="request">Datos del jugador a crear</param>
    /// <returns>Perfil del jugador creado</returns>
    /// <response code="201">Jugador creado exitosamente</response>
    /// <response code="400">Datos de entrada inválidos o email ya registrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/player
    ///     {
    ///       "username": "JuanPerez",
    ///       "email": "juan@example.com",
    ///       "uid": "firebase-uid-123"
    ///     }
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint:
    /// - Valida que el username, email y uid no estén vacíos
    /// - Verifica que el email no esté registrado previamente
    /// - Crea el jugador con valores iniciales (LastLevelId=1, Points=0, Coins=0)
    /// - Retorna el perfil completo del jugador creado
    /// 
    /// **Ejemplo de respuesta exitosa (201):**
    /// 
    ///     {
    ///       "id": 123,
    ///       "name": "JuanPerez",
    ///       "email": "juan@example.com",
    ///       "lastLevelId": 1,
    ///       "points": 0,
    ///       "coins": 0
    ///     }
    ///     
    /// **Posibles errores:**
    /// 
    /// Error 400 (ValidationException):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El email 'juan@example.com' ya está registrado",
    ///     }
    /// 
    /// Error 400 (ValidationException - campo usuario requerido):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El nombre de usuario es requerido",
    ///     }
    ///     
    /// Error 400 (ValidationException - campo email requerido):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El email es requerido",
    ///     }
    /// 
    /// Error 400 (ValidationException - campo uid requerido):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El UID es requerido",
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
    /// 

    [HttpPost]
    [ProducesResponseType(typeof(PlayerProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlayerProfileDto>> CreatePlayer([FromBody] CreatePlayerRequestDto request)
    {
        var playerProfile = await _createPlayerUseCase.ExecuteAsync(
            request.Username, 
            request.Email, 
            request.Uid
        );

        var response = new PlayerProfileDto
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            Email = playerProfile.Email,
            LastLevelId = playerProfile.LastLevelId,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins
        };

        return CreatedAtAction(nameof(GetPlayerById), new { id = playerProfile.Id }, response);
    }

    /// <summary>
    /// Obtiene un jugador por su ID
    /// </summary>
    /// <param name="id">ID del jugador</param>
    /// <returns>Perfil del jugador</returns>
    /// <response code="200">Operación exitosa. Retorna el perfil del jugador.</response>
    /// <response code="400">ID inválido (debe ser mayor a 0)</response>
    /// <response code="404">Jugador no encontrado</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/player/123
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint obtiene el perfil completo de un jugador usando su ID.
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "id": 123,
    ///       "name": "JuanPerez",
    ///       "email": "juan@example.com",
    ///       "lastLevelId": 5,
    ///       "points": 1500,
    ///       "coins": 250
    ///     }
    ///     
    /// **Posibles errores:**
    /// 
    /// Error 400 (ValidationException):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El ID del jugador debe ser mayor a 0",
    ///     }
    /// 
    /// Error 404 (NotFoundException):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Jugador con ID 123 no fue encontrado",
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
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlayerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlayerProfileDto>> GetPlayerById(int id)
    {
        var playerProfile = await _getPlayerByIdUseCase.ExecuteAsync(id);

        var response = new PlayerProfileDto
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            Email = playerProfile.Email,
            LastLevelId = playerProfile.LastLevelId,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins
        };

        return Ok(response);
    }

    /// <summary>
    /// Obtiene un jugador por su UID de Firebase
    /// </summary>
    /// <param name="uid">UID de Firebase del jugador</param>
    /// <returns>Perfil del jugador</returns>
    /// <response code="200">Operación exitosa. Retorna el perfil del jugador.</response>
    /// <response code="400">UID inválido (requerido)</response>
    /// <response code="404">Jugador no encontrado con ese UID</response>
    /// <response code="500">Error interno del servidor</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/player/uid/firebase-uid-123
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint se utiliza para obtener el perfil de un jugador usando su UID de Firebase.
    /// Es útil después de la autenticación con Firebase para recuperar los datos del jugador.
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "id": 123,
    ///       "name": "JuanPerez",
    ///       "email": "juan@example.com",
    ///       "lastLevelId": 5,
    ///       "points": 1500,
    ///       "coins": 250
    ///     }
    ///     
    /// **Posibles errores:**
    /// 
    /// Error 400 (ValidationException):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "El UID es requerido",
    ///     }
    /// 
    /// Error 404 (NotFoundException):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "No se encontró un jugador con el UID proporcionado",
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
    /// 

    [HttpGet("uid/{uid}")]
    [ProducesResponseType(typeof(PlayerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlayerProfileDto>> GetPlayerByUid(string uid)
    {
        var playerProfile = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        var response = new PlayerProfileDto
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            Email = playerProfile.Email,
            LastLevelId = playerProfile.LastLevelId,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins
        };

        return Ok(response);
    }
}