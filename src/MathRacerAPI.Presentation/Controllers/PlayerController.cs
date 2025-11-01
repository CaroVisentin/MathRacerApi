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
    private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;

    public PlayerController(
        RegisterPlayerUseCase registerPlayerUseCase,
        LoginPlayerUseCase loginPlayerUseCase,
        GoogleAuthUseCase googleAuthUseCase,
        GetPlayerByIdUseCase getPlayerByIdUseCase)
    {
        _registerPlayerUseCase = registerPlayerUseCase;
        _loginPlayerUseCase = loginPlayerUseCase;
        _googleAuthUseCase = googleAuthUseCase;
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
        var playerProfile = await _registerPlayerUseCase.ExecuteAsync(
            request.Username,
            request.Email,
            request.Uid
        );
        if (playerProfile == null)
            return BadRequest("No se pudo registrar el jugador. El email puede estar duplicado o los datos son inválidos.");

        var response = new PlayerProfileDto
        {
            Id = playerProfile.Id,
            Name = playerProfile.Name,
            Email = playerProfile.Email,
            LastLevelId = playerProfile.LastLevelId,
            Points = playerProfile.Points,
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ProductDto
            {
                Id = playerProfile.Car.Id,
                Name = playerProfile.Car.Name,
                Description = playerProfile.Car.Description,
                Price = playerProfile.Car.Price,
                ProductType = playerProfile.Car.ProductType
            },

            Background = playerProfile.Background == null ? null : new ProductDto
            {
                Id = playerProfile.Background.Id,
                Name = playerProfile.Background.Name,
                Description = playerProfile.Background.Description,
                Price = playerProfile.Background.Price,
                ProductType = playerProfile.Background.ProductType
            },

            Character = playerProfile.Character == null ? null : new ProductDto
            {
                Id = playerProfile.Character.Id,
                Name = playerProfile.Character.Name,
                Description = playerProfile.Character.Description,
                Price = playerProfile.Character.Price,
                ProductType = playerProfile.Character.ProductType
            }
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
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ProductDto
            {
                Id = playerProfile.Car.Id,
                Name = playerProfile.Car.Name,
                Description = playerProfile.Car.Description,
                Price = playerProfile.Car.Price,
                ProductType = playerProfile.Car.ProductType
            },

            Background = playerProfile.Background == null ? null : new ProductDto
            {
                Id = playerProfile.Background.Id,
                Name = playerProfile.Background.Name,
                Description = playerProfile.Background.Description,
                Price = playerProfile.Background.Price,
                ProductType = playerProfile.Background.ProductType
            },

            Character = playerProfile.Character == null ? null : new ProductDto
            {
                Id = playerProfile.Character.Id,
                Name = playerProfile.Character.Name,
                Description = playerProfile.Character.Description,
                Price = playerProfile.Character.Price,
                ProductType = playerProfile.Character.ProductType
            }
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
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ProductDto
            {
                Id = playerProfile.Car.Id,
                Name = playerProfile.Car.Name,
                Description = playerProfile.Car.Description,
                Price = playerProfile.Car.Price,
                ProductType = playerProfile.Car.ProductType
            },

            Background = playerProfile.Background == null ? null : new ProductDto
            {
                Id = playerProfile.Background.Id,
                Name = playerProfile.Background.Name,
                Description = playerProfile.Background.Description,
                Price = playerProfile.Background.Price,
                ProductType = playerProfile.Background.ProductType
            },

            Character = playerProfile.Character == null ? null : new ProductDto
            {
                Id = playerProfile.Character.Id,
                Name = playerProfile.Character.Name,
                Description = playerProfile.Character.Description,
                Price = playerProfile.Character.Price,
                ProductType = playerProfile.Character.ProductType
            }
        };


        return Ok(response);
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
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ProductDto
            {
                Id = playerProfile.Car.Id,
                Name = playerProfile.Car.Name,
                Description = playerProfile.Car.Description,
                Price = playerProfile.Car.Price,
                ProductType = playerProfile.Car.ProductType
            },

            Background = playerProfile.Background == null ? null : new ProductDto
            {
                Id = playerProfile.Background.Id,
                Name = playerProfile.Background.Name,
                Description = playerProfile.Background.Description,
                Price = playerProfile.Background.Price,
                ProductType = playerProfile.Background.ProductType
            },

            Character = playerProfile.Character == null ? null : new ProductDto
            {
                Id = playerProfile.Character.Id,
                Name = playerProfile.Character.Name,
                Description = playerProfile.Character.Description,
                Price = playerProfile.Character.Price,
                ProductType = playerProfile.Character.ProductType
            }
        };

        return CreatedAtAction(nameof(GetPlayerById), new { id = playerProfile.Id }, response);
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
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ProductDto
            {
                Id = playerProfile.Car.Id,
                Name = playerProfile.Car.Name,
                Description = playerProfile.Car.Description,
                Price = playerProfile.Car.Price,
                ProductType = playerProfile.Car.ProductType
            },

            Background = playerProfile.Background == null ? null : new ProductDto
            {
                Id = playerProfile.Background.Id,
                Name = playerProfile.Background.Name,
                Description = playerProfile.Background.Description,
                Price = playerProfile.Background.Price,
                ProductType = playerProfile.Background.ProductType
            },

            Character = playerProfile.Character == null ? null : new ProductDto
            {
                Id = playerProfile.Character.Id,
                Name = playerProfile.Character.Name,
                Description = playerProfile.Character.Description,
                Price = playerProfile.Character.Price,
                ProductType = playerProfile.Character.ProductType
            }
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
            Coins = playerProfile.Coins,

            Car = playerProfile.Car == null ? null : new ProductDto
            {
                Id = playerProfile.Car.Id,
                Name = playerProfile.Car.Name,
                Description = playerProfile.Car.Description,
                Price = playerProfile.Car.Price,
                ProductType = playerProfile.Car.ProductType
            },

            Background = playerProfile.Background == null ? null : new ProductDto
            {
                Id = playerProfile.Background.Id,
                Name = playerProfile.Background.Name,
                Description = playerProfile.Background.Description,
                Price = playerProfile.Background.Price,
                ProductType = playerProfile.Background.ProductType
            },

            Character = playerProfile.Character == null ? null : new ProductDto
            {
                Id = playerProfile.Character.Id,
                Name = playerProfile.Character.Name,
                Description = playerProfile.Character.Description,
                Price = playerProfile.Character.Price,
                ProductType = playerProfile.Character.ProductType
            }
        };

        return Ok(response);
    }
}