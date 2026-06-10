using Microsoft.AspNetCore.Mvc;
using Sabemi.PaymentWebhook.Application.DTOs;
using Sabemi.PaymentWebhook.Application.UseCases;

namespace Sabemi.PaymentWebhook.Api.Controllers;

[ApiController]
[Route("api/webhook-events")]
public sealed class WebhookEventsController : ControllerBase
{
    private readonly ListWebhookEventsUseCase _listWebhookEvents;

    public WebhookEventsController(ListWebhookEventsUseCase listWebhookEvents)
    {
        _listWebhookEvents = listWebhookEvents;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WebhookEventDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WebhookEventDto>>> List(
        [FromQuery] string? status,
        [FromQuery(Name = "idContrato")] string? contractId,
        CancellationToken cancellationToken)
    {
        var events = await _listWebhookEvents.ExecuteAsync(new WebhookEventQuery(status, contractId), cancellationToken);
        return Ok(events);
    }
}
