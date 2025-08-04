using Microsoft.OpenApi.Models;

namespace Fiap.Cloud.Games.API.Extensions;

public static class RedocExtensions
{
    public static IApplicationBuilder UseRedocConfiguration(this IApplicationBuilder app)
    {
        app.UseReDoc(c =>
        {
            c.RoutePrefix = "docs"; // Exemplo: abrirá em https://localhost:xxxx/docs
            c.SpecUrl = "/swagger/v1/swagger.json"; // Swagger gerado
            c.DocumentTitle = "Documentação da API - Fiap.Cloud.Games";
            c.EnableUntrustedSpec(); // Permitir abrir mesmo sem HTTPS
            c.ExpandResponses("200,201"); // Abrir seções de resposta 200 e 201
        });

        return app;
    }
}
