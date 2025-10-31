using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Infrastructure.Services;
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
    private readonly RegisterPlayerUseCase _registerPlayerUseCase;
    private readonly LoginPlayerUseCase _loginPlayerUseCase;
    private readonly GoogleAuthUseCase _googleAuthUseCase;

    public PlayerController(
        RegisterPlayerUseCase registerPlayerUseCase,
        LoginPlayerUseCase loginPlayerUseCase,
        GoogleAuthUseCase googleAuthUseCase,
        GetPlayerByIdUseCase getPlayerByIdUseCase)
    {
        _registerPlayerUseCase = registerPlayerUseCase;
        _loginPlayerUseCase = loginPlayerUseCase;
        _googleAuthUseCase = googleAuthUseCase;

    }

    /// <summary>
    /// Registro de usuario con email y contraseña
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(PlayerProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            LastLevelId = playerProfile.LastLevelId,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins
        };
        return StatusCode(StatusCodes.Status201Created, response);
    }

    /// <summary>
    /// Login de usuario con email y contraseña
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(PlayerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            LastLevelId = playerProfile.LastLevelId,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins
        };
        return Ok(response);
    }

    /// <summary>
    /// Login/registro con Google (Firebase)
    /// </summary>
    [HttpPost("google")]
    [ProducesResponseType(typeof(PlayerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
            LastLevelId = playerProfile.LastLevelId,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins
        };
        return Ok(response);
    }
}