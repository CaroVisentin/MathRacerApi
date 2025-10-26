# MathRacer API - CI/CD Setup

Este documento describe la configuración de CI/CD para MathRacer API usando GitHub Actions con Clean Architecture y despliegue en Render con Docker.

## Arquitectura del Proyecto

El proyecto sigue **Clean Architecture** con las siguientes capas:

```
src/
├── MathRacerAPI.Domain/         # Lógica de negocio pura
├── MathRacerAPI.Infrastructure/ # Implementaciones concretas
└── MathRacerAPI.Presentation/   # API Controllers, SignalR Hubs
tests/
└── MathRacerAPI.Tests/         # Tests unitarios con xUnit, Moq
```

## Configuración de CI

### 1. GitHub Actions (CI Pipeline)

El pipeline de CI se ejecuta automáticamente en:
- Push a las ramas `main`, `develop` y `feature/*`
- Pull requests hacia `main` y `develop`

#### Etapas del pipeline:

1. **Setup**: Configura .NET 8.0 y cache de NuGet
2. **Restore**: Restaura dependencias del archivo .sln
3. **Build por capas**: Compila cada proyecto en orden de dependencias
   - Domain (sin dependencias externas)
   - Infrastructure (depende de Domain)
   - Presentation (depende de Infrastructure y Domain)
   - Tests (depende de todos los proyectos)
4. **Test**: Ejecuta 22+ tests unitarios con cobertura de código
5. **Artifacts**: Sube resultados de tests y reportes de cobertura
6. **Docker Test**: Verifica que la imagen Docker se puede construir
7. **Architecture Validation**: Valida la estructura Clean Architecture

#### Comandos de CI (locales):

```bash
# Restaurar dependencias
dotnet restore MathRacerAPI.sln

# Compilar por capas
dotnet build src/MathRacerAPI.Domain/MathRacerAPI.Domain.csproj --no-restore --configuration Release
dotnet build src/MathRacerAPI.Infrastructure/MathRacerAPI.Infrastructure.csproj --no-restore --configuration Release
dotnet build src/MathRacerAPI.Presentation/MathRacerAPI.Presentation.csproj --no-restore --configuration Release
dotnet build tests/MathRacerAPI.Tests/MathRacerAPI.Tests.csproj --no-restore --configuration Release

# Ejecutar tests
dotnet test tests/MathRacerAPI.Tests/MathRacerAPI.Tests.csproj --no-build --configuration Release --verbosity normal

# Generar cobertura de código
dotnet test tests/MathRacerAPI.Tests/MathRacerAPI.Tests.csproj --configuration Release --collect:"XPlat Code Coverage"
```

### 2. Configuración de Render (Despliegue Automático)

Para configurar el despliegue en Render:

1. Crear un nuevo **Web Service** en Render
2. Conectar con tu repositorio de GitHub
3. Configurar las siguientes opciones:
   - **Environment**: Docker
   - **Branch**: main (para auto-deploy)
   - **Build Command**: (dejar vacío, se usa Dockerfile)
   - **Start Command**: (dejar vacío, se usa Dockerfile)
   - **Port**: 5153 (puerto configurado en la aplicación)

**Render se encargará automáticamente del despliegue** cada vez que los tests pasen en la rama `main`.

### 3. Dockerfile

El Dockerfile está actualizado para Clean Architecture:
- **Build stage**: Copia y compila todos los proyectos de src/
- **Test stage**: Ejecuta tests para validar antes del deploy
- **Runtime stage**: Solo incluye los binarios necesarios para producción

### 4. Desarrollo Local

#### Desarrollo:
```bash
# Ejecutar la aplicación
dotnet run --project src/MathRacerAPI.Presentation/

# Ejecutar tests en modo watch
dotnet test --watch

# Ejecutar tests con cobertura
dotnet test --collect:"XPlat Code Coverage"
```

#### Docker:
```bash
# Construir la imagen
docker build -t mathracer-api .

# Ejecutar el contenedor
docker run -p 5153:5153 mathracer-api

# Acceder a la API
# HTTP: http://localhost:5153
# SignalR Hub: ws://localhost:5153/gamehub
```

### 5. Testing

El proyecto incluye **22+ tests unitarios** que cubren:
- **GameLogicService**: Lógica de juego, condiciones de victoria, penalizaciones
- **SubmitAnswerUseCase**: Casos de uso de respuestas con validaciones
- **Mocking**: Usando Moq para interfaces y dependencias
- **Assertions**: FluentAssertions para tests más legibles

#### Coverage:
- **Domain Layer**: Lógica de negocio 100% cubierta
- **Use Cases**: Flujos principales cubiertos
- **Services**: Implementaciones concretas validadas

### 5. Configuración de producción

- La aplicación se ejecuta en el puerto 8080
- Usa el archivo `appsettings.Production.json` para configuración específica de producción
- Logs configurados para nivel Information

## Próximos pasos

1. Mergear esta rama a `main`
2. Configurar el servicio en Render conectando tu repositorio
3. El CI verificará automáticamente cada push y PR
4. Render desplegará automáticamente cuando pushees a `main`

## Notas importantes

- El CI verifica build, tests y que Docker funcione correctamente
- No se requieren secrets adicionales en GitHub
- Render maneja el despliegue automáticamente desde su plataforma
- Las pruebas deben pasar para que el CI sea exitoso