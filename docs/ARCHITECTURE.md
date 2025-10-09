# MathRacer API - Clean Architecture

## 🏗️ Capas de la Arquitectura

### 🎭 Presentation (Entrada HTTP)
- **Controllers/**: Maneja requests HTTP, delega a Use Cases, retorna responses
- **DTOs/**: Objetos para requests/responses (Input/Output de la API)
- **Extensions/**: Configuración de Swagger, CORS, middleware HTTP

### 💎 Domain (Núcleo del Negocio)
- **Models/**: Entidades del negocio (Game, Player, Question, etc.)
- **UseCases/**: Lógica de negocio pura (CreateGame, JoinGame, CalculateScore)
- **Repositories/**: Interfaces para acceder a datos (sin implementación)
- **Services/**: Interfaces para servicios externos (sin implementación)

### 🔧 Infrastructure (Implementaciones)
- **Repositories/**: Implementa interfaces de Domain (EF Core, Dapper, etc.)
- **Services/**: Implementa servicios externos (APIs, Email, Cache)
- **Configuration/**: Registro de dependencias (DI Container)
- **Providers/**: Proveedores de datos externos (APIs, Files)

## 🔄 Flujo de Datos
```
HTTP Request → Controller → Use Case → Repository/Service → Database/External
            ← DTO        ← Model    ← Interface      ← Data
```

## 🔗 Reglas de Dependencias
- **Presentation** → Depende de **Domain** (usa Use Cases)
- **Infrastructure** → Depende de **Domain** (implementa interfaces)
- **Domain** → **NO depende de nadie** (núcleo independiente)

## ⚡ Interacción Entre Capas

### 1️⃣ **Request HTTP llega a Presentation**
```
Cliente HTTP → Controller
```
- El **Controller** recibe el request HTTP
- Valida los datos de entrada (DTOs)
- NO tiene lógica de negocio

### 2️⃣ **Presentation llama a Domain**
```
Controller → Use Case (Domain)
```
- **Controller** instancia el **Use Case** (vía DI)
- Pasa los datos necesarios al **Use Case**
- El **Use Case** contiene toda la lógica de negocio

### 3️⃣ **Domain usa Infrastructure (via Interfaces)**
```
Use Case → IRepository/IService → Implementation (Infrastructure)
```
- **Use Case** define QUÉ necesita (interface)
- **Infrastructure** define CÓMO lo obtiene (implementación)
- **DI Container** conecta interface con implementación

### 4️⃣ **Infrastructure accede a recursos externos**
```
Repository → Database/API/Cache
Service → Email/File System/External APIs
```
- **Repositories** manejan persistencia de datos
- **Services** manejan servicios externos
- **Providers** conectan con APIs de terceros

### 5️⃣ **Response regresa por las capas**
```
Data → Implementation → Use Case → Controller → HTTP Response
```
- Los datos regresan por el mismo camino
- **Use Case** procesa y retorna **Models**
- **Controller** convierte Models a **DTOs** para HTTP

## 🎯 Beneficios de esta Interacción
- **Testeable**: Cada capa se puede testear independientemente
- **Flexible**: Cambiar Infrastructure no afecta el Domain
- **Mantenible**: Lógica de negocio centralizada en Domain
- **Escalable**: Fácil agregar nuevas funcionalidades

## 📁 Estructura Actual del Proyecto

### src/MathRacerAPI.Domain/ (Núcleo de Negocio)
```
Domain/
├── Models/          # ✅ Game.cs, Player.cs, Question.cs, GameStatus.cs
├── UseCases/        # ✅ CreateGameUseCase.cs, JoinGameUseCase.cs, SubmitAnswerUseCase.cs
├── Repositories/    # ✅ IGameRepository.cs
└── Services/        # ✅ IGameLogicService.cs
```

### src/MathRacerAPI.Infrastructure/ (Implementaciones)
```
Infrastructure/
├── Repositories/    # ✅ InMemoryGameRepository.cs
├── Services/        # ✅ GameLogicService.cs
├── Configuration/   # ✅ ServiceExtensions.cs
└── Providers/       # ✅ QuestionProvider.cs, ecuaciones.json
```

### src/MathRacerAPI.Presentation/ (API & SignalR)
```
Presentation/
├── Controllers/     # ✅ GameController.cs, HealthController.cs, InfoController.cs, OnlineController.cs
├── DTOs/           # ✅ GameResponseDto.cs, CreateGameRequestDto.cs, QuestionResponseDto.cs
├── Hubs/           # ✅ GameHub.cs (SignalR para multijugador en tiempo real)
└── Configuration/   # ✅ ApplicationExtensions.cs
```

### tests/MathRacerAPI.Tests/ (Testing)
```
Tests/
├── Services/        # ✅ GameLogicServiceTests.cs (11 tests)
├── UseCases/        # ✅ SubmitAnswerUseCaseTests.cs (11 tests)
└── Dependencies     # ✅ xUnit, Moq, FluentAssertions
```

## 🧪 Testing Coverage
- **22+ Tests Unitarios** cubriendo lógica crítica
- **Mocking** con Moq para interfaces y dependencias
- **GameLogicService**: Condiciones de victoria, penalizaciones, posiciones
- **Use Cases**: Validaciones, flujos de negocio, manejo de errores

## 🔄 Flujo SignalR Multijugador
```
Cliente WebSocket → GameHub → Use Case → GameLogicService → Repository
               ← Broadcast ← Model   ← Business Logic ← Data
```