# 🧮 MathRacer API

API para competencias matemáticas en tiempo real con **Clean Architecture**, **SignalR** y **22+ Tests Unitarios**.

## 🏗️ Arquitectura Clean

✅ **Migración Completa a Clean Architecture**
- 🎭 **src/MathRacerAPI.Presentation/**: Controllers, SignalR Hubs, DTOs
- 💎 **src/MathRacerAPI.Domain/**: Models, Use Cases, Interfaces (Lógica Pura)  
- 🔧 **src/MathRacerAPI.Infrastructure/**: Repositories, Services, Providers
- 🧪 **tests/MathRacerAPI.Tests/**: 22+ Tests con xUnit, Moq, FluentAssertions

📖 **Documentación**: [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) | [docs/SIGNALR-README.md](./docs/SIGNALR-README.md)

## 🚀 Quick Start

### Requisitos
- .NET 8.0 SDK
- Visual Studio 2022 / VS Code / Rider

### Ejecutar localmente
```bash
git clone https://github.com/CaroVisentin/MathRacerApi.git
cd MathRacerApi

# Restaurar dependencias
dotnet restore

# Ejecutar API
dotnet run --project src/MathRacerAPI.Presentation/

# Ejecutar tests
dotnet test
```

🌐 **Endpoints**: 
- **API**: http://localhost:5153
- **SignalR**: ws://localhost:5153/gamehub  
- **Swagger**: http://localhost:5153/swagger


## 🔑 Configuración de credenciales Firebase

**Importante:** No subas el archivo de credenciales de Firebase al repositorio. Cada desarrollador debe:

1. Obtener su propio archivo de credenciales desde la consola de Firebase.
2. Guardar el archivo localmente como `firebase-credentials.json` en la raíz del proyecto.
3. Verifica que el archivo `firebase-credentials.json` esté en `.gitignore` para evitar subirlo al repositorio.

**¡Listo!** No es necesario configurar ninguna variable de entorno ni editar `launchSettings.json`. El backend detecta automáticamente el archivo en la raíz.

## 🎮 Funcionalidades Implementadas


### ✅ Endpoints REST

#### Game Endpoints
- `POST /api/games` - Crear nueva partida matemática
- `GET /api/games/{id}` - Obtener estado de partida
- `POST /api/games/{id}/join` - Unirse a partida
- `POST /api/games/{id}/answer` - Enviar respuesta matemática
- `GET /api/games/{id}/question` - Obtener siguiente pregunta

#### Player Endpoints
- `POST /api/player/register` - Registrar nuevo jugador (requiere idToken en header)
- `POST /api/player/login` - Login de jugador (requiere idToken en header)
- `POST /api/player/google` - Login con Google/Firebase (requiere idToken en header)
- `GET /api/player/{id}` - Obtener perfil de jugador por ID (requiere idToken en header)
- `GET /api/player/uid/{uid}` - Obtener perfil de jugador por UID de Firebase (requiere idToken en header)

#### Online Endpoints
- `GET /api/online/game/{gameId}` - Obtener información de partida online
- `GET /api/online/connection-info` - Obtener información de conexión SignalR

#### Worlds Endpoints
- `GET /api/worlds/player/{playerId}` - Obtener mundos disponibles para un jugador

#### Sistema & Monitoring
- `GET /health` - Health Check detallado
- `GET /api/info` - Información de la API
- **Swagger**: Documentación interactiva completa

### ✅ SignalR Multijugador (Tiempo Real)
- **Hub**: `/gamehub` - Conexión WebSocket
- **FindMatch**: Matchmaking automático
- **SendAnswer**: Respuestas en tiempo real
- **GameUpdate**: Notificaciones instantáneas de estado

## 🛠️ Desarrollo

### Estructura Clean Architecture
```
MathRacerApi/
├── src/
│   ├── 🎭 MathRacerAPI.Presentation/   # API Controllers, SignalR Hubs
│   ├── 💎 MathRacerAPI.Domain/         # Business Logic, Use Cases  
│   └── 🔧 MathRacerAPI.Infrastructure/ # Data Access, External Services
├── tests/
│   └── 🧪 MathRacerAPI.Tests/         # Unit Tests (22+ tests)
└── docs/                               # Documentation
```

### Comandos de Desarrollo
```bash
# Compilar por capas (orden de dependencias)
dotnet build src/MathRacerAPI.Domain/
dotnet build src/MathRacerAPI.Infrastructure/
dotnet build src/MathRacerAPI.Presentation/

# Ejecutar API
dotnet run --project src/MathRacerAPI.Presentation/

# Tests con cobertura
dotnet test --collect:"XPlat Code Coverage"

# Tests en modo watch
dotnet test --watch

# Docker (multi-stage optimizado)
docker build -t mathracer-api .
docker run -p 5153:5153 mathracer-api
```

### 🧪 Testing (22+ Tests Implementados)
```bash
# Ejecutar todos los tests
dotnet test tests/MathRacerAPI.Tests/

# Tests específicos
dotnet test --filter "GameLogicServiceTests"
dotnet test --filter "SubmitAnswerUseCaseTests"

# Cobertura detallada
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage
```

## 🔄 CI/CD & Deployment

### GitHub Actions (Configurado)
- ✅ **Build por capas** respetando Clean Architecture
- ✅ **22+ Tests automáticos** con cobertura de código
- ✅ **Docker build validation** 
- ✅ **Architecture validation** automática
- ✅ **Deploy automático** desde `main` a producción

### Deployment (Render)
- **Plataforma**: Render (Docker multi-stage)
- **Puerto**: 5153 (configurado para producción)
- **Branch**: Auto-deploy desde `main`
- **Pipeline**: Build → Test → Docker → Deploy

📖 **Detalles completos**: [docs/CI-CD-README.md](./docs/CI-CD-README.md)

## ✅ Estado Actual (Completado)

### 🏗️ **Clean Architecture Migration**
- [x] **Domain Layer**: Models, Use Cases, Interfaces  
- [x] **Infrastructure Layer**: Services, Repositories, Providers
- [x] **Presentation Layer**: Controllers, SignalR Hubs, DTOs
- [x] **Dependencies**: Correcta dirección de dependencias

### 🧪 **Testing Infrastructure**  
- [x] **GameLogicServiceTests**: 11 tests (lógica de juego)
- [x] **SubmitAnswerUseCaseTests**: 11 tests (casos de uso)
- [x] **Mocking**: Moq para interfaces y dependencias
- [x] **Assertions**: FluentAssertions para tests legibles

### 🎮 **Game Features Implementadas**
- [x] **Creación de juegos** con preguntas matemáticas
- [x] **Sistema de jugadores** y posiciones  
- [x] **Lógica de respuestas** correctas/incorrectas
- [x] **Penalizaciones** por respuestas incorrectas
- [x] **Condiciones de victoria** y finalización de juegos
- [x] **SignalR Hub** para multijugador en tiempo real

### 🔧 **DevOps & Infrastructure**
- [x] **CI/CD Pipeline** con GitHub Actions
- [x] **Docker** con multi-stage build optimizado
- [x] **Health Checks** y monitoring
- [x] **Swagger** documentación completa
- [x] **CORS** configuración para desarrollo

## � Próximas Mejoras

### � **Persistencia & Escalabilidad**
- [ ] Migrar de InMemory a **SQL Server/PostgreSQL**
- [ ] Implementar **Entity Framework Core**
- [ ] **Redis** para sesiones SignalR distribuidas
- [ ] **Caching** estratégico para performance

### 🔒 **Seguridad & Autenticación**  
- [ ] **JWT Authentication** para usuarios
- [ ] **Authorization** por roles y permisos
- [ ] **Rate Limiting** para prevenir spam
- [ ] **Input Validation** mejorada

### 📈 **Features Avanzadas**
- [ ] **Rankings y estadísticas** de jugadores
- [ ] **Diferentes tipos** de preguntas matemáticas
- [ ] **Salas privadas** y públicas
- [ ] **Spectator mode** para observar partidas

## 🧪 Testing & Quality Assurance

### Tests Implementados (22+ Tests)
```bash
# Ejecutar todos los tests
dotnet test
# ✅ 22/22 tests passing

# Tests por categoría
dotnet test --filter "GameLogicServiceTests"     # 11 tests - Lógica de juego  
dotnet test --filter "SubmitAnswerUseCaseTests" # 11 tests - Casos de uso

# Cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Reporte detallado
dotnet test --verbosity normal --logger "console;verbosity=detailed"
```

### Coverage Actual
- **Domain Layer**: ✅ 100% - Lógica de negocio cubierta
- **Use Cases**: ✅ 95%+ - Flujos principales validados  
- **Services**: ✅ 90%+ - Implementaciones testeadas
- **Controllers**: 🔄 Próximo - Integration tests planeados

## 🔧 Contribución

### Workflow de Desarrollo
```bash
# 1. Crear rama desde main o rama específica (ej: refactor/clean-architecture)
git checkout main
git pull origin main
git checkout -b feature/nueva-funcionalidad

# 2. Hacer cambios siguiendo Clean Architecture
# 3. Commit siguiendo conventional commits
git commit -m "feat: agregar nuevo endpoint de usuario"

# 4. Push y crear PR hacia la rama correspondiente
git push origin feature/nueva-funcionalidad
```

### Convenciones del Proyecto
- **Branches**: `feature/`, `bugfix/`, `hotfix/`, `refactor/`
- **Commits**: [Conventional Commits](https://conventionalcommits.org/)
- **PRs**: Hacia rama base correspondiente (main/develop según contexto)
- **Code Style**: Seguir convenciones de C# y comentarios XML

### Agregar Nueva Funcionalidad
```csharp
// 1. Definir interfaz en Domain
public interface IGameService
{
    Task<Game> CreateGameAsync(CreateGameRequest request);
}

// 2. Implementar en Infrastructure  
public class GameService : IGameService { /* ... */ }

