using MathRacerAPI.Domain.UseCases;
using MathRacerAPI.Presentation.DTOs.Chest;
using Microsoft.AspNetCore.Mvc;
using static MathRacerAPI.Domain.Models.ChestItem;

namespace MathRacerAPI.Presentation.Controllers;

/// <summary>
/// Controller para gestión de cofres
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Tags("Chest")]
public class ChestController : ControllerBase
{
    private readonly OpenRandomChestUseCase _openRandomChest;
    private readonly OpenTutorialChestUseCase _openTutorialChest;

    public ChestController(
        OpenRandomChestUseCase openRandomChest,
        OpenTutorialChestUseCase openTutorialChest)
    {
        _openRandomChest = openRandomChest;
        _openTutorialChest = openTutorialChest;
    }

    /// <summary>
    /// Abre un cofre aleatorio para el jugador autenticado
    /// POST /api/chest/open
    /// Requiere token de Firebase en header Authorization
    /// </summary>
    /// <response code="200">Cofre abierto exitosamente. Retorna los items obtenidos.</response>
    /// <response code="401">No autorizado. Token inválido o faltante.</response>
    /// <response code="404">Jugador no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/chest/open
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint:
    /// - Identifica automáticamente al jugador por su UID de Firebase
    /// - Genera 3 items aleatorios según probabilidades configuradas:
    ///   - 20% probabilidad de producto (rareza según RarityEntity.Probability)
    ///   - 50% probabilidad de monedas (100-1000)
    ///   - 30% probabilidad de wildcard (1-3)
    /// - Aplica automáticamente las recompensas a la cuenta del jugador
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///    {
    ///     "items": [
    ///       {
    ///           "type": "Product",
    ///           "quantity": 1,
    ///           "product": {
    ///             "id": 15,
    ///             "name": "Auto Veloz",
    ///             "description": "Un auto raro",
    ///             "productType": 1,
    ///             "rarityId": 3,
    ///             "rarityName": "Raro",
    ///             "rarityColor": "#9C27B0"
    ///           },
    ///           "wildcard": null,
    ///           "compensationCoins": 400  
    ///          },
    ///          {
    ///           "type": "Coins",
    ///           "quantity": 650,
    ///           "product": null,
    ///           "wildcard": null,
    ///           "compensationCoins": null
    ///          },
    ///          {
    ///          "type": "Wildcard",
    ///          "quantity": 2,
    ///          "product": null,
    ///          "wildcard": {
    ///             "id": 1,
    ///             "name": "Matafuego",
    ///             "description": "Permite eliminar una opción incorrecta"
    ///          },
    ///          "compensationCoins": null
    ///       }
    ///      ]
    ///    }
    /// </remarks>
    [HttpPost("open")]
    [ProducesResponseType(typeof(ChestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChestResponseDto>> OpenRandomChest()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;

        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var chest = await _openRandomChest.ExecuteAsync(uid);

        var response = new ChestResponseDto
        {
            Items = chest.Items.Select(item => new ChestItemDto
            {
                Type = item.Type.ToString(),
                Quantity = item.Quantity,
                Product = item.Product != null ? new ChestProductDto
                {
                    Id = item.Product.Id,
                    Name = item.Product.Name,
                    Description = item.Product.Description,
                    ProductType = item.Product.ProductType,
                    RarityId = item.Product.RarityId,
                    RarityName = item.Product.RarityName,
                    RarityColor = item.Product.RarityColor
                } : null,
                Wildcard = item.Wildcard != null ? new WildcardDto
                {
                    Id = item.Wildcard.Id,
                    Name = item.Wildcard.Name,
                    Description = item.Wildcard.Description
                } : null,
                CompensationCoins = item.CompensationCoins
            }).ToList()
        };

        return Ok(response);
    }

    /// <summary>
    /// Completa el tutorial del jugador y abre el cofre de bienvenida
    /// POST /api/chest/complete-tutorial
    /// Requiere token de Firebase en header Authorization
    /// </summary>
    /// <response code="200">Tutorial completado. Retorna el cofre con los productos iniciales.</response>
    /// <response code="401">No autorizado. Token inválido o faltante.</response>
    /// <response code="404">Jugador no encontrado.</response>
    /// <response code="500">Error interno del servidor.</response>
    /// <remarks>
    /// Ejemplo de solicitud:
    /// 
    ///     POST /api/chest/complete-tutorial
    ///     Headers:
    ///       Authorization: Bearer {firebase-id-token}
    /// 
    /// **Descripción:**
    /// 
    /// Este endpoint:
    /// - Marca el tutorial como completado
    /// - Abre el cofre de bienvenida con 3 productos comunes (1 auto, 1 personaje, 1 fondo)
    /// - Asigna los productos como activos automáticamente
    /// - Retorna el cofre para mostrar al usuario
    /// 
    /// **Flujo recomendado:**
    /// 1. Usuario se registra (POST /api/player/register)
    /// 2. Usuario completa el tutorial en el frontend
    /// 3. Frontend llama a este endpoint
    /// 4. Frontend muestra animación del cofre con los productos
    /// 
    /// **Ejemplo de respuesta exitosa (200):**
    /// 
    ///     {
    ///       "items": [
    ///         {
    ///           "type": "Product",
    ///           "quantity": 1,
    ///           "product": {
    ///             "id": 1,
    ///             "name": "Auto Común",
    ///             "description": "Tu primer auto",
    ///             "productType": 1,
    ///             "rarityId": 1,
    ///             "rarityName": "Común",
    ///             "rarityColor": "#FFFFFF"
    ///           },
    ///           "wildcard": null,
    ///           "compensationCoins": null
    ///         },
    ///         {
    ///           "type": "Product",
    ///           "quantity": 1,
    ///           "product": {
    ///             "id": 5,
    ///             "name": "Personaje Común",
    ///             "description": "Tu primer personaje",
    ///             "productType": 2,
    ///             "rarityId": 1,
    ///             "rarityName": "Común",
    ///             "rarityColor": "#FFFFFF"
    ///           },
    ///           "wildcard": null,
    ///           "compensationCoins": null
    ///         },
    ///         {
    ///           "type": "Product",
    ///           "quantity": 1,
    ///           "product": {
    ///             "id": 10,
    ///             "name": "Fondo Común",
    ///             "description": "Tu primer fondo",
    ///             "productType": 3,
    ///             "rarityId": 1,
    ///             "rarityName": "Común",
    ///             "rarityColor": "#FFFFFF"
    ///           },
    ///           "wildcard": null,
    ///           "compensationCoins": null
    ///         }
    ///       ]
    ///     }
    /// </remarks>
    [HttpPost("complete-tutorial")]
    [ProducesResponseType(typeof(ChestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChestResponseDto>> CompleteTutorial()
    {
        var uid = HttpContext.Items["FirebaseUid"] as string;

        if (string.IsNullOrEmpty(uid))
        {
            return Unauthorized(new { message = "Token de autenticación requerido o inválido." });
        }

        var chest = await _openTutorialChest.ExecuteAsync(uid);

        var response = new ChestResponseDto
        {
            Items = chest.Items.Select(item => new ChestItemDto
            {
                Type = item.Type.ToString(),
                Quantity = item.Quantity,
                Product = item.Product != null ? new ChestProductDto
                {
                    Id = item.Product.Id,
                    Name = item.Product.Name,
                    Description = item.Product.Description,
                    ProductType = item.Product.ProductType,
                    RarityId = item.Product.RarityId,
                    RarityName = item.Product.RarityName,
                    RarityColor = item.Product.RarityColor
                } : null,
                Wildcard = null, // Tutorial chest no contiene wildcards
                CompensationCoins = null // Tutorial chest no tiene compensación
            }).ToList()
        };

        return Ok(response);
    }
}