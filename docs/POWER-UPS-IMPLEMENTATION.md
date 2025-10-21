# Power-ups Implementation Guide

## 📋 Resumen General

El sistema de power-ups está diseñado para partidas **multijugador online** con **SignalR**. Cada jugador recibe power-ups al inicio de la partida y puede usarlos **solo una vez** durante toda la partida.

## 🎮 Power-ups Disponibles

### 1. **Double Points** (Backend)
- **Descripción**: La siguiente respuesta correcta cuenta como 2 respuestas correctas
- **Implementación**: Completamente en el backend
- **Efecto**: Se activa automáticamente en la próxima respuesta correcta del jugador
- **Ejemplo**: Jugador tiene 5/10 → usa power-up → responde bien → queda 7/10

### 2. **Shuffle Rival** (Backend) 
- **Descripción**: Mezcla de forma inmediata las opciones de respuesta de la pregunta actual del oponente
- **Implementación**: Completamente en el backend
- **Efecto**: El rival verá las opciones en orden diferente en su pregunta actual, de forma inmediata

### 3. **50/50** (Frontend únicamente)
- **Descripción**: Elimina 2 opciones incorrectas de la pregunta actual
- **Implementación**: Solo en el frontend (sin comunicación con el backend)
- **Efecto**: Visual, el jugador ve solo 2 opciones (la correcta + 1 incorrecta)

## 🏗️ Arquitectura Backend

### Modelos de Dominio

```csharp
// Tipos de power-ups disponibles
public enum PowerUpType
{
    DoublePoints = 1,   // Duplica puntos de siguiente respuesta correcta
    ShuffleRival = 2    // Mezcla opciones del oponente
}

// Power-up disponible para un jugador
public class PowerUp
{
    public int Id { get; set; }
    public PowerUpType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

// Efecto activo en el juego
public class ActiveEffect
{
    public int Id { get; set; }
    public PowerUpType Type { get; set; }
    public int SourcePlayerId { get; set; }        // Quien activó el power-up
    public int? TargetPlayerId { get; set; }       // A quien afecta (null = propio)
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int QuestionsRemaining { get; set; }    // Preguntas que dura el efecto
    public bool IsActive { get; set; }
    public Dictionary<string, object> Properties { get; set; }
}
```

### Propiedades del Jugador

```csharp
public class Player
{
    // ... otras propiedades
    public List<PowerUp> AvailablePowerUps { get; set; } = new();
    public bool HasDoublePointsActive { get; set; } = false;
}
```

### Propiedades del Juego

```csharp
public class Game
{
    // ... otras propiedades
    public bool PowerUpsEnabled { get; set; } = false;
    public List<ActiveEffect> ActiveEffects { get; set; } = new();
    public int MaxPowerUpsPerPlayer { get; set; } = 3;
}
```

## 📡 SignalR Communication

### DTOs para el Frontend

```csharp
// Power-up disponible
public class PowerUpDto
{
    public int Id { get; set; }
    public int Type { get; set; }              // PowerUpType como int
    public string Name { get; set; }
    public string Description { get; set; }
}

// Notificación de uso de power-up
public class PowerUpUsedDto
{
    public int GameId { get; set; }
    public int PlayerId { get; set; }
    public PowerUpType PowerUpType { get; set; }
}

// Efecto activo en el juego
public class ActiveEffectDto
{
    public int Id { get; set; }
    public int Type { get; set; }
    public int SourcePlayerId { get; set; }
    public int? TargetPlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int QuestionsRemaining { get; set; }
}
```

### Métodos SignalR del GameHub

```csharp
// Método para usar un power-up
public async Task UsePowerUp(int gameId, int playerId, PowerUpType powerUpType)
```

### Notas sobre preguntas y condición de victoria
- Cada partida genera `MaxQuestions` preguntas por defecto (valor actual: 30).
- La condición para ganar es `ConditionToWin` respuestas correctas (por defecto 10). Si un jugador alcanza este umbral la partida termina inmediatamente.
- Si se terminan todas las preguntas (`IndexAnswered >= MaxQuestions`) y nadie alcanzó la condición, gana el que tenga más respuestas correctas; en empate, gana el que finalizó primero.

### Eventos SignalR que recibe el Frontend

```csharp
// Actualización completa del estado del juego
"GameUpdate" -> GameUpdateDto

// Notificación de power-up usado
// Payload: PowerUpUsedDto (GameId, PlayerId, PowerUpType, TargetPlayerId?)
"PowerUpUsed" -> PowerUpUsedDto

// Errores
"Error" -> string
```

## 🔄 Flujo de Implementación

### 1. Inicio de Partida

**Backend:**
```csharp
// Al crear jugador en FindMatchUseCase
player.AvailablePowerUps = _powerUpService.GrantInitialPowerUps(player.Id);

// Contenido de GrantInitialPowerUps:
return new List<PowerUp>
{
    CreatePowerUp(PowerUpType.DoublePoints),    // "Puntos Dobles"
    CreatePowerUp(PowerUpType.ShuffleRival)     // "Confundir Rival"
};

// Al crear partida online
game.PowerUpsEnabled = true;
game.MaxPowerUpsPerPlayer = 3;
```

