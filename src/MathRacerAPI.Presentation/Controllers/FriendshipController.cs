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

        /// <summary>
        /// Retrieves all friends for a given player.
        /// </summary>
        /// <param name="playerId">The ID of the player whose friends are being requested.</param>
        /// <returns>A list of friends as FriendProfileDto.</returns>
        [HttpGet("{playerId}/friends")]
        [ProducesResponseType(typeof(IEnumerable<FriendProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FriendProfileDto>>> GetFriends(int playerId)
        {

            try
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while fetching friends.");

            }
        }

        /// <summary>
        /// Sends a friend request from one player to another.
        /// </summary>
        /// <param name="request">Contains FromPlayerId and ToPlayerId.</param>
        [HttpPost("request")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> SendFriendRequest([FromBody] FriendRequestDto request)
        {
            if (request.FromPlayerId == request.ToPlayerId)
                return BadRequest("You cannot send a friend request to yourself");

            try
            {
                var fromPlayerId = await GetAuthenticatedPlayerId();
                await _sendFriendRequestUseCase.ExecuteAsync(fromPlayerId, request.ToPlayerId);
                return Created(string.Empty, null);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error ocurred while sending the friend request.");
            }
        }


        /// <summary>
        /// Accepts a pending friend request.
        /// </summary>
        /// <param name="request">Contains FromPlayerId and ToPlayerId.</param>
        [HttpPost("accept")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> AcceptFriendRequest([FromBody] FriendRequestDto request)
        {
            try
            {
                var toPlayerId = await GetAuthenticatedPlayerId();
                await _acceptFriendRequestUseCase.ExecuteAsync(request.FromPlayerId, toPlayerId);
                return Ok("Friend request accepted.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while accepting the friend request.");
            }
        }


        /// <summary>
        /// Rejects a pending friend request.
        /// </summary>
        /// <param name="request">Contains FromPlayerId and ToPlayerId.</param>
        [HttpPost("reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RejectFriendRequest([FromBody] FriendRequestDto request)
        {
            try
            {
                var toPlayerId = await GetAuthenticatedPlayerId();

                await _rejectFriendRequestUseCase.ExecuteAsync(request.FromPlayerId, toPlayerId);
                return Ok("Friend request rejected.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while rejecting the friend request.");
            }
        }


        /// <summary>
        /// Deletes an accepted friendship between two players.
        /// </summary>
        /// <param name="request">Contains FromPlayerId and ToPlayerId.</param>
        [HttpPost("delete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteFriend([FromBody] FriendRequestDto request)
        {
            try
            {
                var authenticatedPlayerId = await GetAuthenticatedPlayerId();
                int otherPlayerId = request.FromPlayerId == authenticatedPlayerId ? request.ToPlayerId : request.FromPlayerId;

                await _deleteFriendUseCase.ExecuteAsync(authenticatedPlayerId, otherPlayerId);
                return Ok("Friend deleted.");
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the friend.");
            }
        }

        /// <summary>
        /// Retrieves all pending friend requests for a given player.
        /// </summary>
        /// <param name="playerId">The ID of the player whose pending friend requests are being requested.</param>
        /// <returns>A list of pending friend requests as FriendProfileDto.</returns>
        [HttpGet("{playerId}/pending")]
        [ProducesResponseType(typeof(IEnumerable<FriendProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<FriendProfileDto>>> GetPendingFriendRequests(int playerId)
        {
            try
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
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "An error occurred while fetching pending friend requests.");
            }
        }

        private async Task<int> GetAuthenticatedPlayerId()
        {
            if (!HttpContext.Items.TryGetValue("FirebaseUid", out var uidObj) || uidObj == null)
                throw new UnauthorizedAccessException("Usuario no autenticado");

            var uid = uidObj.ToString();
            var player = await _getPlayerByIdUseCase.ExecuteByUidAsync(uid);
            Console.WriteLine($"Auth UID: {uid}, PlayerId: {player.Id}");

            return player.Id;
        }


    }
}
