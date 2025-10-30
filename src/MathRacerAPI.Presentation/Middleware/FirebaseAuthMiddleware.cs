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
            // Solo validar si hay header Authorization
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    var idToken = authHeader.Substring("Bearer ".Length).Trim();
                    
                    // Validar token con Firebase (puede lanzar UnauthorizedAccessException)
                    var uid = await firebaseService.ValidateIdTokenAsync(idToken);
                    
                    if (!string.IsNullOrEmpty(uid))
                    {
                        // Agregar el UID validado al contexto HTTP
                        context.Items["FirebaseUid"] = uid;
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("El token de autenticación es inválido.");
                    }
                }
            }
            
            await _next(context);
        }
    }
}
