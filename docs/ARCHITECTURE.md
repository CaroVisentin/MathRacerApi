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

## 📁 Carpetas Preparadas (con .gitkeep)

### Domain/
```
Domain/
├── Models/          # ✅ ApiInfoResponse.cs, HealthCheckResponse.cs
├── UseCases/        # ✅ GetApiInfoUseCase.cs, GetHealthStatusUseCase.cs
├── Repositories/    # � IGameRepository.cs, IPlayerRepository.cs
└── Services/        # 📁 IEmailService.cs, ICacheService.cs
```

### Infrastructure/
```
Infrastructure/
├── Repositories/    # 📁 GameRepository.cs (EF Core)
├── Services/        # 📁 EmailService.cs, CacheService.cs
├── Configuration/   # ✅ ServiceExtensions.cs
└── Providers/       # 📁 ExternalApiProvider.cs
```

### Presentation/
```
Presentation/
├── Controllers/     # ✅ HealthController.cs, InfoController.cs
├── DTOs/           # 📁 CreateGameDto.cs, GameResponseDto.cs
└── Extensions/      # 📁 SwaggerExtensions.cs, CorsExtensions.cs
```