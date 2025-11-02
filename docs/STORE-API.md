# 🛒 API de Tienda MathRacer

Este documento describe los endpoints de la tienda implementados para MathRacer API.

## 📋 Endpoints Disponibles

### 🚗 GET `/api/Store/cars`
Obtiene todos los autos disponibles en la tienda.

**Parámetros:**
- `playerId` (query) - ID del jugador para verificar propiedad

**Respuestas:**
- `200 OK` - Lista de autos con información de propiedad
- `404 Not Found` - Jugador no encontrado
- `500 Internal Server Error` - Error del servidor

### 👤 GET `/api/Store/characters`
Obtiene todos los personajes disponibles en la tienda.

**Parámetros:**
- `playerId` (query) - ID del jugador para verificar propiedad

**Respuestas:**
- `200 OK` - Lista de personajes con información de propiedad
- `404 Not Found` - Jugador no encontrado
- `500 Internal Server Error` - Error del servidor

### 🖼️ GET `/api/Store/backgrounds`
Obtiene todos los fondos disponibles en la tienda.

**Parámetros:**
- `playerId` (query) - ID del jugador para verificar propiedad

**Respuestas:**
- `200 OK` - Lista de fondos con información de propiedad
- `404 Not Found` - Jugador no encontrado
- `500 Internal Server Error` - Error del servidor

### 💰 POST `/api/Store/purchase`
Compra un producto de la tienda.

**Body (JSON):**
```json
{
  "playerId": 1,
  "productId": 3
}
```

**Respuestas:**
- `200 OK` - Compra exitosa
- `400 Bad Request` - Error en la compra (monedas insuficientes, producto ya poseído, etc.)
- `404 Not Found` - Jugador o producto no encontrado
- `500 Internal Server Error` - Error del servidor

## 📊 Estructura de Respuesta

### StoreResponseDto (GET endpoints)
```json
{
  "items": [
    {
      "id": 1,
      "name": "Auto Deportivo",
      "description": "Un auto rápido y elegante",
      "price": 500.00,
      "imageUrl": "",
      "productTypeId": 1,
      "productTypeName": "Auto",
      "rarity": "Común",
      "isOwned": true,
      "currency": "Coins"
    }
  ],
  "totalCount": 1
}
```

### PurchaseResponseDto (POST purchase)
```json
{
  "success": true,
  "message": "Compra realizada exitosamente",
  "remainingCoins": 750.00
}
```

## 🏗️ Arquitectura

La implementación sigue **Clean Architecture**:

### Domain Layer
- **Models**: `StoreItem`, `PurchaseResult`
- **Interfaces**: `IGetStoreCarsUseCase`, `IGetStoreCharactersUseCase`, `IGetStoreBackgroundsUseCase`, `IPurchaseStoreItemUseCase`
- **UseCases**: Implementaciones con lógica de negocio
- **Repository**: `IStoreRepository` para abstracción de datos

### Infrastructure Layer
- **Repository**: `StoreRepository` con Entity Framework Core
- **Entities**: Mapeo a entidades de base de datos
- **DI**: Registro de servicios en `ServiceExtensions`

### Presentation Layer
- **Controller**: `StoreController` con documentación Swagger completa
- **DTOs**: `StoreResponseDto`, `PurchaseRequestDto`, `PurchaseResponseDto`

## 🔧 Funcionalidades Clave

### ✅ Validaciones de Compra
1. **Existencia del jugador**: Verifica que el jugador exista
2. **Existencia del producto**: Confirma que el producto esté disponible
3. **Verificación de propiedad**: No permite compras duplicadas
4. **Validación de monedas**: Confirma fondos suficientes

### 🛡️ Transacciones ACID
- Uso de transacciones de base de datos
- Rollback automático en caso de error
- Consistencia garantizada en todas las operaciones

### 📝 Documentación Swagger
- Documentación completa con ejemplos
- Códigos de respuesta detallados
- Ejemplos de request/response
- Casos de error documentados

## 🚀 Testing

Usa el archivo `StoreAPI.http` incluido para probar todos los endpoints:

```bash
# Ejecutar la aplicación
dotnet run --project src/MathRacerAPI.Presentation/

# Abrir StoreAPI.http en VS Code y ejecutar requests
```

## 📖 Swagger UI

Accede a la documentación interactiva en:
```
http://localhost:5153/swagger
```

La documentación incluye:
- Descripción detallada de cada endpoint
- Ejemplos de request/response
- Códigos de estado y errores
- Modelos de datos interactivos