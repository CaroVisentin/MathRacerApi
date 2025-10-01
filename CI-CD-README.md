# MathRacer API - CI/CD Setup

Este documento describe la configuración de CI para MathRacer API usando GitHub Actions y despliegue manual en Render con Docker.

## Configuración de CI

### 1. GitHub Actions (CI Pipeline)

El pipeline de CI se ejecuta automáticamente en:
- Push a las ramas `main` y `develop`
- Pull requests hacia `main` y `develop`

#### Etapas del pipeline:

1. **Build**: Restaura dependencias y compila la aplicación
2. **Test**: Ejecuta las pruebas unitarias
3. **Docker Test**: Verifica que la imagen Docker se puede construir correctamente

### 2. Configuración de Render (Despliegue Manual)

Para configurar el despliegue en Render:

1. Crear un nuevo **Web Service** en Render
2. Conectar con tu repositorio de GitHub
3. Configurar las siguientes opciones:
   - **Environment**: Docker
   - **Branch**: main (para auto-deploy)
   - **Build Command**: (dejar vacío, se usa Dockerfile)
   - **Start Command**: (dejar vacío, se usa Dockerfile)
   - **Port**: 8080

**Render se encargará automáticamente del despliegue** cada vez que pushees cambios a la rama `main`.

### 3. Dockerfile

El Dockerfile usa multi-stage build para optimizar el tamaño de la imagen:
- **Build stage**: Usa SDK de .NET 8 para compilar la aplicación
- **Runtime stage**: Usa runtime de .NET 8 para ejecutar la aplicación

### 4. Configuración local

Para probar Docker localmente:

```bash
# Construir la imagen
docker build -t mathracer-api .

# Ejecutar el contenedor
docker run -p 8080:8080 mathracer-api
```

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