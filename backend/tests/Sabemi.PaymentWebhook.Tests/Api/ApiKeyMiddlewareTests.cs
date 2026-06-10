using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Sabemi.PaymentWebhook.Api.Middleware;

namespace Sabemi.PaymentWebhook.Tests.Api;

public sealed class ApiKeyMiddlewareTests
{
    [Fact]
    public async Task ApiKeyMiddleware_MissingApiKey_ReturnsUnauthorized()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => nextCalled = true);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/webhooks/pagamento";

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.False(nextCalled);
    }

    [Fact]
    public async Task ApiKeyMiddleware_ValidApiKey_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => nextCalled = true);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/webhooks/pagamento";
        context.Request.Headers[ApiKeyMiddleware.HeaderName] = "test-key";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task ApiKeyMiddleware_ValidSignatureHeader_CallsNext()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(_ => nextCalled = true);
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Post;
        context.Request.Path = "/webhooks/pagamento";
        context.Request.Headers[ApiKeyMiddleware.SignatureHeaderName] = "test-key";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    private static ApiKeyMiddleware CreateMiddleware(Action<HttpContext> next)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey"] = "test-key"
            })
            .Build();

        return new ApiKeyMiddleware(
            context =>
            {
                next(context);
                return Task.CompletedTask;
            },
            configuration,
            NullLogger<ApiKeyMiddleware>.Instance);
    }
}
