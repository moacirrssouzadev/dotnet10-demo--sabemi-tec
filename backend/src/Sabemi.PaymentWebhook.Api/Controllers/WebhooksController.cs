using Microsoft.AspNetCore.Mvc;
using Sabemi.PaymentWebhook.Application.DTOs;
using Sabemi.PaymentWebhook.Application.UseCases;

namespace Sabemi.PaymentWebhook.Api.Controllers;

[ApiController]
[Route("webhooks")]
public sealed class WebhooksController : ControllerBase
{
    private readonly ReceivePaymentWebhookUseCase _receivePaymentWebhook;

    public WebhooksController(ReceivePaymentWebhookUseCase receivePaymentWebhook)
    {
        _receivePaymentWebhook = receivePaymentWebhook;
    }

    [HttpPost("pagamento")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ReceivePaymentWebhook([FromBody] PaymentWebhookRequest request, CancellationToken cancellationToken)
    {
        Request.Body.Position = 0;
        using var reader = new StreamReader(Request.Body);
        var rawPayload = await reader.ReadToEndAsync(cancellationToken);

        var result = await _receivePaymentWebhook.ExecuteAsync(request, rawPayload, cancellationToken);

        return result.Outcome switch
        {
            ReceiveWebhookOutcome.Accepted => Accepted(new { id = result.EventId, message = result.Message }),
            ReceiveWebhookOutcome.Duplicate => Ok(new { id = result.EventId, message = result.Message }),
            ReceiveWebhookOutcome.Invalid => BadRequest(new { id = result.EventId, message = result.Message }),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }
}
