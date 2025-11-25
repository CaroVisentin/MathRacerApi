using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Infrastructure.Services;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]

public class PlayerController : ControllerBase
{
    private readonly RegisterPlayerUseCase _registerPlayerUseCase;
    private readonly LoginPlayerUseCase _loginPlayerUseCase;
    private readonly GoogleAuthUseCase _googleAuthUseCase;
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;
    private readonly GetPlayerByEmailUseCase _getPlayerByEmailUseCase;
    private readonly DeletePlayerUseCase _deletePlayerUseCase;

    public PlayerController(
        RegisterPlayerUseCase registerPlayerUseCase,
        LoginPlayerUseCase loginPlayerUseCase,
        GoogleAuthUseCase googleAuthUseCase,
        GetPlayerByIdUseCase getPlayerByIdUseCase,
        GetPlayerByEmailUseCase getPlayerByEmailUseCase,
        DeletePlayerUseCase deletePlayerUseCase
        )
    {
        _registerPlayerUseCase = registerPlayerUseCase;
        _loginPlayerUseCase = loginPlayerUseCase;
        _googleAuthUseCase = googleAuthUseCase;
        _getPlayerByIdUseCase = getPlayerByIdUseCase;
        _getPlayerByEmailUseCase = getPlayerByEmailUseCase;
        _deletePlayerUseCase = deletePlayerUseCase;
    }

    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Registro de usuario con Firebase Authentication",
        Description = "Registra un nuevo usuario en el sistema utilizando Firebase Authentication. El token de Firebase debe enviarse en el header Authorization.",
        OperationId = "RegisterPlayer",
        Tags = new[] { "Player - Gestión de jugadores" }
    )]
    [SwaggerResponse(201, "Usuario registrado exitosamente.", typeof(PlayerProfileDto))]
    [SwaggerResponse(400, "Datos inválidos o email ya registrado.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    public async Task<ActionResult<PlayerProfileDto>> Register([FromBody] RegisterRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Email))
            return BadRequest("Nombre de usuario y email son requeridos.");

        string? idToken = null;
        if (Request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Bearer "))
                idToken = authHeader.Substring("Bearer ".Length).Trim();
        }

        // Si el token está presente, validarlo (puedes adaptar el caso de uso para recibirlo)
        var playerProfile = await _registerPlayerUseCase.ExecuteAsync(request.Username, request.Email, request.Uid, idToken);
        if (playerProfile == null)
            return BadRequest("El email ya está registrado o el token es inválido.");
        var response = new PlayerProfileDto
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            Email = playerProfile.Email,
            LastLevelId = playerProfile.LastLevelId ?? 0,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ActiveProductDto { Id = playerProfile.Car.Id },
            Background = playerProfile.Background == null ? null : new ActiveProductDto { Id = playerProfile.Background.Id },
            Character = playerProfile.Character == null ? null : new ActiveProductDto { Id = playerProfile.Character.Id }
        };
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login de usuario con email y contraseña",
        Description = "Autentica un usuario existente en el sistema usando email y contraseña. Retorna el perfil del jugador si las credenciales son válidas.",
        OperationId = "LoginPlayer",
        Tags = new[] { "Player - Gestión de jugadores" }
    )]
    [SwaggerResponse(200, "Login exitoso.", typeof(PlayerProfileDto))]
    [SwaggerResponse(400, "Datos de login inválidos.")]
    [SwaggerResponse(401, "Credenciales incorrectas.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    public async Task<ActionResult<PlayerProfileDto>> Login([FromBody] LoginRequestDto request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest("Email y contraseña son requeridos.");

        string? idToken = null;
        if (Request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Bearer "))
                idToken = authHeader.Substring("Bearer ".Length).Trim();
        }

        var playerProfile = await _loginPlayerUseCase.ExecuteAsync(request.Email, request.Password, idToken);
        if (playerProfile == null)
            return Unauthorized("Credenciales inválidas o token inválido.");
        var response = new PlayerProfileDto
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            Email = playerProfile.Email,
            LastLevelId = playerProfile.LastLevelId ?? 0,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ActiveProductDto { Id = playerProfile.Car.Id },
            Background = playerProfile.Background == null ? null : new ActiveProductDto { Id = playerProfile.Background.Id },
            Character = playerProfile.Character == null ? null : new ActiveProductDto { Id = playerProfile.Character.Id }
        };

        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Autenticación con Google Firebase",
        Description = "Permite el login o registro automático de usuarios utilizando Google Firebase Authentication. Si el usuario no existe, se crea automáticamente.",
        OperationId = "GoogleAuth",
        Tags = new[] { "Player - Gestión de jugadores" }
    )]
    [SwaggerResponse(200, "Autenticación exitosa.", typeof(PlayerProfileDto))]
    [SwaggerResponse(400, "Token de Firebase requerido o datos inválidos.")]
    [SwaggerResponse(401, "Token de Firebase inválido.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpPost("google")]
    public async Task<ActionResult<PlayerProfileDto>> Google([FromBody] GoogleRequestDto request)
    {
        string? idToken = null;
        if (Request.Headers.ContainsKey("Authorization"))
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            if (authHeader.StartsWith("Bearer "))
                idToken = authHeader.Substring("Bearer ".Length).Trim();
        }
        if (string.IsNullOrEmpty(idToken))
            return BadRequest("El token de Firebase es requerido en el header Authorization.");
        var playerProfile = await _googleAuthUseCase.ExecuteAsync(idToken, request.Username, request.Email);
        if (playerProfile == null)
            return Unauthorized("Token de Firebase inválido o datos insuficientes.");
        var response = new PlayerProfileDto
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            Email = playerProfile.Email,
            LastLevelId = playerProfile.LastLevelId ?? 0,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ActiveProductDto { Id = playerProfile.Car.Id },
            Background = playerProfile.Background == null ? null : new ActiveProductDto { Id = playerProfile.Background.Id },
            Character = playerProfile.Character == null ? null : new ActiveProductDto { Id = playerProfile.Character.Id }
        };

        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Buscar jugador por email",
        Description = "Obtiene el perfil de un jugador específico utilizando su dirección de email. Útil para funciones de búsqueda de amigos.",
        OperationId = "GetPlayerByEmail",
        Tags = new[] { "Player - Gestión de jugadores" }
    )]
    [SwaggerResponse(200, "Jugador encontrado exitosamente.", typeof(PlayerProfileDto))]
    [SwaggerResponse(404, "Usuario no encontrado con el email especificado.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpGet("email/{email}")]
    public async Task<ActionResult<FriendProfileDto>> GetPlayerByEmail(string email)
    {
        var user = await _getPlayerByEmailUseCase.ExecuteAsync(email);
        if (user == null) return NotFound("Usuario no encontrado.");

        var response = new PlayerProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Points = user.Points,
            Coins = user.Coins,
            Character = user.Character == null ? null : new ActiveProductDto
            {
                Id = user.Character.Id
            },
            Car = user.Car == null ? null : new ActiveProductDto
            {
                Id = user.Car.Id
            },
            Background = user.Background == null ? null : new ActiveProductDto
            {
                Id = user.Background.Id
            }
        };

        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Obtener jugador por ID",
        Description = "Obtiene el perfil de un jugador específico utilizando su UID. Retorna información completa del jugador incluyendo puntos y productos activos.",
        OperationId = "GetPlayerById",
        Tags = new[] { "Player - Gestión de jugadores" }
    )]
    [SwaggerResponse(200, "Jugador encontrado exitosamente.", typeof(PlayerProfileDto))]
    [SwaggerResponse(400, "UID inválido o no proporcionado.")]
    [SwaggerResponse(404, "Jugador no encontrado con el UID especificado.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpGet("{uid}")]
    public async Task<ActionResult<PlayerProfileDto>> GetById(string uid)
    {
        var player = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);

        var response = new PlayerProfileDto
        {
            Id = player.Id,
            Name = player.Name,
            Email = player.Email,
            Points = player.Points,
            Coins = player.Coins,
            Character = player.Character == null ? null : new ActiveProductDto
            {
                Id = player.Character.Id
            },
            Car = player.Car == null ? null : new ActiveProductDto
            {
                Id = player.Car.Id
            },
            Background = player.Background == null ? null : new ActiveProductDto
            {
                Id = player.Background.Id
            }
        };

        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Eliminar cuenta de jugador",
        Description = "Realiza la baja lógica de la cuenta del jugador autenticado. El jugador será marcado como eliminado y no podrá acceder a su cuenta.",
        OperationId = "DeletePlayer",
        Tags = new[] { "Player - Gestión de jugadores" }
    )]
    [SwaggerResponse(200, "Cuenta eliminada exitosamente.")]
    [SwaggerResponse(401, "No autorizado - Token inválido o faltante.")]
    [SwaggerResponse(404, "Jugador no encontrado.")]
    [SwaggerResponse(500, "Error interno del servidor.")]
    [HttpDelete("delete")]
    public async Task<ActionResult> DeletePlayer()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        await _deletePlayerUseCase.ExecuteAsync(uid);

        return Ok(new { message = "Cuenta eliminada exitosamente." });
    }
}
