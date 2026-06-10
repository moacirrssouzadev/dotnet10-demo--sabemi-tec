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
        var signatureSecret = _configuration["SignatureSecret"] ?? "0b1e0099db2325c261e9d9689545bd0c0d915e6ed569d28f97cdd8bdb8ee737e";
        var receivedApiKey = context.Request.Headers[HeaderName].FirstOrDefault();
        var receivedSignature = context.Request.Headers[SignatureHeaderName].FirstOrDefault();

        bool isValid = false;

        if (!string.IsNullOrWhiteSpace(receivedApiKey))
        {
            isValid = SecureEquals(receivedApiKey, expectedApiKey);
        }
        else if (!string.IsNullOrWhiteSpace(receivedSignature))
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var rawPayload = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            var computedSignature = ComputeHmacSha256Signature(rawPayload, signatureSecret);
            isValid = SecureEquals(receivedSignature, computedSignature);
        }

        if (!isValid)
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

    private static string ComputeHmacSha256Signature(string payload, string secret)
    {
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(secretBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private static bool SecureEquals(string received, string expected)
    {
        var receivedBytes = Encoding.UTF8.GetBytes(received);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        return receivedBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(receivedBytes, expectedBytes);
    }
}
