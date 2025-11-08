namespace MathRacerAPI.Presentation.Configuration;

/// <summary>
/// Extensiones para configurar el pipeline de la aplicación
/// </summary>
public static class ApplicationExtensions
{
    /// <summary>
    /// Configura Swagger para todos los ambientes
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "MathRacer API v1.0.0");
            c.RoutePrefix = "swagger";
            c.DocumentTitle = "MathRacer API Documentation";
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.DefaultModelsExpandDepth(-1); // Ocultar modelos por defecto
            c.EnableValidator();
            c.EnableDeepLinking();
        });

        return app;
    }

    /// <summary>
    /// Configura los endpoints personalizados
    /// </summary>
    public static IApplicationBuilder UseCustomEndpoints(this IApplicationBuilder app)
    {
        var webApp = (WebApplication)app;

        // Root endpoint - redirect to Swagger
        webApp.MapGet("/", () => Results.Redirect("/swagger"))
               .ExcludeFromDescription();

        return app;
    }
}