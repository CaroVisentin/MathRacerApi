using MathRacerAPI.Domain.Exceptions;
using MathRacerAPI.Domain.Repositories;

namespace MathRacerAPI.Domain.UseCases;

/// <summary>
/// Caso de uso para comprar wildcards de la tienda
/// </summary>
public class PurchaseWildcardUseCase
{
    private readonly IWildcardRepository _wildcardRepository;
    private readonly IPlayerRepository _playerRepository;

    public PurchaseWildcardUseCase(IWildcardRepository wildcardRepository, IPlayerRepository playerRepository)
    {
        _wildcardRepository = wildcardRepository;
        _playerRepository = playerRepository;
    }

    /// <summary>
    /// Procesa la compra de wildcards para un jugador
    /// </summary>
    /// <param name="playerId">ID del jugador que compra</param>
    /// <param name="wildcardId">ID del wildcard a comprar</param>
    /// <param name="quantity">Cantidad de wildcards a comprar (por defecto 1)</param>
    /// <returns>Resultado de la compra con información de éxito, mensaje y nueva cantidad</returns>
    /// <exception cref="NotFoundException">Se lanza cuando el jugador o wildcard no existe</exception>
    /// <exception cref="ValidationException">Se lanza cuando la cantidad es inválida</exception>
    /// <exception cref="ConflictException">Se lanza cuando excede el límite máximo</exception>
    /// <exception cref="InsufficientFundsException">Se lanza cuando no tiene suficientes monedas</exception>
    /// <exception cref="BusinessException">Se lanza cuando hay un error de lógica de negocio</exception>
    public async Task<(bool success, string message, int newQuantity)> ExecuteAsync(int playerId, int wildcardId, int quantity = 1)
    {
        // Verificar que el jugador existe
        var player = await _playerRepository.GetByIdAsync(playerId);
        if (player == null)
        {
            throw new NotFoundException("Jugador no encontrado");
        }

        // Validar cantidad
        if (quantity <= 0)
        {
            throw new ValidationException("La cantidad debe ser mayor a cero");
        }

        // Obtener información del wildcard
        var wildcard = await _wildcardRepository.GetWildcardByIdAsync(wildcardId);
        if (wildcard == null)
        {
            throw new NotFoundException("Wildcard no encontrado");
        }

        // Obtener cantidad actual del jugador para este wildcard
        var playerWildcards = await _wildcardRepository.GetPlayerWildcardsAsync(playerId);
        var currentWildcard = playerWildcards.FirstOrDefault(pw => pw.WildcardId == wildcardId);
        var currentQuantity = currentWildcard?.Quantity ?? 0;

        // Verificar que no exceda el máximo de 99 comodines
        const int MAX_WILDCARD_QUANTITY = 99;
        if (currentQuantity + quantity > MAX_WILDCARD_QUANTITY)
        {
            var maxCanBuy = MAX_WILDCARD_QUANTITY - currentQuantity;
            if (maxCanBuy <= 0)
            {
                throw new ConflictException("Ya tienes la cantidad máxima de este wildcard (99 unidades)");
            }
            throw new ConflictException($"Solo puedes comprar {maxCanBuy} unidades más de este wildcard. Ya tienes {currentQuantity}/{MAX_WILDCARD_QUANTITY}");
        }

        // Calcular precio total
        var pricePerUnit = (int)wildcard.Price;
        var totalPrice = pricePerUnit * quantity;

        // Verificar que el jugador tiene suficientes monedas
        if (player.Coins < totalPrice)
        {
            throw new InsufficientFundsException($"No tienes suficientes monedas. Necesitas {totalPrice}, tienes {player.Coins}");
        }

        // Procesar la compra (transacción)
        var purchaseSuccessful = await _wildcardRepository.PurchaseWildcardAsync(playerId, wildcardId, quantity, totalPrice);
        
        if (!purchaseSuccessful)
        {
            throw new BusinessException("Error al procesar la compra del wildcard");
        }

        // Devolver el resultado de la compra
        var newQuantity = currentQuantity + quantity;
        var message = quantity == 1 
            ? $"¡Compra exitosa! Ahora tienes {newQuantity} unidades de {wildcard.Name}"
            : $"¡Compra exitosa! Compraste {quantity} unidades. Ahora tienes {newQuantity} unidades de {wildcard.Name}";

        return (true, message, newQuantity);
    }
}