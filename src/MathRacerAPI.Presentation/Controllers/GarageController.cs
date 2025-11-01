using MathRacerAPI.Domain.Models;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GarageController : ControllerBase
    {
        private readonly GetPlayerGarageItemsUseCase _getPlayerGarageItemsUseCase;
        private readonly ActivatePlayerItemUseCase _activatePlayerItemUseCase;

        public GarageController(
            GetPlayerGarageItemsUseCase getPlayerGarageItemsUseCase,
            ActivatePlayerItemUseCase activatePlayerItemUseCase)
        {
            _getPlayerGarageItemsUseCase = getPlayerGarageItemsUseCase;
            _activatePlayerItemUseCase = activatePlayerItemUseCase;
        }



        /// <summary>
        /// Get player's cars
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>List of cars with ownership and active status</returns>
        [HttpGet("cars/{playerId}")]
        public async Task<ActionResult<GarageItemsResponseDto>> GetPlayerCars(int playerId)
        {
            try
            {
                var result = await _getPlayerGarageItemsUseCase.ExecuteAsync(playerId, "Auto");
                return Ok(MapToDto(result));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("does not exist"))
            {
                return NotFound(new ApiErrorResponse
                {
                    Error = "Player Not Found",
                    Message = "The requested player could not be found",
                    StatusCode = 404
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Error = "Invalid Request",
                    Message = "The request contains invalid parameters",
                    StatusCode = 400
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Error = "Internal Server Error",
                    Message = "An unexpected error occurred while processing your request",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Get player's characters
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>List of characters with ownership and active status</returns>
        [HttpGet("characters/{playerId}")]
        public async Task<ActionResult<GarageItemsResponseDto>> GetPlayerCharacters(int playerId)
        {
            try
            {
                var result = await _getPlayerGarageItemsUseCase.ExecuteAsync(playerId, "Personaje");
                return Ok(MapToDto(result));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("does not exist"))
            {
                return NotFound(new ApiErrorResponse
                {
                    Error = "Player Not Found",
                    Message = "The requested player could not be found",
                    StatusCode = 404
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Error = "Invalid Request",
                    Message = "The request contains invalid parameters",
                    StatusCode = 400
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Error = "Internal Server Error",
                    Message = "An unexpected error occurred while processing your request",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Get player's backgrounds
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <returns>List of backgrounds with ownership and active status</returns>
        [HttpGet("backgrounds/{playerId}")]
        public async Task<ActionResult<GarageItemsResponseDto>> GetPlayerBackgrounds(int playerId)
        {
            try
            {
                var result = await _getPlayerGarageItemsUseCase.ExecuteAsync(playerId, "Fondo");
                return Ok(MapToDto(result));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("does not exist"))
            {
                return NotFound(new ApiErrorResponse
                {
                    Error = "Player Not Found",
                    Message = "The requested player could not be found",
                    StatusCode = 404
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Error = "Invalid Request",
                    Message = "The request contains invalid parameters",
                    StatusCode = 400
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Error = "Internal Server Error",
                    Message = "An unexpected error occurred while processing your request",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Activate a player's item (car, character, or background)
        /// </summary>
        /// <param name="playerId">The player ID</param>
        /// <param name="productId">The product ID to activate</param>
        /// <param name="productType">The product type (Auto, Personaje, Fondo)</param>
        /// <returns>Success or failure status</returns>
        [HttpPut("players/{playerId}/items/{productId}/activate")]
        public async Task<ActionResult<ActivateItemResponseDto>> ActivatePlayerItem(
            int playerId, 
            int productId, 
            [FromQuery] string productType)
        {
            try
            {
                var domainRequest = new ActivateItemRequest
                {
                    PlayerId = playerId,
                    ProductId = productId,
                    ProductType = productType
                };

                var result = await _activatePlayerItemUseCase.ExecuteAsync(domainRequest);
                
                if (result)
                {
                    return Ok(new ActivateItemResponseDto
                    {
                        Success = true,
                        Message = "Item activated successfully"
                    });
                }
                else
                {
                    return NotFound(new ApiErrorResponse
                    {
                        Error = "Item Not Found",
                        Message = "The requested item could not be found or is not owned by the player",
                        StatusCode = 404
                    });
                }
            }
            catch (ArgumentException ex) when (ex.Message.Contains("does not exist"))
            {
                return NotFound(new ApiErrorResponse
                {
                    Error = "Player Not Found",
                    Message = "The requested player could not be found",
                    StatusCode = 404
                });
            }
            catch (ArgumentException)
            {
                return BadRequest(new ApiErrorResponse
                {
                    Error = "Invalid Request",
                    Message = "The request contains invalid parameters",
                    StatusCode = 400
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiErrorResponse
                {
                    Error = "Internal Server Error",
                    Message = "An unexpected error occurred while processing your request",
                    StatusCode = 500
                });
            }
        }

        private GarageItemsResponseDto MapToDto(GarageItemsResponse response)
        {
            return new GarageItemsResponseDto
            {
                Items = response.Items.Select(item => new GarageItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    ProductType = item.ProductType,
                    Rarity = item.Rarity,
                    IsOwned = item.IsOwned,
                    IsActive = item.IsActive
                }).ToList(),
                ActiveItem = response.ActiveItem != null ? new GarageItemDto
                {
                    Id = response.ActiveItem.Id,
                    ProductId = response.ActiveItem.ProductId,
                    Name = response.ActiveItem.Name,
                    Description = response.ActiveItem.Description,
                    Price = response.ActiveItem.Price,
                    ProductType = response.ActiveItem.ProductType,
                    Rarity = response.ActiveItem.Rarity,
                    IsOwned = response.ActiveItem.IsOwned,
                    IsActive = response.ActiveItem.IsActive
                } : null,
                ItemType = response.ItemType
            };
        }
    }
}