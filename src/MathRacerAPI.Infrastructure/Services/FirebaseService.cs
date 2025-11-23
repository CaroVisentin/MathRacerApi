using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MathRacerAPI.Infrastructure.Services
{
    public class FirebaseService : Domain.Services.IFirebaseService
    {
        private static bool _initialized = false;

        public FirebaseService()
        {
            if (!_initialized)
            {
                AppOptions options;
                
                // 1. Intentar desde variable de entorno JSON (para Render, Heroku, etc.)
                var credJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
                if (!string.IsNullOrWhiteSpace(credJson))
                {
                    Console.WriteLine("✅ Firebase credentials cargadas desde variable de entorno FIREBASE_CREDENTIALS_JSON");
                    try
                    {
                        options = new AppOptions
                        {
                            Credential = GoogleCredential.FromJson(credJson)
                        };
                        FirebaseApp.Create(options);
                        _initialized = true;
                        return;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error al parsear FIREBASE_CREDENTIALS_JSON: {ex.Message}");
                        throw new InvalidOperationException("La variable FIREBASE_CREDENTIALS_JSON contiene JSON inválido", ex);
                    }
                }
                
                // 2. Intentar desde archivo (desarrollo local)
                var credPath = GetCredentialsPath();
                if (!string.IsNullOrWhiteSpace(credPath) && System.IO.File.Exists(credPath))
                {
                    Console.WriteLine($"✅ Firebase credentials encontradas en archivo: {credPath}");
                    options = new AppOptions
                    {
                        Credential = GoogleCredential.FromFile(credPath)
                    };
                    FirebaseApp.Create(options);
                    _initialized = true;
                    return;
                }
                
                // 3. Fallback a credenciales por defecto (Google Cloud)
                Console.WriteLine("⚠️ No se encontró FIREBASE_CREDENTIALS_JSON ni archivo. Intentando credenciales por defecto...");
                try
                {
                    options = new AppOptions
                    {
                        Credential = GoogleCredential.GetApplicationDefault()
                    };
                    FirebaseApp.Create(options);
                    _initialized = true;
                    Console.WriteLine("✅ Firebase inicializado con credenciales por defecto");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error al inicializar Firebase: {ex.Message}");
                    throw new InvalidOperationException(
                        "No se pudo inicializar Firebase. Configura la variable de entorno FIREBASE_CREDENTIALS_JSON " +
                        "con el contenido JSON de tus credenciales, o coloca el archivo firebase-credentials.json en la raíz del proyecto.",
                        ex
                    );
                }
            }
        }

        public async Task<string?> ValidateIdTokenAsync(string idToken)
        {
            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new UnauthorizedAccessException("El token de autenticación no fue proporcionado.");
            }

            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                return decodedToken?.Uid;
            }
            catch (FirebaseAuthException ex)
            {
                // Lanzar UnauthorizedAccessException con el mensaje original de Firebase
                throw new UnauthorizedAccessException($"Token de Firebase inválido: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Cualquier otro error de validación
                throw new UnauthorizedAccessException("Error al validar el token de autenticación.", ex);
            }
        }

        private string? GetCredentialsPath()
        {
            // 1. Intentar desde variable de entorno (prioridad máxima)
            var envPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
            if (!string.IsNullOrWhiteSpace(envPath) && System.IO.File.Exists(envPath))
            {
                return envPath;
            }

            // 2. Buscar en directorio base (bin/Debug/net8.0)
            var baseDir = AppContext.BaseDirectory;
            var basePath = System.IO.Path.Combine(baseDir, "firebase-credentials.json");
            if (System.IO.File.Exists(basePath))
            {
                return basePath;
            }

            // 3. Buscar en raíz del proyecto Presentation (subir 3 niveles)
            var presentationRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "..", "..", ".."));
            var presentationPath = System.IO.Path.Combine(presentationRoot, "firebase-credentials.json");
            if (System.IO.File.Exists(presentationPath))
            {
                return presentationPath;
            }

            // 4. Buscar en raíz del src (subir 4 niveles)
            var srcRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "..", "..", "..", ".."));
            var srcPath = System.IO.Path.Combine(srcRoot, "firebase-credentials.json");
            if (System.IO.File.Exists(srcPath))
            {
                return srcPath;
            }

            // 5. Buscar en raíz del repositorio (subir 5 niveles)
            var repoRoot = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "..", "..", "..", "..", ".."));
            var repoPath = System.IO.Path.Combine(repoRoot, "firebase-credentials.json");
            if (System.IO.File.Exists(repoPath))
            {
                return repoPath;
            }

            // 6. Último intento: Buscar desde directorio actual de trabajo
            var currentDir = Directory.GetCurrentDirectory();
            var currentPath = System.IO.Path.Combine(currentDir, "firebase-credentials.json");
            if (System.IO.File.Exists(currentPath))
            {
                return currentPath;
            }

            // Debug: Mostrar dónde se buscó (útil para diagnóstico)
            Console.WriteLine($"❌ firebase-credentials.json NO encontrado en:");
            Console.WriteLine($"   1. Variable entorno GOOGLE_APPLICATION_CREDENTIALS: {envPath ?? "(no definida)"}");
            Console.WriteLine($"   2. BaseDirectory: {basePath}");
            Console.WriteLine($"   3. Presentation root: {presentationPath}");
            Console.WriteLine($"   4. src root: {srcPath}");
            Console.WriteLine($"   5. Repository root: {repoPath}");
            Console.WriteLine($"   6. Current directory: {currentPath}");

            return null;
        }
    }
}
