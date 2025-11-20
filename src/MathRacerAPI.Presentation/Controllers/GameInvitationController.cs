using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class GameInvitationController : ControllerBase
    {
        private readonly SendGameInvitationUseCase _sendInvitationUseCase;
        private readonly GetGameInvitationsUseCase _getInvitationsUseCase;
        private readonly RespondGameInvitationUseCase _respondInvitationUseCase;

        public GameInvitationController(
            SendGameInvitationUseCase sendInvitationUseCase,
            GetGameInvitationsUseCase getInvitationsUseCase,
            RespondGameInvitationUseCase respondInvitationUseCase)
        {
            _sendInvitationUseCase = sendInvitationUseCase;
            _getInvitationsUseCase = getInvitationsUseCase;
            _respondInvitationUseCase = respondInvitationUseCase;
        }

        [SwaggerOperation(
            Summary = "Envía una invitación de partida a un amigo",
            Description = "Crea una partida privada y envía una invitación al amigo especificado. Requiere autenticación con Firebase.",
            OperationId = "SendGameInvitation",
            Tags = new[] { "Online - Invitaciones" }
        )]
        [SwaggerResponse(201, "Invitación enviada exitosamente.")]
        [SwaggerResponse(400, "Solicitud inválida.")]
        [SwaggerResponse(401, "No autorizado.")]
        [SwaggerResponse(404, "Amigo no encontrado.")]
        [HttpPost("send")]
        public async Task<IActionResult> SendInvitation([FromBody] SendGameInvitationRequestDto request)
        {
            var firebaseUid = HttpContext.Items["FirebaseUid"] as string;
            if (string.IsNullOrEmpty(firebaseUid))
                return Unauthorized(new { error = "Token de autenticación requerido o inválido." });

            var invitation = await _sendInvitationUseCase.ExecuteAsync(
                firebaseUid,
                request.InvitedFriendId,
                request.Difficulty,
                request.ExpectedResult
            );

            return CreatedAtAction(
                nameof(GetInvitations),
                new
                {
                    InvitationId = invitation.Id,
                    GameId = invitation.GameId,
                    InvitedPlayerName = invitation.InvitedPlayerName,
                    GameName = invitation.GameName,
                    Difficulty = request.Difficulty,
                    ExpectedResult = request.ExpectedResult,
                    Message = "Invitación enviada exitosamente. El amigo recibirá la notificación."
                });
        }

        [SwaggerOperation(
            Summary = "Obtiene las invitaciones pendientes del jugador (buzón)",
            Description = "Retorna todas las invitaciones de partida pendientes que el jugador ha recibido.",
            OperationId = "GetGameInvitations",
            Tags = new[] { "Online - Invitaciones" }
        )]
        [SwaggerResponse(200, "Lista de invitaciones obtenida exitosamente.")]
        [SwaggerResponse(401, "No autorizado.")]
        [HttpGet("inbox")]
        public async Task<IActionResult> GetInvitations()
        {
            var firebaseUid = HttpContext.Items["FirebaseUid"] as string;
            if (string.IsNullOrEmpty(firebaseUid))
                return Unauthorized(new { error = "Token de autenticación requerido o inválido." });

            var invitations = await _getInvitationsUseCase.ExecuteAsync(firebaseUid);

            var invitationDtos = invitations.Select(i => new GameInvitationDto
            {
                Id = i.Id,
                GameId = i.GameId,
                InviterPlayerName = i.InviterPlayerName,
                GameName = i.GameName,
                Difficulty = i.Difficulty,
                ExpectedResult = i.ExpectedResult,
                CreatedAt = i.CreatedAt,
                Status = i.Status.ToString()
            }).ToList();

            return Ok(new
            {
                TotalInvitations = invitationDtos.Count,
                Invitations = invitationDtos
            });
        }

        [SwaggerOperation(
            Summary = "Responde a una invitación de partida",
            Description = "Permite aceptar o rechazar una invitación de partida. Si se acepta, el jugador podrá unirse a la partida.",
            OperationId = "RespondGameInvitation",
            Tags = new[] { "Online - Invitaciones" }
        )]
        [SwaggerResponse(200, "Invitación respondida exitosamente.")]
        [SwaggerResponse(400, "Solicitud inválida.")]
        [SwaggerResponse(401, "No autorizado.")]
        [SwaggerResponse(404, "Invitación no encontrada.")]
        [HttpPost("respond")]
        public async Task<IActionResult> RespondInvitation([FromBody] RespondGameInvitationRequestDto request)
        {
            var firebaseUid = HttpContext.Items["FirebaseUid"] as string;
            if (string.IsNullOrEmpty(firebaseUid))
                return Unauthorized(new { error = "Token de autenticación requerido o inválido." });

            var (accepted, gameId) = await _respondInvitationUseCase.ExecuteAsync(
                firebaseUid,
                request.InvitationId,
                request.Accept
            );

            if (accepted)
            {
                return Ok(new
                {
                    Accepted = true,
                    GameId = gameId,
                    Message = "Invitación aceptada. Conéctate a SignalR y llama JoinGame para unirte a la partida."
                });
            }
            else
            {
                return Ok(new
                {
                    Accepted = false,
                    Message = "Invitación rechazada exitosamente."
                });
            }
        }
    }
}