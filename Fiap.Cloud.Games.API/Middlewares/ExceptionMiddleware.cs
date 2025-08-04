namespace Fiap.Cloud.Games.API.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ArgumentException ex)
        {
            // Tratamento de erros de domínio (validação)
            logger.LogWarning(ex, "Erro de validação: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status400BadRequest);
        }
        catch (Exception ex)
        {
            // Tratamento de erros inesperados
            logger.LogError(ex, "Erro interno no servidor.");
            await HandleExceptionAsync(context, ex.Message, StatusCodes.Status500InternalServerError);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, string mensagem, int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new { Mensagem = mensagem };
        return context.Response.WriteAsJsonAsync(response);
    }
}