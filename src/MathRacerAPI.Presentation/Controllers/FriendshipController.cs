using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class FriendshipController : ControllerBase
    {
        private readonly SendFriendRequestUseCase _sendFriendRequestUseCase;
        private readonly AcceptFriendRequestUseCase _acceptFriendRequestUseCase;
        private readonly RejectFriendRequestUseCase _rejectFriendRequestUseCase;
        private readonly GetFriendsUseCase _getFriendsUseCase;
        private readonly DeleteFriendUseCase _deleteFriendUseCase;
        private readonly GetPendingFriendRequestsUseCase _getPendingFriendRequestsUseCase;
        private readonly GetPlayerByIdUseCase _getPlayerByIdUseCase;


        public FriendshipController(
            SendFriendRequestUseCase sendFriendRequestUseCase,
            AcceptFriendRequestUseCase acceptFriendRequestUseCase,
            RejectFriendRequestUseCase rejectFriendRequestUseCase,
            GetFriendsUseCase getFriendsUseCase,
            DeleteFriendUseCase deleteFriendUseCase,
            GetPendingFriendRequestsUseCase getPendingFriendRequestsUseCase,
            GetPlayerByIdUseCase getPlayerByIdUseCase)
        {
            _sendFriendRequestUseCase = sendFriendRequestUseCase;
            _acceptFriendRequestUseCase = acceptFriendRequestUseCase;
            _rejectFriendRequestUseCase = rejectFriendRequestUseCase;
            _getFriendsUseCase = getFriendsUseCase;
            _deleteFriendUseCase = deleteFriendUseCase;
            _getPendingFriendRequestsUseCase = getPendingFriendRequestsUseCase;
            _getPlayerByIdUseCase = getPlayerByIdUseCase;
        }

        [SwaggerOperation(
            Summary = "Obtiene la lista de amigos de un jugador",
            Description = "Retorna todos los amigos confirmados de un jugador específico con su información de perfil y estado.",
            OperationId = "GetPlayerFriends",
            Tags = new[] { "Friendship - Sistema de amistades" }
        )]
        [SwaggerResponse(200, "Lista de amigos obtenida exitosamente.", typeof(IEnumerable<FriendProfileDto>))]
        [SwaggerResponse(404, "Jugador no encontrado.")]
        [SwaggerResponse(500, "Error interno del servidor.")]
        [HttpGet("{playerId}/friends")]
        public async Task<ActionResult<IEnumerable<FriendProfileDto>>> GetFriends(int playerId)
        {
            var authenticatedPlayerId = await GetAuthenticatedPlayerId();

            if (playerId != authenticatedPlayerId)
                return Unauthorized("You cannot fetch friends of another user.");

            var friends = await _getFriendsUseCase.ExecuteAsync(authenticatedPlayerId);

            var response = friends.Select(p => new FriendProfileDto
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email,
                Uid = p.Uid,
                Points = p.Points,
                Character = p.Character == null ? null : new ActiveProductDto { Id = p.Character.Id }
            });

            return Ok(response);
        }

        [SwaggerOperation(
            Summary = "Envía una solicitud de amistad",
            Description = "Permite a un jugador enviar una solicitud de amistad a otro jugador. El sistema valida que no sea una auto-solicitud.",
            OperationId = "SendFriendRequest",
            Tags = new[] { "Friendship - Sistema de amistades" }
        )]
        [SwaggerResponse(201, "Solicitud de amistad enviada exitosamente.")]
        [SwaggerResponse(400, "Solicitud inv�lida o jugadores iguales.")]
        [SwaggerResponse(500, "Error interno del servidor.")]
        [HttpPost("request")]
        public async Task<ActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            if (request.FromPlayerId == request.ToPlayerId)
                return BadRequest("You cannot send a friend request to yourself");

            var fromPlayerId = await GetAuthenticatedPlayerId();
            await _sendFriendRequestUseCase.ExecuteAsync(fromPlayerId, request.ToPlayerId);
            return Created(string.Empty, null);
        }


        [SwaggerOperation(
            Summary = "Acepta una solicitud de amistad pendiente",
            Description = "Permite al jugador receptor aceptar una solicitud de amistad previamente enviada, estableciendo una relaci�n de amistad bidireccional.",
            OperationId = "AcceptFriendRequest",
            Tags = new[] { "Friendship - Sistema de amistades" }
        )]
        [SwaggerResponse(200, "Solicitud de amistad aceptada exitosamente.")]
        [SwaggerResponse(400, "Solicitud inv�lida o no existe.")]
        [SwaggerResponse(500, "Error interno del servidor.")]
        [HttpPost("accept")]
        public async Task<ActionResult> AcceptFriendRequest([FromBody] FriendRequestDto request)
        {
            var toPlayerId = await GetAuthenticatedPlayerId();
            await _acceptFriendRequestUseCase.ExecuteAsync(request.FromPlayerId, toPlayerId);
            return Ok("Friend request accepted.");
        }


        [SwaggerOperation(
            Summary = "Rechaza una solicitud de amistad pendiente",
            Description = "Permite al jugador receptor rechazar una solicitud de amistad, eliminando la solicitud del sistema.",
            OperationId = "RejectFriendRequest",
            Tags = new[] { "Friendship - Sistema de amistades" }
        )]
        [SwaggerResponse(200, "Solicitud de amistad rechazada exitosamente.")]
        [SwaggerResponse(400, "Solicitud inv�lida o no existe.")]
        [SwaggerResponse(500, "Error interno del servidor.")]
        [HttpPost("reject")]
        public async Task<ActionResult> RejectFriendRequest([FromBody] FriendRequestDto request)
        {
            var toPlayerId = await GetAuthenticatedPlayerId();

            await _rejectFriendRequestUseCase.ExecuteAsync(request.FromPlayerId, toPlayerId);
            return Ok("Friend request rejected.");
        }


        [SwaggerOperation(
            Summary = "Elimina una amistad aceptada",
            Description = "Elimina la amistad establecida entre dos jugadores",
            OperationId = "DeleteFriend",
            Tags = new[] { "Friendship - Sistema de amistades" })]
        [SwaggerResponse(200, "Amistad eliminada correctamente")]
        [SwaggerResponse(400, "Solicitud inv�lida")]
        [SwaggerResponse(401, "No autorizado")]
        [SwaggerResponse(500, "Error interno del servidor")]
        [HttpDelete("delete")]
        public async Task<ActionResult> DeleteFriend([FromBody] FriendRequestDto request)
        {
            var authenticatedPlayerId = await GetAuthenticatedPlayerId();
            int otherPlayerId = request.FromPlayerId == authenticatedPlayerId ? request.ToPlayerId : request.FromPlayerId;

            await _deleteFriendUseCase.ExecuteAsync(authenticatedPlayerId, otherPlayerId);
            return Ok("Friend deleted.");
        }

        [SwaggerOperation(
            Summary = "Obtiene solicitudes de amistad pendientes",
            Description = "Obtiene todas las solicitudes de amistad pendientes para un jugador espec�fico",
            OperationId = "GetPendingFriendRequests", 
            Tags = new[] { "Friendship - Sistema de amistades" })]
        [SwaggerResponse(200, "Solicitudes de amistad pendientes obtenidas correctamente", typeof(IEnumerable<FriendProfileDto>))]
        [SwaggerResponse(401, "No autorizado")]
        [SwaggerResponse(500, "Error interno del servidor")]
        [HttpGet("{playerId}/pending")]
        public async Task<ActionResult<IEnumerable<FriendProfileDto>>> GetPendingFriendRequests(int playerId)
        {
            var authenticatedPlayerId = await GetAuthenticatedPlayerId();

            if (playerId != authenticatedPlayerId)
                return Unauthorized("You cannot fetch pending friend requests of another user.");

            var pendingFriends = await _getPendingFriendRequestsUseCase.ExecuteAsync(authenticatedPlayerId);

            var response = pendingFriends.Select(p => new FriendProfileDto
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email,
                Uid = p.Uid,
                Points = p.Points,
                Character = p.Character == null ? null : new ActiveProductDto { Id = p.Character.Id }
            });

            return Ok(response);
        }

        private async Task<int> GetAuthenticatedPlayerId()
        {
            if (!HttpContext.Items.TryGetValue("FirebaseUid", out var uidObj) || uidObj == null)
                throw new UnauthorizedAccessException("Usuario no autenticado");

            var uid = uidObj.ToString();
            if (string.IsNullOrEmpty(uid))
                throw new UnauthorizedAccessException("UID de usuario inválido");
                
            var player = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);
            Console.WriteLine($"Auth UID: {uid}, PlayerId: {player.Id}");

            return player.Id;
        }


    }
}
