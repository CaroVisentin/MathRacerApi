using Microsoft.AspNetCore.Mvc;
using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Presentation.DTOs;
using MathRacerAPI.Presentation.Mappers;
using Swashbuckle.AspNetCore.Annotations;

namespace MathRacerAPI.Presentation.Controllers;

[ApiController]
[Route("api/characters")]

public class CharactersController : ControllerBase
{
    private readonly GetStoreCharactersUseCase _getStoreCharactersUseCase;
    private readonly PurchaseStoreItemUseCase _purchaseStoreItemUseCase;

    public CharactersController(
        GetStoreCharactersUseCase getStoreCharactersUseCase,
        PurchaseStoreItemUseCase purchaseStoreItemUseCase)
    {
        _getStoreCharactersUseCase = getStoreCharactersUseCase;
        _purchaseStoreItemUseCase = purchaseStoreItemUseCase;
    }

    [SwaggerOperation(
        Summary = "Obtiene el catálogo de personajes disponibles",
        Description = "Retorna el catálogo completo de personajes en la tienda con precios, rareza e información de propiedad del jugador específico",
        OperationId = "GetAllCharacters",
        Tags = new[] { "Characters - Tienda de personajes" })]
    [SwaggerResponse(200, "Catálogo de personajes obtenido exitosamente", typeof(StoreResponseDto))]
    [SwaggerResponse(400, "ID de jugador inválido")]
    [SwaggerResponse(404, "Jugador no encontrado")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpGet]
    public async Task<ActionResult<StoreResponseDto>> GetAllCharacters([FromQuery] int playerId)
    {
        if (playerId <= 0)
        {
            throw new ValidationException("El ID del jugador debe ser mayor a 0");
        }

        var characters = await _getStoreCharactersUseCase.ExecuteAsync(playerId);
        var response = characters.ToStoreResponseDto();
        return Ok(response);
    }

    [SwaggerOperation(
        Summary = "Compra un personaje específico",
        Description = "Realiza la compra de un personaje específico para el jugador. Valida fondos suficientes y que el jugador no posea el personaje previamente.",
        OperationId = "PurchaseCharacter",
        Tags = new[] { "Characters - Tienda de personajes" })]
    [SwaggerResponse(200, "Personaje comprado exitosamente", typeof(PurchaseSuccessResponseDto))]
    [SwaggerResponse(400, "Fondos insuficientes")]
    [SwaggerResponse(404, "Jugador o personaje no encontrado")]
    [SwaggerResponse(409, "El jugador ya posee este personaje")]
    [SwaggerResponse(500, "Error interno del servidor")]
    [HttpPost("/api/players/{playerId}/characters/{characterId}")]
    public async Task<ActionResult<PurchaseSuccessResponseDto>> PurchaseCharacter(int playerId, int characterId)
    {
        var remainingCoins = await _purchaseStoreItemUseCase.ExecuteAsync(playerId, characterId);
        return Ok(remainingCoins.ToSuccessDto());
    }
}