**Frontend recibe:**
```json
{
  "gameId": 1001,
  "status": "InProgress",
  "players": [
    {
      "id": 1001,
      "name": "Player1",
      "availablePowerUps": [
        { "id": 1, "type": 1, "name": "Puntos Dobles", "description": "La siguiente respuesta correcta cuenta como 2" },
        { "id": 2, "type": 2, "name": "Confundir Rival", "description": "Cambia el orden de las opciones del oponente" }
      ]
    }
  ],
  "powerUpsEnabled": true
}
```

### 2. Uso de Power-up (Double Points)

**Frontend envía:**
```javascript
// Activar power-up de puntos dobles
connection.invoke("UsePowerUp", gameId, playerId, 1); // 1 = DoublePoints
```

**Backend procesa:**
```csharp
// En PowerUpService.UsePowerUp()
1. Busca el power-up en player.AvailablePowerUps
2. Remueve el power-up de la lista (consumido)
3. Activa player.HasDoublePointsActive = true
4. Crea ActiveEffect para trackear el efecto
5. Guarda la partida actualizada
```

**Frontend recibe:**
```javascript
// Notificación de uso
connection.on("PowerUpUsed", (data) => {
  console.log(`Jugador ${data.playerId} usó ${data.powerUpType}`);
  if (data.targetPlayerId) {
    if (data.targetPlayerId === myPlayerId) {
      // Mostrar notificación específica al afectado
      showNotification("Tus opciones han sido mezcladas", 'info');
    }
  }
});

// Estado actualizado
connection.on("GameUpdate", (gameState) => {
  // El jugador ya no tiene ese power-up en availablePowerUps
  // El efecto aparece en activeEffects
});
```

### 3. Aplicación del Efecto (Double Points)

**Cuando el jugador responde correctamente:**
```csharp
// En GameLogicService.ApplyAnswerResult()
if (isCorrect)
{
    if (player.HasDoublePointsActive)
    {
        player.CorrectAnswers += 2; // Suma 2 respuestas correctas (avanza 2 posiciones)
        player.HasDoublePointsActive = false; // Se desactiva automáticamente
    }
    else
    {
        player.CorrectAnswers++; // Suma 1 respuesta correcta normal
    }
}
```

### 4. Uso de Power-up (Shuffle Rival)

**Frontend envía:**
```javascript
// Activar power-up de confundir rival
connection.invoke("UsePowerUp", gameId, playerId, 2); // 2 = ShuffleRival
```

**Backend procesa:**
```csharp
// En PowerUpService.UsePowerUp()
1. Remueve power-up de la lista del jugador
2. Encuentra al oponente (targetPlayer)
3. Crea ActiveEffect que afecta al oponente
4. El efecto se aplicará de forma inmediata en la pregunta actual del rival
```

**Efecto en próxima pregunta del rival:**
```csharp
// En GetNextOnlineQuestionUseCase o similar
// Si existe un efecto de ShuffleRival activo y contiene las opciones precomputadas,
// esas opciones se devolverán inmediatamente para la pregunta actual del rival.
```

## ⚡ Puntos Clave para el Frontend

### 1. **Gestión de Estado**
- **50/50**: Estado local por pregunta (se resetea en nueva pregunta)
- **Backend power-ups**: Estado sincronizado via SignalR GameUpdate
- **Efectos activos**: Mostrar notificaciones cuando el rival los use

### 2. **Validaciones**
- **50/50**: Verificar que no se haya usado en la pregunta actual
- **Backend**: Los power-ups desaparecen automáticamente al usarse
- **Una vez por partida**: Una vez usado, no vuelve a aparecer

### 3. **UX/UI Recommendations**
- **Botones deshabilitados**: Cuando ya se usaron
- **Indicadores visuales**: Mostrar efectos activos del rival
- **Feedback inmediato**: Confirmación visual del uso
- **Tooltips**: Descripción clara de cada power-up

### 4. **Manejo de Errores**
- Escuchar evento "Error" para mostrar mensajes específicos de power-ups
- Validar estado del juego antes de permitir uso de power-ups
- Mostrar feedback claro cuando no se puede usar un power-up

## 🔧 Testing

### Casos de Prueba Importantes

1. **Inicio de partida**: Verificar que cada jugador reciba 2 power-ups
2. **50/50 frontend**: Debe eliminar exactamente 2 opciones incorrectas
3. **Double Points**: Verificar que la siguiente respuesta correcta sume +2 al CorrectAnswers
4. **Shuffle Rival**: Verificar que las opciones del oponente se mezclen
5. **Una vez por partida**: Verificar que no se puedan reutilizar
6. **SignalR sync**: Verificar sincronización en tiempo real entre jugadores

Esta implementación garantiza una experiencia fluida y balanceada para ambos jugadores, con efectos claros y predecibles. ¡El sistema está listo para integración!