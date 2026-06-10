using System.Security.Cryptography;
using System.Text;

namespace Sabemi.PaymentWebhook.Api.Middleware;

public sealed class ApiKeyMiddleware
{
    public const string HeaderName = "X-API-KEY";
    public const string SignatureHeaderName = "X-Signature";

    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!RequiresApiKey(context))
        {
            await _next(context);
            return;
        }

        var expectedApiKey = _configuration["ApiKey"] ?? "sabemi-dev-api-key";
        var receivedApiKey = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? context.Request.Headers[SignatureHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(receivedApiKey) || !SecureEquals(receivedApiKey, expectedApiKey))
        {
            _logger.LogWarning("Unauthorized webhook request from {RemoteIpAddress}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Invalid or missing API key or signature." });
            return;
        }

        await _next(context);
    }

    private static bool RequiresApiKey(HttpContext context)
    {
        return HttpMethods.IsPost(context.Request.Method)
            && context.Request.Path.Equals("/webhooks/pagamento", StringComparison.OrdinalIgnoreCase);
    }

    private static bool SecureEquals(string received, string expected)
    {
        var receivedBytes = Encoding.UTF8.GetBytes(received);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return receivedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(receivedBytes, expectedBytes);
    }
}
