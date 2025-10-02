# MathRacer API

API para el juego MathRacer - Competencias matemáticas en tiempo real con soporte multijugador.

## 🚀 Quick Start

### Requisitos
- .NET 8.0
- Docker (para despliegue)

### Ejecutar localmente
```bash
git clone https://github.com/CaroVisentin/MathRacerApi.git
cd MathRacerApi
dotnet run
```

La API estará disponible en: `http://localhost:5152`

### Swagger Documentation
- **Local**: http://localhost:5152/swagger
- **Producción**: https://mathracerapi.onrender.com/swagger

---

## 📋 Endpoints Disponibles

### API Information
- `GET /` - Redirige a Swagger
- `GET /api/info` - Información general de la API
- `GET /swagger` - Documentación interactiva

### Monitoring
- `GET /health` - Health check de la aplicación

### Game (Coming Soon)
- Endpoints de juego y multijugador (próxima implementación)

---

## 🏗️ Arquitectura

Este proyecto utiliza **Clean Architecture** con las siguientes capas:

```
Controllers → Services → Repositories → Entities
     ↓           ↓           ↓           ↓
   HTTP      Business    Data Access   Domain
  Handling    Logic      Layer        Models
```

### Estructura de Carpetas
- **`Controllers/`** - API Controllers
- **`Services/`** - Lógica de negocio  
- **`Models/`** - DTOs y ViewModels
- **`Extensions/`** - Configuración modular
- **`Repositories/`** - Acceso a datos (futuro)
- **`Entities/`** - Entidades de dominio (futuro)
- **`Hubs/`** - SignalR Hubs (futuro)

📖 **Documentación completa**: [ARCHITECTURE.md](./ARCHITECTURE.md)

---

## 🔄 CI/CD Pipeline

### GitHub Actions
- ✅ Build y test automático
- ✅ Verificación de código en PRs
- ✅ Deploy automático a producción

### Deployment
- **Plataforma**: Render
- **Container**: Docker
- **Auto-deploy**: Push a `main`

### URLs
- **Production**: https://mathracerapi.onrender.com
- **Swagger Docs**: https://mathracerapi.onrender.com/swagger

---

## 🎮 Roadmap de Funcionalidades

### ✅ Completado
- [x] API base con Clean Architecture
- [x] Health checks y monitoring  
- [x] Swagger documentation
- [x] CI/CD pipeline
- [x] Docker deployment

### 🚧 En Desarrollo
- [ ] SignalR para multijugador en tiempo real
- [ ] Sistema de matchmaking
- [ ] Gestión de salas de juego

### 📋 Próximas Features
- [ ] Autenticación con Firebase
- [ ] Base de datos (Entity Framework)
- [ ] Generación de problemas matemáticos
- [ ] Sistema de puntuación
- [ ] Estadísticas de jugadores

---

## 🧪 Testing

### Ejecutar Tests
```bash
dotnet test
```

### Coverage (Futuro)
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🔧 Desarrollo

### Crear nueva feature
```bash
git checkout develop
git pull
git checkout -b feature/nueva-funcionalidad
```

### Convenciones
- **Branches**: `feature/`, `bugfix/`, `hotfix/`
- **Commits**: Conventional commits
- **PRs**: Siempre hacia `develop`

### Estructura de Servicios
```csharp
// Interface
public interface IGameService
{
    Task<GameResponse> CreateGameAsync(CreateGameRequest request);
}

// Implementation
public class GameService : IGameService
{
    public async Task<GameResponse> CreateGameAsync(CreateGameRequest request)
    {
        // Implementación
    }
}

// Registration in ServiceExtensions.cs
services.AddScoped<IGameService, GameService>();
```

---

## 📱 Clients

Esta API será consumida por:
- **Web App**: React frontend
- **Mobile App**: Android con Kotlin
- **Game Dashboard**: Admin panel

---

## 🔒 Configuración de Seguridad

### Variables de Entorno (Producción)
```env
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

### CORS (Configurado para desarrollo)
```csharp
// En producción, configurar origins específicos
app.UseCors(policy => policy
    .WithOrigins("https://mathracer-web.com")
    .AllowAnyMethod()
    .AllowAnyHeader());
```

---

## 📊 Monitoring

### Health Checks
- **Endpoint**: `/health`
- **Información**: Estado, memoria, uptime

### Metrics (Futuro)
- Response times
- Active connections
- Game sessions
- Error rates

---

## 🤝 Contribución

1. Fork del repositorio
2. Crear rama feature desde `develop`
3. Implementar cambios siguiendo la arquitectura
4. Agregar tests si es necesario
5. Crear PR hacia `develop`

### Code Style
- Seguir convenciones de C#
- Comentarios XML en APIs públicas
- Mantener separación de responsabilidades

---

## 📞 Soporte

- **Issues**: GitHub Issues
- **Documentación**: [ARCHITECTURE.md](./ARCHITECTURE.md)
- **API Docs**: Swagger UI

---

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Ver [LICENSE](./LICENSE) para más detalles.