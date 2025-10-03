# MathRacer API

API para el juego MathRacer - Competencias matemáticas en tiempo real desarrollada con **Clean Architecture**.

## 🏗️ Arquitectura

Este proyecto implementa **Clean Architecture** con 3 capas principales:
- 🎭 **Presentation**: Controllers, DTOs, HTTP Pipeline
- 💎 **Domain**: Models, Use Cases, Interfaces (Core)  
- 🔧 **Infrastructure**: Repositories, Providers, Configuration

📖 **Ver documentación completa**: [ARCHITECTURE.md](./ARCHITECTURE.md)

## 🚀 Quick Start

### Requisitos
- .NET 8.0 SDK
- Editor: Visual Studio 2022 / VS Code

### Ejecutar localmente
```bash
git clone https://github.com/CaroVisentin/MathRacerApi.git
cd MathRacerApi
dotnet run
```

🌐 **API disponible en**: `https://localhost:5001` (HTTPS) | `http://localhost:5000` (HTTP)

### 📚 Documentación API
- **Local**: https://localhost:5001/swagger
- **Producción**: https://mathracerapi.onrender.com/swagger

## 📋 Endpoints Activos

### ✅ Funcionales (Implementados)
- `GET /` - Redirige a Swagger Documentation
- `GET /api/info` - Información general de la API
- `GET /health` - Health Check de la aplicación
### 📋 Game Endpoints (Próximamente)
- `POST /api/games` - Crear nueva partida
- `GET /api/games/{id}` - Obtener estado de partida  
- `POST /api/games/{id}/join` - Unirse a partida
- `/hub/game` - SignalR Hub para tiempo real

## 🛠️ Desarrollo

### Estructura del Código
```
MathRacerApi/
├── 🎭 Presentation/     # Controllers, DTOs, HTTP Config
├── 💎 Domain/           # Use Cases, Models, Interfaces  
└── 🔧 Infrastructure/   # Repositories, Providers, DI
```

### Comandos Útiles
```bash
# Compilar
dotnet build

# Ejecutar en desarrollo
dotnet run

# Ejecutar tests (cuando se agreguen)
dotnet test

# Docker
docker build -t mathracer-api .
docker run -p 5000:8080 mathracer-api
```

## 🔄 CI/CD & Deployment

### GitHub Actions
- ✅ Build y test automático en PRs
- ✅ Deploy automático a producción desde `main`
- ✅ Verificación de código y arquitectura

### Deployment
- **Plataforma**: Render (Docker)
- **URL Producción**: https://mathracerapi.onrender.com
- **Auto-deploy**: Push a branch `main`

## � Roadmap

### ✅ Fase 1: Base API (Completado)
- [x] Clean Architecture implementada
- [x] Health checks y monitoring
- [x] Swagger documentation  
- [x] CI/CD pipeline
- [x] Docker deployment

### 🚧 Fase 2: Core Game Features (En Desarrollo)
- [ ] Autenticación de usuarios
- [ ] Sistema de partidas
- [ ] SignalR para tiempo real
- [ ] Persistencia de datos
- [ ] Gestión de salas de juego

### 📋 Próximas Features
### 🚧 Fase 3: Advanced Features (Planeado)
- [ ] Base de datos con Entity Framework
- [ ] Autenticación y autorización
- [ ] Generación de problemas matemáticos
- [ ] Sistema de puntuación y rankings
- [ ] Estadísticas de jugadores

## 🧪 Testing & Quality

```bash
# Ejecutar tests (cuando se agreguen)
dotnet test

# Analizar cobertura
dotnet test --collect:"XPlat Code Coverage"

# Verificar build
dotnet build --configuration Release
```

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

## 📚 Recursos y Referencias

- 📖 **Arquitectura**: [ARCHITECTURE.md](./ARCHITECTURE.md) - Documentación completa
- 🌐 **API Docs**: [Swagger UI](https://localhost:5001/swagger) - Documentación interactiva
- 🚀 **CI/CD**: [CI-CD-README.md](./CI-CD-README.md) - Pipeline de despliegue
- 🏗️ **Clean Architecture**: [Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 🤝 Soporte

- **🐛 Reportar Bugs**: [GitHub Issues](https://github.com/CaroVisentin/MathRacerApi/issues)
- **💡 Sugerencias**: [GitHub Discussions](https://github.com/CaroVisentin/MathRacerApi/discussions)  
- **📖 Documentación**: Consultar archivos `.md` en el repositorio

---

*Desarrollado con ❤️ usando Clean Architecture y .NET 8*