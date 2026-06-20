using Microsoft.AspNetCore.Http;

namespace Platform.Serilog.Logging.Correlation;

public sealed class CorrelationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = CorrelationId.GetOrCreate(
            context.Request.Headers[CorrelationHeaders.CorrelationId].FirstOrDefault());

        string? useCase = context.Request.Headers[CorrelationHeaders.UseCase].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(useCase))
        {
            useCase = $"{context.Request.Method} {context.Request.Path}";
        }

        string? causationId = context.Request.Headers[CorrelationHeaders.CausationId].FirstOrDefault();

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationHeaders.CorrelationId] = correlationId;
            return Task.CompletedTask;
        });

        context.Items[nameof(CorrelationContext)] = new CorrelationContext(correlationId, useCase, causationId);

        using (CorrelationScope.Begin(correlationId, useCase, causationId))
        {
            await next(context);
        }
    }
}
