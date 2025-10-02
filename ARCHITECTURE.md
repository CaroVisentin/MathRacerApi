# MathRacer API - Arquitectura

## 🏗️ Arquitectura General

Este proyecto implementa una **arquitectura en capas** basada en los principios de Clean Architecture, diseñada para separar responsabilidades y facilitar el mantenimiento.

## 📊 Capas de la Arquitectura

La aplicación está organizada en **4 capas principales**:

### **1. Capa de Presentación (Presentation Layer)**
- **Carpeta**: `Controllers/`
- **Responsabilidad**: Manejo de requests HTTP y responses
- **Componentes**: API Controllers
- **Dependencias**: → Services

### **2. Capa de Servicios (Business Layer)**  
- **Carpeta**: `Services/`
- **Responsabilidad**: Lógica de negocio y reglas de dominio
- **Componentes**: Services e interfaces
- **Dependencias**: → Repositories (futuro), Models

### **3. Capa de Acceso a Datos (Data Access Layer)**
- **Carpeta**: `Repositories/` (preparada para futuro)
- **Responsabilidad**: Comunicación con base de datos y APIs externas
- **Componentes**: Repositories e interfaces
- **Dependencias**: → Entities

### **4. Capa de Dominio (Domain Layer)**
- **Carpeta**: `Entities/` (preparada para futuro)
- **Responsabilidad**: Entidades de negocio y modelos de dominio
- **Componentes**: Entidades, Value Objects
- **Dependencias**: Ninguna (núcleo independiente)

### **Capas de Soporte**
- **`Models/`**: DTOs para transferencia de datos entre capas
- **`Extensions/`**: Configuración modular y extension methods
- **`Hubs/`**: Comunicación en tiempo real (SignalR - futuro)

---

## 📁 Estructura del Proyecto

```
MathRacerApi/
├── Controllers/           # API Controllers - Manejo de HTTP requests
├── Services/             # Lógica de negocio - Core business logic  
├── Models/               # DTOs y ViewModels - Data Transfer Objects
├── Repositories/         # Acceso a datos - Data access layer (futuro)
├── Entities/             # Entidades de dominio - Domain models (futuro)
├── Hubs/                 # SignalR Hubs - Real-time communication (futuro)
├── Extensions/           # Extension methods - Configuración modular
├── Middleware/           # Custom middleware (futuro)
├── Configuration/        # Configuraciones específicas (futuro)
└── Program.cs           # Entry point y configuración
```

## 🔄 Flujo de Datos

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│  Controller │───▶│   Service   │───▶│ Repository  │───▶│  Database   │
│(Presentation)│    │ (Business)  │    │(Data Access)│    │   (Data)    │
└─────────────┘    └─────────────┘    └─────────────┘    └─────────────┘
       ▲                   ▲                   ▲                   
       │                   │                   │                   
   HTTP Request        Business Logic     Data Access         
       │                   │                   │                   
       ▼                   ▼                   ▼                   
┌─────────────┐    ┌─────────────┐    ┌─────────────┐              
│     DTO     │◀───│   Models    │◀───│  Entities   │              
│ (Response)  │    │             │    │  (Domain)   │              
└─────────────┘    └─────────────┘    └─────────────┘              
```

### Principios del Flujo
1. **Requests**: Entran por Controllers y se delegan a Services
2. **Business Logic**: Se procesa en Services usando Models
3. **Data Access**: Services comunican con Repositories para datos
4. **Response**: Se retorna a través de DTOs sin exponer entidades internas

## 📋 Componentes Implementados

### **Controllers (Capa de Presentación)**
```csharp
InfoController          // GET /api/info - Información de la API
HealthController         // GET /health - Health checks
WeatherForecastController // GET /WeatherForecast - Ejemplo existente
```

### **Services (Capa de Negocio)**
```csharp
IApiInfoService / ApiInfoService     // Lógica de información de API
IHealthService / HealthService       // Lógica de health checks
```

### **Models (DTOs)**
```csharp  
ApiInfoResponse          // DTO para respuesta de información
HealthCheckResponse      // DTO para respuesta de health check
ApiEndpoints            // DTO para endpoints disponibles
```

### **Extensions (Configuración)**
```csharp
ServiceExtensions        // Configuración de Dependency Injection
ApplicationExtensions    // Configuración del pipeline de middleware
```

### **Dependency Injection**
Todos los services se registran usando interfaces para facilitar testing:
```csharp
services.AddScoped<IApiInfoService, ApiInfoService>();
services.AddScoped<IHealthService, HealthService>();
```

## 🚀 Endpoints Disponibles

| Método | Endpoint | Descripción | Capa | Controller |
|--------|----------|-------------|------|------------|
| GET | `/` | Redirige a Swagger | Middleware | - |
| GET | `/swagger` | Documentación API | Middleware | - |
| GET | `/health` | Health check | Presentación | HealthController |
| GET | `/api/info` | Información de la API | Presentación | InfoController |
| GET | `/WeatherForecast` | Datos meteorológicos | Presentación | WeatherForecastController |

## 🔧 Configuración de la Aplicación

### **Program.cs - Punto de Entrada**
```csharp
// Registro de servicios por capas
builder.Services.AddControllers();              // Capa Presentación
builder.Services.AddApplicationServices();      // Capa Servicios  
builder.Services.AddSwaggerDocumentation();     // Configuración
builder.Services.AddHealthCheckServices();      // Health Checks

// Pipeline de middleware
app.UseSwaggerDocumentation();    // Documentación
app.UseHttpsRedirection();        // Seguridad
app.UseAuthorization();           // Autorización
app.MapHealthChecks("/health");   // Health endpoint
app.UseCustomEndpoints();         // Endpoints personalizados
app.MapControllers();             // Controllers de la API
```

### **Inversión de Dependencias**
```csharp
// Interfaces definen contratos
public interface IApiInfoService { ... }

// Implementaciones concretas
public class ApiInfoService : IApiInfoService { ... }

// Registro en DI Container
services.AddScoped<IApiInfoService, ApiInfoService>();

// Inyección en Controllers
public InfoController(IApiInfoService apiInfoService) { ... }
```

### **Swagger con Documentación XML**
- Generación automática de documentación desde comentarios `///`
- Configurado para todos los ambientes
- Incluye información detallada de cada endpoint