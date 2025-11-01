using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace MathRacerAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Friendship")]
    public class FriendshipController : ControllerBase
    {
        private readonly SendFriendRequestUseCase _sendFriendRequestUseCase;
        private readonly AcceptFriendRequestUseCase _acceptFriendRequestUseCase;
        private readonly RejectFriendRequestUseCase _rejectFriendRequestUseCase;
        private readonly GetFriendsUseCase _getFriendsUseCase;

        public FriendshipController(
            SendFriendRequestUseCase sendFriendRequestUseCase,
            AcceptFriendRequestUseCase acceptFriendRequestUseCase,
            RejectFriendRequestUseCase rejectFriendRequestUseCase,
            GetFriendsUseCase getFriendsUseCase)
        {
            _sendFriendRequestUseCase = sendFriendRequestUseCase;
            _acceptFriendRequestUseCase = acceptFriendRequestUseCase;
            _rejectFriendRequestUseCase = rejectFriendRequestUseCase;
            _getFriendsUseCase = getFriendsUseCase;
        }

        [HttpGet("{playerId}/friends")]
        [ProducesResponseType(typeof(IEnumerable<FriendProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FriendProfileDto>>> GetFriends(int playerId)
        {
            var friends = await _getFriendsUseCase.ExecuteAsync(playerId);

            // Mapear a DTOs para exponer solo los campos requeridos
            var response = friends.Select(p => new FriendProfileDto
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email,
                Uid = p.Uid,
                Points = p.Points,
                Character = p.Character == null ? null : new ProductDto
                {
                    Id = p.Character.Id,
                    Name = p.Character.Name,
                    Description = p.Character.Description,
                    Price = p.Character.Price,
                    ProductType = p.Character.ProductType
                }
            });

            return Ok(response);
        }

        [HttpPost("request")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            if (request.FromPlayerId == request.ToPlayerId)
                return BadRequest("No te podes enviar solicitud a vos mismo.");

            await _sendFriendRequestUseCase.ExecuteAsync(request.FromPlayerId, request.ToPlayerId);
            return Created(string.Empty, null);
        }

        [HttpPost("accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AcceptFriendRequest([FromBody] FriendRequestDto request)
        {
            await _acceptFriendRequestUseCase.ExecuteAsync(request.FromPlayerId, request.ToPlayerId);
            return Ok();
        }

        [HttpPost("reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RejectFriendRequest([FromBody] FriendRequestDto request)
        {
            await _rejectFriendRequestUseCase.ExecuteAsync(request.FromPlayerId, request.ToPlayerId);
            return Ok();
        }
    }
}
