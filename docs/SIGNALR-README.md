# SignalR Integration - MathRacer Online Mode

Este documento explica cómo usar la integración de SignalR para el modo multijugador online siguiendo Clean Architecture.

## 🏗️ Arquitectura Clean para SignalR

### src/MathRacerAPI.Presentation/ (Presentation Layer)
- **Hubs/GameHub.cs**: Hub de SignalR que maneja comunicaciones en tiempo real
- **Controllers/OnlineController.cs**: Endpoints REST complementarios 
- **DTOs/SignalR/**: Objetos específicos para transferencia de datos SignalR
- **Configuration/ApplicationExtensions.cs**: Configuración de CORS y SignalR

### src/MathRacerAPI.Domain/ (Domain Layer - Lógica Pura)
#### Use Cases (Casos de Uso)
- **FindMatchUseCase.cs**: Busca/crea partidas multijugador online
- **ProcessOnlineAnswerUseCase.cs**: Procesa respuestas en tiempo real
- **GetNextOnlineQuestionUseCase.cs**: Obtiene siguiente pregunta
- **SubmitAnswerUseCase.cs**: Reutilizado para ambos modos (online/offline)

#### Domain Services (Lógica Compartida)
- **IGameLogicService**: Interface con lógica de juego (testeable)
  - ✅ Verificación de condiciones de finalización
  - ✅ Cálculo de posiciones de jugadores  
  - ✅ Aplicación de penalizaciones
  - ✅ Validaciones de estado de juego

#### Models
- **Game.cs**: Entidad principal del juego
- **Player.cs**: Jugadores con estado y posiciones
- **Question.cs**: Preguntas matemáticas
- **GameSession.cs**: Sesión de juego para SignalR

### src/MathRacerAPI.Infrastructure/ (Infrastructure Layer)
- **Services/GameLogicService.cs**: Implementación concreta del IGameLogicService
- **Repositories/InMemoryGameRepository.cs**: Persistencia en memoria
- **Providers/QuestionProvider.cs**: Proveedor de preguntas matemáticas

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

## 🔧 Configuración Clean Architecture

### Program.cs (Presentation Layer)
```csharp
// src/MathRacerAPI.Presentation/Program.cs

// Configurar servicios por capas
builder.Services.AddDomainServices();      // Domain registrations
builder.Services.AddInfrastructureServices(); // Infrastructure registrations  
builder.Services.AddPresentationServices();   // Presentation registrations

// CORS para SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()); // Necesario para SignalR
});

// SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configurar pipeline
app.UseCors("AllowFrontend");
app.MapHub<GameHub>("/gameHub"); // Hub en /gameHub
```

### ServiceExtensions (Infrastructure Layer)
```csharp
// src/MathRacerAPI.Infrastructure/Configuration/ServiceExtensions.cs
public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IGameRepository, InMemoryGameRepository>();
        services.AddScoped<IGameLogicService, GameLogicService>();
        services.AddSingleton<IQuestionProvider, QuestionProvider>();
        return services;
    }
}
```

## 💻 Ejemplo de Uso (JavaScript Client)

### Configuración Base
```javascript
// Conectar al hub SignalR (Presentation Layer)
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5153/gameHub") // Puerto configurado
    .withAutomaticReconnect() // Reconexión automática
    .build();
```

### Event Handlers (Recibir del Hub)
```javascript
// Configurar eventos que vienen del GameHub
connection.on("GameUpdate", (gameUpdate) => {
    console.log("🎮 Estado del juego actualizado:", gameUpdate);
    updateGameUI(gameUpdate); // Actualizar interfaz
});

connection.on("PlayerJoined", (playerInfo) => {
    console.log("👤 Jugador unido:", playerInfo);
    updatePlayersList(playerInfo);
});

connection.on("GameStarted", (gameData) => {
    console.log("🚀 Juego iniciado:", gameData);
    startGameUI(gameData);
});

connection.on("GameFinished", (result) => {
    console.log("🏆 Juego finalizado:", result);
    showGameResults(result);
});

connection.on("Error", (error) => {
    console.error("❌ Error:", error);
    showErrorMessage(error);
});
```

### Client Actions (Enviar al Hub)
```javascript
// Iniciar conexión y buscar partida
async function startOnlineGame(playerName) {
    try {
        await connection.start();
        console.log("🔌 Conectado al GameHub");
        
        // Invocar FindMatchUseCase via Hub
        await connection.invoke("FindMatch", playerName);
        
    } catch (err) {
        console.error("💥 Error de conexión:", err);
    }
}

// Enviar respuesta (ProcessOnlineAnswerUseCase)
async function sendAnswer(gameId, playerId, answer) {
    try {
        await connection.invoke("SendAnswer", gameId, playerId, answer);
    } catch (err) {
        console.error("💥 Error enviando respuesta:", err);
    }
}

// Desconectar limpiamente
function disconnectFromGame() {
    connection.stop();
}
```

### Integración con UI
```javascript
// Ejemplo de integración completa
class MathRacerOnline {
    constructor() {
        this.setupConnection();
        this.gameId = null;
        this.playerId = null;
    }
    
    setupConnection() {
        // ... configuración del connection como arriba
    }
    
    async joinGame(playerName) {
        await this.startOnlineGame(playerName);
    }
    
    submitAnswer(answer) {
        if (this.gameId && this.playerId) {
            this.sendAnswer(this.gameId, this.playerId, answer);
        }
    }
    
    updateGameUI(gameUpdate) {
        // Actualizar posiciones, puntajes, pregunta actual
        document.getElementById('current-question').textContent = gameUpdate.currentQuestion;
        // ... más lógica de UI
    }
}
```

## 🔄 Diferencias Modo Offline vs Online

| Aspecto | Modo Offline | Modo Online (SignalR) |
|---------|-------------|----------------------|
| **Comunicación** | HTTP REST (`/api/game`) | WebSocket (`/gameHub`) |
| **Use Cases** | `SubmitAnswerUseCase` | `ProcessOnlineAnswerUseCase` |
| **Controllers** | `GameController` | `GameHub` + `OnlineController` |
| **Tiempo Real** | ❌ Polling manual | ✅ Push automático |
| **Multijugador** | ❌ Individual | ✅ Tiempo real |

### Lógica Compartida (Clean Architecture)
- **Domain Layer**: Ambos modos usan el mismo `IGameLogicService`
- **Models**: Mismas entidades (`Game`, `Player`, `Question`)  
- **Repository**: Mismo `IGameRepository` para persistencia
- **Testing**: Mismos tests unitarios cubren ambos flujos

## 🧪 Testing SignalR

### Unit Tests (Existentes)
```csharp
// tests/MathRacerAPI.Tests/Services/GameLogicServiceTests.cs
[Fact]
public void CheckAndUpdateGameEndConditions_WhenPlayerReachesWinCondition_ShouldSetGameAsFinishedAndSetWinner()
{
    // Mismo test válido para offline y online
}
```

### Integration Tests (Sugeridos)
```csharp
// Mockear SignalR Hub Context
var mockHubContext = new Mock<IHubContext<GameHub>>();
var mockClients = new Mock<IHubCallerClients>();
mockHubContext.Setup(x => x.Clients).Returns(mockClients.Object);

// Testear GameHub methods
var gameHub = new GameHub(mockHubContext.Object, gameRepository, findMatchUseCase);
```

## 🚀 Consideraciones de Producción

### 1. **Escalabilidad**
- **Actual**: InMemory storage para desarrollo
- **Producción**: Migrar a SQL Server/PostgreSQL + Redis para SignalR scale-out

### 2. **Monitoreo**  
```csharp
// Métricas de SignalR
services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Solo desarrollo
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
});
```

### 3. **Seguridad**
- **Authentication**: JWT tokens para partidas privadas
- **Rate Limiting**: Limitar respuestas por segundo
- **Validation**: Validar todas las entradas del cliente

### 4. **Resilencia**
- **Reconnection**: Cliente debe manejar desconexiones
- **Timeouts**: Expulsar jugadores inactivos
- **Error Handling**: Rollback de estado en errores