using MathRacerAPI.Domain.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace MathRacerAPI.Presentation.Middleware
{
    /// <summary>
    /// Middleware que valida tokens de Firebase y agrega el UID al contexto HTTP
    /// </summary>
    public class FirebaseAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public FirebaseAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IFirebaseService firebaseService)
        {
            string? idToken = null;

            // Solo validar si hay header Authorization
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();

                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                
                {
                     idToken = authHeader.Substring("Bearer ".Length).Trim();
               
                }
            }

            //  Buscar token en query string (para SignalR)
            if (string.IsNullOrEmpty(idToken) && context.Request.Query.ContainsKey("access_token"))
            {
                idToken = context.Request.Query["access_token"].ToString();
            }
            if (context.Request.Path.StartsWithSegments("/gameHub"))
            {
                Console.WriteLine($"[Middleware] Hub request: {context.Request.Method} {context.Request.Path}{context.Request.QueryString}");
                foreach (var q in context.Request.Query)
                    Console.WriteLine($"[Middleware] Query {q.Key}={q.Value}");
            }

            //Validar token si existe
            if (!string.IsNullOrEmpty(idToken))
            {
                try
                {
                    var uid = await firebaseService.ValidateIdTokenAsync(idToken);

                    if (!string.IsNullOrEmpty(uid))
                    {
                        context.Items["FirebaseUid"] = uid;
                       
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error token: {ex.Message}");
                    // Para SignalR no fallar, para REST sí
                    if (!context.Request.Path.StartsWithSegments("/gameHub"))
                    {
                        throw new UnauthorizedAccessException("Token inválido.");
                    }
                }
            }
            else if (context.Request.Path.StartsWithSegments("/gameHub"))
            {
                Console.WriteLine("[Middleware] Hub connection without token");
            }



            await _next(context);
        }
    }
}
