# 🎯 Sistema de Matchmaking Basado en Ranking

## 📋 **Descripción**

El sistema de matchmaking permite emparejar jugadores basándose en sus puntos de ranking, creando partidas más equilibradas y competitivas.

## 🚀 **Funcionalidades Implementadas**

### 1. **Matchmaking por Puntos**
- **Tolerancia Adaptativa**: El rango de búsqueda se ajusta según el nivel del jugador:
  - **Principiante** (≤50 puntos): ±25 puntos
  - **Intermedio** (51-150 puntos): ±30 puntos
  - **Avanzado** (151-250 puntos): ±40 puntos
  - **Experto** (>250 puntos): ±50 puntos

### 2. **Compatibilidad con Sistema Existente**
- Se mantiene el método `FindMatch()` original para matchmaking **FIFO (First In, First Out)**
- Se añade `FindMatchWithMatchmaking()` para matchmaking **basado en ranking**
- Ambos sistemas coexisten sin interferir

### 3. **Diferencias entre Sistemas**
- **FIFO**: Empareja al primer jugador disponible, rápido pero puede ser desbalanceado
- **Ranking**: Empareja jugadores con habilidades similares, más lento pero equilibrado

## 🔧 **Uso del Sistema**

### **Parámetros**
- `playerUid`: UID único del jugador (ambos métodos obtienen nombre real de la BD)
  - **FindMatch**: FIFO (First In, First Out) - emparejamiento inmediato + nombre auténtico
  - **FindMatchWithMatchmaking**: Matchmaking por puntos de ranking + tolerancias + nombre auténtico

## 🏗️ **Arquitectura**

### **Casos de Uso**
- `FindMatchUseCase`: Matchmaking FIFO original
- `FindMatchWithMatchmakingUseCase`: Nuevo matchmaking por ranking

### **Cambios en Modelos**
- `Player`: Añadido campo `Uid` para tracking de jugadores

### **GameHub**
- `FindMatch()`: Método original
- `FindMatchWithMatchmaking()`: Nuevo método con parametros de ranking

## ⚙️ **Configuración**

El sistema se registra automáticamente en el contenedor de dependencias:

```csharp
services.AddScoped<FindMatchWithMatchmakingUseCase>();
```

## 📊 **Algoritmo de Matchmaking**

1. **Obtener Perfil**: Se busca el `PlayerProfile` usando el UID
2. **Calcular Tolerancia**: Se determina el rango basado en los puntos del jugador
3. **Buscar Partidas**: Se filtran partidas compatibles dentro del rango
4. **Verificar Compatibilidad**: Se comparan puntos entre jugadores
5. **Emparejar o Crear**: Se une a partida compatible o se crea nueva

## 🎮 **Sistema de Puntos (Recordatorio)**

- **Victoria Online**: +10 puntos
- **Derrota Online**: -5 puntos (mínimo 0)
- **Rango Típico**: 50-350 puntos

## 🔍 **Logging**

El sistema incluye logging detallado para monitorear:
- Inicio de matchmaking con UID
- Búsqueda de partidas compatibles
- Creación de nuevas partidas
- Emparejamiento exitoso

## 📈 **Beneficios**

✅ **Partidas Equilibradas**: Jugadores con habilidades similares
✅ **Experiencia Mejorada**: Menos partidas desbalanceadas  
✅ **Compatibilidad**: No afecta el sistema existente
✅ **Escalabilidad**: Tolerancia adaptativa según experiencia
✅ **Flexibilidad**: Dos opciones de matchmaking disponibles

## 🚧 **Próximas Mejoras**

- [ ] Métricas de tiempo de espera por rango
- [ ] Algoritmo dinámico de tolerancia basado en población
- [ ] Matchmaking por región geográfica
- [ ] Sistema de ranking por temporadas