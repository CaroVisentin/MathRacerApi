using MathRacerAPI.Domain.Services;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Solo;
using MathRacerAPI.Presentation.Mappers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controlador para el modo de juego individual contra la máquina
/// </summary>
[ApiController]
[Route("api/solo")]
public class SoloController : ControllerBase
{
    private readonly StartSoloGameUseCase _startSoloGameUseCase;
    private readonly GetSoloGameStatusUseCase _getSoloGameStatusUseCase;
    private readonly SubmitSoloAnswerUseCase _submitSoloAnswerUseCase;
    private readonly UseWildcardUseCase _useWildcardUseCase;

    public SoloController(
        StartSoloGameUseCase startSoloGameUseCase,
        GetSoloGameStatusUseCase getSoloGameStatusUseCase,
        SubmitSoloAnswerUseCase submitSoloAnswerUseCase,
        UseWildcardUseCase useWildcardUseCase)
    {
        _startSoloGameUseCase = startSoloGameUseCase;
        _getSoloGameStatusUseCase = getSoloGameStatusUseCase;
        _submitSoloAnswerUseCase = submitSoloAnswerUseCase;
        _useWildcardUseCase = useWildcardUseCase;
    }

    /// <summary>
    /// Inicia una nueva partida individual contra la máquina en un nivel específico
    /// </summary>
    /// <param name="levelId">ID del nivel a jugar</param>
    /// <returns>Información inicial de la partida incluyendo la primera pregunta y productos del jugador</returns>
    /// <response code="200">Partida iniciada exitosamente. Retorna el estado inicial del juego.</response>
    /// <response code="400">Solicitud inválida. Sin energía suficiente, productos incompletos o error en la configuración.</response>
    /// <response code="401">No autorizado. Token inválido o faltante.</response>
    /// <response code="404">Nivel o mundo no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/solo/start/1
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint crea una nueva partida individual contra la máquina con las siguientes características:
    /// - Verifica que el jugador tenga energía disponible
    /// - Genera **10 preguntas** basadas en la configuración del nivel y mundo
    /// - Asigna **3 vidas** al jugador
    /// - Carga los **3 productos activos** del jugador (auto, personaje, fondo)
    /// - Selecciona **3 productos aleatorios** para la máquina
    /// - Inicia el **temporizador de la máquina** basado en el tiempo total estimado
    /// 
    /// **Seguridad:**
    /// - Requiere token de Firebase en el header `Authorization`
    /// - El endpoint identifica automáticamente al jugador por su UID de Firebase
    /// - Valida que el jugador tenga energía disponible antes de iniciar
    /// 
    /// **Mecánica del Juego:**
    /// - **Objetivo**: Responder correctamente más preguntas que la máquina antes de perder las 3 vidas
    /// - **Máquina**: Avanza automáticamente basándose en el tiempo transcurrido
    /// - **Jugador**: Avanza una posición por cada respuesta correcta
    /// - **Vidas**: Se pierde una vida por cada respuesta incorrecta o timeout
    /// - **Fin del juego**: Cuando el jugador pierde todas las vidas, termina todas las preguntas, o la máquina llega al final
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "gameId": 123,
    ///       "playerId": 456,
    ///       "playerName": "Juan",
    ///       "levelId": 1,
    ///       "totalQuestions": 10,
    ///       "timePerEquation": 10,
    ///       "livesRemaining": 3,
    ///       "gameStartedAt": "2025-11-01T10:30:00Z",
    ///       "currentQuestion": {
    ///         "id": 7821,
    ///         "equation": "y = 2*x + 3",
    ///         "options": [-5, 0, 3, 8],
    ///         "startedAt": "2025-11-01T10:30:00Z"
    ///       },
    ///       "playerProducts": [
    ///         {
    ///           "productId": 1,
    ///           "name": "Auto Rojo",
    ///           "description": "Auto de color rojo increíble",
    ///           "productTypeId": 1,
    ///           "productTypeName": "Auto",
    ///           "rarityId": 1,
    ///           "rarityName": "Común",
    ///           "rarityColor": "#FFFFFF"
    ///         },
    ///         {
    ///           "productId": 2,
    ///           "name": "Personaje default",
    ///           "description": "Un personaje común",
    ///           "productTypeId": 2,
    ///           "productTypeName": "Personaje",
    ///           "rarityId": 1,
    ///           "rarityName": "Común",
    ///           "rarityColor": "#FFFFFF"
    ///         }
    ///         {
    ///           "productId": 3,
    ///           "name": "Fondo de ciudad",
    ///           "description": "Fondo que representa una ciudad",
    ///           "productTypeId": 3,
    ///           "productTypeName": "Fondo",
    ///           "rarityId": 1,
    ///           "rarityName": "Común",
    ///           "rarityColor": "#FFFFFF"
    ///         }
    ///       ],
    ///       "machineProducts": [
    ///         {
    ///           "productId": 5,
    ///           "name": "Auto Azul",
    ///           "description": "Auto de color azul increíble",
    ///           "productTypeId": 1,
    ///           "productTypeName": "Auto",
    ///           "rarityId": 1,
    ///           "rarityName": "Común",
    ///           "rarityColor": "#FFFFFF"
    ///         },
    ///         {
    ///           "productId": 2,
    ///           "name": "Personaje raro",
    ///           "description": "Un personaje raro",
    ///           "productTypeId": 2,
    ///           "productTypeName": "Personaje",
    ///           "rarityId": 2,
    ///           "rarityName": "Poco común",
    ///           "rarityColor": "#1EFF00"
    ///         }
    ///         {
    ///           "productId": 3,
    ///           "name": "Fondo de carreras",
    ///           "description": "Fondo que representa una carrera",
    ///           "productTypeId": 3,
    ///           "productTypeName": "Fondo",
    ///           "rarityId": 1,
    ///           "rarityName": "Común",
    ///           "rarityColor": "#FFFFFF"
    ///         }
    ///       ]
    ///     }
    ///     
    /// **Posibles errores:**
    /// 
    /// Error 400 (BusinessException - sin energía):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "No tienes energía suficiente para jugar. Espera a que se regenere."
    ///     }
    /// 
    /// Error 400 (BusinessException - productos incompletos):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "Error, debes tener 3 productos activos (auto, personaje, fondo)"
    ///     }
    /// 
    /// Error 400 (BusinessException - productos de máquina):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "Error al cargar productos de la máquina"
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
    /// Error 404 (NotFoundException - nivel):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Nivel con ID 1 no encontrado"
    ///     }
    /// 
    /// Error 404 (NotFoundException - mundo):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Mundo con ID 1 no encontrado"
    ///     }
    /// 
    /// Error 404 (NotFoundException - jugador):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Jugador no encontrado. Por favor, regístrate primero."
    ///     }
    /// 
    /// Error 500 (Error interno):
    /// 
    ///     {
    ///       "statusCode": 500,
    ///       "message": "Ocurrió un error interno en el servidor."
    ///     }
    /// 
    /// </remarks>
    [HttpPost("start/{levelId}")]
    [ProducesResponseType(typeof(StartSoloGameResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StartSoloGameResponseDto>> StartGame(int levelId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var game = await _startSoloGameUseCase.ExecuteAsync(uid, levelId);

        return Ok(game.ToStartGameDto());
    }

    /// <summary>
    /// Obtiene el estado actual de una partida individual en progreso
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <returns>Estado completo de la partida incluyendo progreso, posiciones y la pregunta actual</returns>
    /// <response code="200">Estado obtenido exitosamente. Retorna el estado completo del juego.</response>
    /// <response code="400">Solicitud inválida. Intento de acceder antes del tiempo de revisión permitido.</response>
    /// <response code="401">No autorizado. Token inválido o faltante.</response>
    /// <response code="403">Prohibido. El jugador no tiene permiso para acceder a esta partida.</response>
    /// <response code="404">Partida no encontrada.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     GET /api/solo/123
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint retorna el estado completo de una partida en progreso:
    /// - **Progreso del jugador**: Posición actual, vidas restantes, respuestas correctas
    /// - **Progreso de la máquina**: Posición basada en el tiempo transcurrido
    /// - **Pregunta actual**: Si el juego está en progreso y hay preguntas disponibles
    /// - **Información de tiempo**: Tiempo transcurrido desde el inicio del juego
    /// - **Estado del juego**: InProgress, PlayerWon, MachineWon, PlayerLost
    /// 
    /// **Seguridad:**
    /// - Requiere token de Firebase en el header `Authorization`
    /// - Solo el jugador dueño de la partida puede consultar su estado
    /// - Valida que hayan pasado `ReviewTimeSeconds` (3 segundos) desde la última respuesta
    /// 
    /// **Flujo de Tiempo:**
    /// 1. Jugador responde pregunta → Recibe `WaitTimeSeconds = 3`
    /// 2. Cliente espera 3 segundos mostrando la respuesta correcta
    /// 3. Cliente llama a GET /api/solo/{gameId} para obtener la siguiente pregunta
    /// 4. Si llama antes de tiempo → Error 400 con segundos restantes
    /// 
    /// **Actualización Automática:**
    /// - La **posición de la máquina** se actualiza en cada llamada basándose en el tiempo total transcurrido
    /// - El servidor se encarga de validar las posiciones y el tiempo
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Juego en progreso:**
    /// 
    ///     {
    ///       "gameId": 123,
    ///       "status": "InProgress",
    ///       "playerPosition": 3,
    ///       "machinePosition": 2,
    ///       "livesRemaining": 2,
    ///       "correctAnswers": 3,
    ///       "currentQuestion": {
    ///         "id": 5432,
    ///         "equation": "y = 3*x - 5",
    ///         "options": [-8, -2, 1, 4],
    ///         "startedAt": "2025-11-01T10:31:35Z"
    ///       },
    ///       "currentQuestionIndex": 3,
    ///       "totalQuestions": 10,
    ///       "timePerEquation": 10,
    ///       "gameStartedAt": "2025-11-01T10:30:00Z",
    ///       "gameFinishedAt": null,
    ///       "elapsedTime": 95.5
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Juego terminado:**
    /// 
    ///     {
    ///       "gameId": 123,
    ///       "status": "PlayerWon",
    ///       "playerPosition": 10,
    ///       "machinePosition": 8,
    ///       "livesRemaining": 1,
    ///       "correctAnswers": 10,
    ///       "currentQuestion": null,
    ///       "currentQuestionIndex": 10,
    ///       "totalQuestions": 10,
    ///       "timePerEquation": 10,
    ///       "gameStartedAt": "2025-11-01T10:30:00Z",
    ///       "gameFinishedAt": "2025-11-01T10:32:15Z",
    ///       "elapsedTime": 135.0
    ///     }
    ///     
    /// **Posibles errores:**
    /// 
    /// Error 400 (ValidationException - tiempo de revisión):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "Debes esperar 2 segundos más antes de ver la siguiente pregunta"
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
    /// Error 403 (BusinessException - no autorizado):
    /// 
    ///     {
    ///       "statusCode": 403,
    ///       "message": "No tienes permiso para acceder a esta partida"
    ///     }
    /// 
    /// Error 404 (NotFoundException):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Partida con ID 123 no encontrada"
    ///     }
    /// 
    /// Error 500 (Error interno):
    /// 
    ///     {
    ///       "statusCode": 500,
    ///       "message": "Ocurrió un error interno en el servidor."
    ///     }
    /// 
    /// </remarks>
    [HttpGet("{gameId}")]
    [ProducesResponseType(typeof(SoloGameStatusResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SoloGameStatusResponseDto>> GetGameStatus(int gameId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var result = await _getSoloGameStatusUseCase.ExecuteAsync(gameId, uid);

        return Ok(result.ToStatusDto());
    }

    /// <summary>
    /// Envía la respuesta del jugador a la pregunta actual de la partida
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <param name="answer">Valor numérico de la respuesta seleccionada por el jugador</param>
    /// <returns>Feedback de la respuesta procesada incluyendo si fue correcta, el estado actualizado del juego y las monedas obtenidas si completó el nivel</returns>
    /// <response code="200">Respuesta procesada exitosamente. Retorna feedback, nuevo estado y monedas obtenidas (si aplica).</response>
    /// <response code="400">Solicitud inválida. Partida finalizada, timeout, o no hay preguntas disponibles.</response>
    /// <response code="401">No autorizado. Token inválido o faltante.</response>
    /// <response code="403">Prohibido. El jugador no tiene permiso para responder en esta partida.</response>
    /// <response code="404">Partida no encontrada.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/solo/123/answer
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    ///       Content-Type: application/json
    ///     
    ///     Body:
    ///       5
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint procesa la respuesta del jugador y aplica la lógica del juego:
    /// 
    /// **Si la respuesta es correcta:**
    /// - Incrementa `PlayerPosition` en 1 (o 2 si tiene doble progreso activo)
    /// - Incrementa `CorrectAnswers` en 1
    /// - Verifica si el jugador alcanzó `TotalQuestions` (gana)
    /// - **Si gana**: Otorga recompensa de monedas según el mundo y si es primera vez
    /// 
    /// **Si la respuesta es incorrecta:**
    /// - Decrementa `LivesRemaining` en 1
    /// - Verifica si `LivesRemaining` llegó a 0 (pierde)
    /// - Consume 1 energía si el jugador pierde todas las vidas
    /// 
    /// **Sistema de Recompensas:**
    /// - **Primera vez completando el nivel**: worldId × 100 ± 20% (Ejemplo: Mundo 2 = 160-240 monedas)
    /// - **Repetición del nivel**: worldId × 10 ± 1% (Ejemplo: Mundo 2 = 18-22 monedas)
    /// - El campo `CoinsEarned` indica las monedas obtenidas (0 si no completó el nivel)
    /// 
    /// **Validaciones de Tiempo:**
    /// - **Primera pregunta**: Valida tiempo desde `GameStartedAt`
    /// - **Siguientes preguntas**: Valida tiempo desde `LastAnswerTime` + `ReviewTimeSeconds` (3s)
    /// - Si excede `TimePerEquation`: Cuenta como respuesta incorrecta por timeout
    /// 
    /// **Actualización de Estado:**
    /// - Registra `LastAnswerTime = DateTime.UtcNow` (para validar siguiente llamada a GET)
    /// - Incrementa `CurrentQuestionIndex` para avanzar a la siguiente pregunta
    /// - Actualiza `MachinePosition` basándose en el tiempo total transcurrido
    /// - Verifica condiciones de fin de juego (jugador ganó, máquina ganó, sin vidas)
    /// 
    /// **Seguridad:**
    /// - Requiere token de Firebase en el header `Authorization`
    /// - Solo el jugador dueño de la partida puede enviar respuestas
    /// - El servidor valida timeout para prevenir trampas
    /// 
    /// **Flujo Post-Respuesta:**
    /// 1. Servidor procesa respuesta y retorna `WaitTimeSeconds = 3`
    /// 2. Cliente muestra respuesta correcta durante 3 segundos
    /// 3. Cliente llama a GET /api/solo/{gameId} para obtener la siguiente pregunta
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Respuesta correcta:**
    /// 
    ///     {
    ///       "isCorrect": true,
    ///       "correctAnswer": 5,
    ///       "playerAnswer": 5,
    ///       "status": "InProgress",
    ///       "livesRemaining": 3,
    ///       "playerPosition": 4,
    ///       "machinePosition": 3,
    ///       "correctAnswers": 4,
    ///       "waitTimeSeconds": 3,
    ///       "answeredAt": "2025-11-01T10:31:25Z",
    ///       "currentQuestionIndex": 4,
    ///       "shouldOpenWorldCompletionChest": false,
    ///       "progressIncrement": 1,
    ///       "coinsEarned": 0
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Respuesta incorrecta:**
    /// 
    ///     {
    ///       "isCorrect": false,
    ///       "correctAnswer": 5,
    ///       "playerAnswer": 3,
    ///       "status": "InProgress",
    ///       "livesRemaining": 2,
    ///       "playerPosition": 3,
    ///       "machinePosition": 3,
    ///       "correctAnswers": 3,
    ///       "waitTimeSeconds": 3,
    ///       "answeredAt": "2025-11-01T10:31:25Z",
    ///       "currentQuestionIndex": 4,
    ///       "shouldOpenWorldCompletionChest": false,
    ///       "progressIncrement": 1,
    ///       "coinsEarned": 0
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Jugador ganó (primera vez):**
    /// 
    ///     {
    ///       "isCorrect": true,
    ///       "correctAnswer": 8,
    ///       "playerAnswer": 8,
    ///       "status": "PlayerWon",
    ///       "livesRemaining": 1,
    ///       "playerPosition": 10,
    ///       "machinePosition": 8,
    ///       "correctAnswers": 10,
    ///       "waitTimeSeconds": 3,
    ///       "answeredAt": "2025-11-01T10:32:15Z",
    ///       "currentQuestionIndex": 10,
    ///       "shouldOpenWorldCompletionChest": false,
    ///       "progressIncrement": 1,
    ///       "coinsEarned": 195
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Jugador ganó (repetición):**
    /// 
    ///     {
    ///       "isCorrect": true,
    ///       "correctAnswer": 8,
    ///       "playerAnswer": 8,
    ///       "status": "PlayerWon",
    ///       "livesRemaining": 2,
    ///       "playerPosition": 10,
    ///       "machinePosition": 7,
    ///       "correctAnswers": 10,
    ///       "waitTimeSeconds": 3,
    ///       "answeredAt": "2025-11-01T10:32:15Z",
    ///       "currentQuestionIndex": 10,
    ///       "shouldOpenWorldCompletionChest": false,
    ///       "progressIncrement": 1,
    ///       "coinsEarned": 20
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Completó último nivel del mundo:**
    /// 
    ///     {
    ///       "isCorrect": true,
    ///       "correctAnswer": 8,
    ///       "playerAnswer": 8,
    ///       "status": "PlayerWon",
    ///       "livesRemaining": 1,
    ///       "playerPosition": 10,
    ///       "machinePosition": 8,
    ///       "correctAnswers": 10,
    ///       "waitTimeSeconds": 3,
    ///       "answeredAt": "2025-11-01T10:32:15Z",
    ///       "currentQuestionIndex": 10,
    ///       "shouldOpenWorldCompletionChest": true,
    ///       "progressIncrement": 1,
    ///       "coinsEarned": 195
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Jugador perdió todas las vidas:**
    /// 
    ///     {
    ///       "isCorrect": false,
    ///       "correctAnswer": 2,
    ///       "playerAnswer": 4,
    ///       "status": "PlayerLost",
    ///       "livesRemaining": 0,
    ///       "playerPosition": 5,
    ///       "machinePosition": 7,
    ///       "correctAnswers": 5,
    ///       "waitTimeSeconds": 3,
    ///       "answeredAt": "2025-11-01T10:31:45Z",
    ///       "currentQuestionIndex": 8,
    ///       "shouldOpenWorldCompletionChest": false,
    ///       "progressIncrement": 1,
    ///       "coinsEarned": 0
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Máquina ganó:**
    /// 
    ///     {
    ///       "isCorrect": true,
    ///       "correctAnswer": 1,
    ///       "playerAnswer": 1,
    ///       "status": "MachineWon",
    ///       "livesRemaining": 2,
    ///       "playerPosition": 7,
    ///       "machinePosition": 10,
    ///       "correctAnswers": 7,
    ///       "waitTimeSeconds": 3,
    ///       "answeredAt": "2025-11-01T10:33:00Z",
    ///       "currentQuestionIndex": 7,
    ///       "shouldOpenWorldCompletionChest": false,
    ///       "progressIncrement": 1,
    ///       "coinsEarned": 0
    ///     }
    ///     
    /// **Posibles errores:**
    /// 
    /// Error 400 (BusinessException - partida finalizada):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "La partida ya finalizó"
    ///     }
    /// 
    /// Error 400 (BusinessException - no hay preguntas):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "No hay más preguntas disponibles"
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
    /// Error 403 (BusinessException - no autorizado):
    /// 
    ///     {
    ///       "statusCode": 403,
    ///       "message": "No tienes permiso para enviar respuestas a esta partida"
    ///     }
    /// 
    /// Error 404 (NotFoundException):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Partida con ID 123 no encontrada"
    ///     }
    /// 
    /// Error 500 (Error interno):
    /// 
    ///     {
    ///       "statusCode": 500,
    ///       "message": "Ocurrió un error interno en el servidor."
    ///     }
    /// 
    /// </remarks>
    [HttpPost("{gameId}/answer")]
    [ProducesResponseType(typeof(SubmitSoloAnswerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SubmitSoloAnswerResponseDto>> SubmitAnswer(
        int gameId, 
        [FromBody] int answer)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var result = await _submitSoloAnswerUseCase.ExecuteAsync(gameId, answer, uid);

        return Ok(result.ToAnswerDto());
    }

    /// <summary>
    /// Activa un wildcard en la partida individual actual
    /// </summary>
    /// <param name="gameId">ID de la partida</param>
    /// <param name="wildcardId">ID del wildcard a usar (1: Eliminar opción, 2: Saltar pregunta, 3: Doble progreso)</param>
    /// <returns>Resultado de activar el wildcard incluyendo efectos aplicados</returns>
    /// <response code="200">Wildcard activado exitosamente. Retorna el resultado con los efectos aplicados.</response>
    /// <response code="400">Solicitud inválida. Wildcard ya usado en esta partida, juego finalizado, wildcard no disponible o cantidad insuficiente.</response>
    /// <response code="401">No autorizado. Token inválido o faltante.</response>
    /// <response code="403">Prohibido. El jugador no tiene permiso para usar wildcards en esta partida.</response>
    /// <response code="404">Partida o wildcard no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/solo/123/wildcard/1
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint activa un wildcard (comodín) en una partida individual en progreso. Cada wildcard tiene un efecto único:
    /// 
    /// **Wildcards Disponibles:**
    /// 
    /// 1. **Eliminar Opción Incorrecta (ID: 1)**
    ///    - Elimina una opcion incorrecta de la pregunta actual
    ///    - Facilita acertar la respuesta correcta
    ///    - Solo puede usarse una vez por partida
    /// 
    /// 2. **Saltar Pregunta (ID: 2)**
    ///    - Salta a la siguiente pregunta sin penalización
    ///    - No consume vidas ni afecta el progreso
    ///    - Útil para preguntas muy difíciles
    /// 
    /// 3. **Doble Progreso (ID: 3)**
    ///    - La siguiente respuesta correcta avanza 2 posiciones en lugar de 1
    ///    - Se activa como un efecto temporal
    ///    - Se consume al responder correctamente la siguiente pregunta
    /// 
    /// **Restricciones:**
    /// - Solo se puede usar **un wildcard de cada tipo por partida**
    /// - El jugador debe tener al menos **1 unidad** del wildcard en su inventario
    /// - El juego debe estar en estado **InProgress**
    /// - Solo el jugador dueño de la partida puede usar wildcards
    /// 
    /// **Seguridad:**
    /// - Requiere token de Firebase en el header `Authorization`
    /// - Valida que el jugador sea dueño de la partida
    /// - Verifica disponibilidad y cantidad del wildcard
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Eliminar Opción (ID 1):**
    /// 
    ///     {
    ///       "wildcardId": 1,
    ///       "success": true,
    ///       "message": "Se eliminó una opción incorrecta",
    ///       "remainingQuantity": 2,
    ///       "modifiedOptions": [5, 8],
    ///       "newQuestionIndex": null,
    ///       "newQuestion": null,
    ///       "doubleProgressActive": false
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Saltar Pregunta (ID 2):**
    /// 
    ///     {
    ///       "wildcardId": 2,
    ///       "success": true,
    ///       "message": "Pregunta cambiada exitosamente",
    ///       "remainingQuantity": 1,
    ///       "modifiedOptions": null,
    ///       "newQuestionIndex": 5,
    ///       "newQuestion": {
    ///         "id": 9821,
    ///         "equation": "y = 4*x + 1",
    ///         "options": [-3, 1, 5, 9],
    ///         "startedAt": "2025-11-01T10:32:10Z"
    ///       },
    ///       "doubleProgressActive": false
    ///     }
    /// 
    /// **Ejemplo de respuesta exitosa (200) - Doble Progreso (ID 3):**
    /// 
    ///     {
    ///       "wildcardId": 3,
    ///       "success": true,
    ///       "message": "Doble progreso activado. La siguiente respuesta correcta valdrá doble.",
    ///       "remainingQuantity": 0,
    ///       "modifiedOptions": null,
    ///       "newQuestionIndex": null,
    ///       "newQuestion": null,
    ///       "doubleProgressActive": true
    ///     }
    ///     
    /// **Posibles errores:**
    /// 
    /// Error 400 (BusinessException - wildcard ya usado):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "Ya usaste este tipo de comodín en esta partida"
    ///     }
    /// 
    /// Error 400 (BusinessException - juego finalizado):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "No puedes usar comodines en un juego finalizado"
    ///     }
    /// 
    /// Error 400 (BusinessException - wildcard no disponible):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "No tienes este comodín disponible"
    ///     }
    /// 
    /// Error 400 (BusinessException - cantidad insuficiente):
    /// 
    ///     {
    ///       "statusCode": 400,
    ///       "message": "No tienes suficientes unidades de este comodín"
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
    /// Error 403 (BusinessException - no autorizado):
    /// 
    ///     {
    ///       "statusCode": 403,
    ///       "message": "No tienes permiso para usar wildcards en esta partida"
    ///     }
    /// 
    /// Error 404 (NotFoundException - partida):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Partida con ID 123 no encontrada"
    ///     }
    /// 
    /// Error 404 (NotFoundException - wildcard):
    /// 
    ///     {
    ///       "statusCode": 404,
    ///       "message": "Comodín con ID 1 no encontrado"
    ///     }
    /// 
    /// Error 500 (Error interno):
    /// 
    ///     {
    ///       "statusCode": 500,
    ///       "message": "Ocurrió un error interno en el servidor."
    ///     }
    /// 
    /// </remarks>
    [HttpPost("{gameId}/wildcard/{wildcardId}")]
    [ProducesResponseType(typeof(UseWildcardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UseWildcardResponseDto>> UseWildcard(int gameId, int wildcardId)
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;
        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var result = await _useWildcardUseCase.ExecuteAsync(gameId, wildcardId, uid);

        return Ok(result.ToWildcardResponseDto());
    }
}