// 3. Registrar en ServiceExtensions
services.AddScoped<IGameService, GameService>();

// 4. Usar en Use Case
public class CreateGameUseCase
{
    private readonly IGameService _gameService;
    // ...
}
```

## 🔒 Seguridad & Configuración

### Variables de Entorno
```env
# Desarrollo
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:5001;http://localhost:5000

# Producción  
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

### Configuración CORS
```csharp
// Configurado para desarrollo - Actualizar para producción
app.UseCors(policy => policy
    .AllowAnyOrigin()      // ⚠️ Cambiar en producción
    .AllowAnyMethod()
    .AllowAnyHeader());
```

## 📊 Monitoring & Observability

### Health Checks Disponibles
- **Basic**: `/health` - Estado general, memoria, uptime
- **Detailed**: Información de dependencias (cuando se agreguen)

### Métricas (Futuras)
- Tiempos de respuesta de API
- Conexiones activas de SignalR
- Sesiones de juego simultáneas
- Tasa de errores por endpoint

## 📚 Documentación & Referencias

### 📖 **Guías del Proyecto**
- **🏗️ Arquitectura**: [docs/ARCHITECTURE.md](./docs/ARCHITECTURE.md) - Clean Architecture detallada
- **🔄 SignalR**: [docs/SIGNALR-README.md](./docs/SIGNALR-README.md) - Multijugador tiempo real  
- **🚀 CI/CD**: [docs/CI-CD-README.md](./docs/CI-CD-README.md) - Pipeline y deployment

### 🌐 **API & Testing**
- **Swagger Local**: http://localhost:5153/swagger - Documentación interactiva
- **Health Check**: http://localhost:5153/health - Estado de la aplicación
- **Test Coverage**: Reportes en `./coverage/` después de `dotnet test --collect:"XPlat Code Coverage"`

### 🔗 **Referencias Técnicas**
- **Clean Architecture**: [Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- **SignalR**: [Microsoft Docs](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- **xUnit Testing**: [xUnit.net Documentation](https://xunit.net/)

## 🤝 Soporte

- **🐛 Reportar Bugs**: [GitHub Issues](https://github.com/CaroVisentin/MathRacerApi/issues)
- **💡 Sugerencias**: [GitHub Discussions](https://github.com/CaroVisentin/MathRacerApi/discussions)  
- **📖 Documentación**: Consultar archivos `.md` en el repositorio

---

*Desarrollado con ❤️ usando Clean Architecture y .NET 8*