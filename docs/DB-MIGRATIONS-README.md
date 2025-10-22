# MathRacer API - Configuración de Migraciones y Archivos .env

Este documento explica cómo configurar las **migraciones de base de datos** y los archivos **.env** en MathRacer API, siguiendo las mejores prácticas de Clean Architecture y .NET 8.

---

## 1. Archivos .env para conexión a base de datos

Debes crear **dos archivos .env** en el proyecto `MathRacerAPI.Presentation` (al mismo nivel que `appsettings.json`):

- `.env.development`
- `.env.production`

Estos archivos almacenan la cadena de conexión a la base de datos para los entornos de desarrollo y producción.

### Ejemplo de contenido para `.env.development`:

#### Si usas usuario y contraseña:
DB_CONNECTION="Server=NombreDelServidor;Database=NombreDeLaBaseDeDatos;User Id=TuUsuario;Password=TuPassword;TrustServerCertificate=True;"

#### Si usas autenticación de Windows:
DB_CONNECTION="Server=NombreDelServidor;Database=NombreDeLaBaseDeDatos;Trusted_Connection=True;TrustServerCertificate=True;"


> **Importante:**  
> Completa los valores con tus datos personales de conexión.  
> El archivo `.env.production` debe quedar vacío hasta que tengas una base de datos de producción configurada.

---

## 2. Configuración de migraciones con Entity Framework Core

Las migraciones permiten crear y actualizar la estructura de la base de datos según los modelos definidos en el proyecto.

### Pasos para ejecutar migraciones:

1. **Verifica que los archivos `.env` estén correctamente configurados** con la cadena de conexión.

2. **Abre la consola de administración de paquetes NuGet** en Visual Studio:
   - Menú: `Herramientas` → `Administrador de paquetes NuGet` → `Consola del administrador de paquetes NuGet`

3. **Selecciona el proyecto `MathRacerAPI.Infrastructure`** en el menú desplegable de la consola.

4. **Ejecuta los siguientes comandos, uno a la vez:**
Add-Migration InitialCreate
   Update-Database

- `Add-Migration InitialCreate`: Crea una migración con los cambios iniciales del modelo.
- `Update-Database`: Aplica la migración y crea las tablas en la base de datos.

> **Recomendaciones:**
> - Espera a que cada comando finalice antes de ejecutar el siguiente.
> - Si modificas los modelos, repite el proceso con un nombre de migración descriptivo.

---

## 3. Buenas prácticas y notas adicionales

- **No subas tus archivos `.env` con credenciales reales al repositorio.**  
Añade `.env.*` al archivo `.gitignore` si no está ya incluido.

- **El archivo `.env.production` debe completarse solo cuando la base de datos de producción esté disponible.**

- **Las migraciones deben ejecutarse siempre sobre una base de datos vacía o controlada.**

- **Si tienes problemas de conexión, revisa la cadena en el archivo `.env` y la configuración de tu servidor SQL.**

---

## 4. Ejemplo de estructura de archivos
src/
├── MathRacerAPI.Presentation/
│   ├── appsettings.json
│   ├── .env.development
│   └── .env.production
├── MathRacerAPI.Infrastructure/
│   └── Migrations/
│       ├── 20251019232335_Refactor.cs
│       ├── MathiRacerDbContextModelSnapshot.cs
│       └── ...
└── MathRacerAPI.Domain/

> **Importante:**
> - La carpeta Migrations se generará de forma automática al ejecutar los comandos previos.

---

## 5. Próximos pasos

1. Configura los archivos `.env` con tu cadena de conexión.
2. Ejecuta las migraciones siguiendo los pasos indicados.
3. Verifica que la base de datos se haya creado correctamente.
4. Completa el archivo `.env.production` cuando tengas la base de datos de producción.

---