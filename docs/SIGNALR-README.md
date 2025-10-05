# SignalR Integration - MathRacer Online Mode

Este documento explica cómo usar la integración de SignalR para el modo multijugador online.

## Arquitectura

### Presentation Layer
- **GameHub**: Hub de SignalR que maneja las comunicaciones en tiempo real
- **OnlineController**: Controlador REST para obtener información sobre partidas online
- **DTOs**: Objetos de transferencia de datos específicos para SignalR

### Domain Layer
#### Use Cases (Casos de Uso)
- **FindMatchUseCase**: Busca o crea partidas multijugador online
- **ProcessOnlineAnswerUseCase**: Procesa respuestas en partidas online
- **GetNextOnlineQuestionUseCase**: Obtiene la siguiente pregunta
- Reutiliza casos de uso existentes para modo offline

#### Domain Services (Lógica Compartida)
- **IGameLogicService**: Servicio con lógica de juego compartida entre casos de uso
  - Verificación de condiciones de finalización
  - Cálculo de posiciones de jugadores
  - Aplicación de penalizaciones
  - Validaciones de estado de juego

#### Models
- **GameSession**: Modelo que representa el estado de una sesión de juego
- Reutiliza modelos existentes: `Game`, `Player`, `Question`

### Infrastructure Layer
- **GameLogicService**: Implementación del servicio de lógica compartida
- Integración con el repositorio existente `IGameRepository`

## Endpoints SignalR

### Hub: `/gameHub`

#### Métodos que puede llamar el cliente:

1. **FindMatch(string playerName)**
   - Busca una partida disponible o crea una nueva
   - Une al jugador al grupo de la partida
   - Retorna el estado inicial del juego

2. **SendAnswer(int gameId, int playerId, string answer)**
   - Procesa la respuesta de un jugador
   - Actualiza el estado del juego
   - Notifica a todos los jugadores del nuevo estado

#### Eventos que recibe el cliente:

1. **GameUpdate(GameUpdateDto gameUpdate)**
   - Se envía cuando hay cambios en el estado del juego
   - Incluye información de jugadores, pregunta actual, ganador, etc.

2. **Error(string message)**
   - Se envía cuando ocurre un error

## Endpoints REST (Opcionales)

### GET `/api/online/game/{gameId}`
Obtiene información sobre una partida online específica.

### GET `/api/online/connection-info`
Obtiene información de configuración para conectarse al hub de SignalR.

## Flujo de Juego

1. **Conexión**: El cliente se conecta al hub `/gameHub`
2. **Búsqueda de partida**: El cliente llama a `FindMatch(playerName)`
3. **Emparejamiento**: El sistema busca una partida disponible o crea una nueva
4. **Inicio del juego**: Cuando hay 2 jugadores, el juego comienza
5. **Respuestas**: Los jugadores llaman a `SendAnswer(gameId, playerId, answer)`
6. **Actualizaciones**: Todos reciben `GameUpdate` con el nuevo estado
7. **Finalización**: El juego termina cuando alguien alcanza la condición de victoria

## Configuración

### Cors
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // Necesario para SignalR
});
```

### SignalR
```csharp
builder.Services.AddSignalR();
app.MapHub<GameHub>("/gameHub");
```

## Ejemplo de Uso (JavaScript)

```javascript
// Conectar al hub
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gameHub")
    .build();

// Configurar eventos
connection.on("GameUpdate", (gameUpdate) => {
    console.log("Estado del juego:", gameUpdate);
    // Actualizar UI con el nuevo estado
});

connection.on("Error", (error) => {
    console.error("Error:", error);
});

// Iniciar conexión
connection.start().then(() => {
    // Buscar partida
    connection.invoke("FindMatch", "NombreJugador");
}).catch(err => console.error(err));

// Enviar respuesta
function sendAnswer(gameId, playerId, answer) {
    connection.invoke("SendAnswer", gameId, playerId, answer);
}
```

## Diferencias con el Modo Offline

- **Modo Offline**: Usa endpoints REST tradicionales (`/api/game`)
- **Modo Online**: Usa SignalR para comunicación en tiempo real
- **Lógica compartida**: Ambos modos reutilizan los casos de uso del dominio
- **Separación clara**: Los modos no tienen dependencias directas entre ellos

## Consideraciones

1. **Escalabilidad**: Esta implementación usa memoria para almacenar partidas. Para producción, considerar una base de datos.
2. **Reconexión**: Implementar lógica de reconexión en el cliente.
3. **Timeouts**: Considerar timeouts para partidas inactivas.
4. **Validación**: Añadir validaciones adicionales según necesidades del negocio.