using System.Net;
using System.Text.Json;
using BarberShopAgenda.Domain.Interfaces;

namespace BarberShopAgenda.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado ao processar a requisição {Path}", context.Request.Path);
            await TratarExcecaoAsync(context, ex);
        }
    }

    private static async Task TratarExcecaoAsync(HttpContext context, Exception exception)
    {
        var (statusCode, mensagem) = exception switch
        {
            ConflitoHorarioException => (HttpStatusCode.Conflict, exception.Message),
            RegraNegocioException => (HttpStatusCode.BadRequest, exception.Message),
            EmailNaoConfirmadoException => (HttpStatusCode.Forbidden, exception.Message),
            KeyNotFoundException => (HttpStatusCode.NotFound, exception.Message),
            _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro interno ao processar a requisição.")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var resposta = JsonSerializer.Serialize(new { mensagem });
        await context.Response.WriteAsync(resposta);
    }
}
