using FluentValidation;
using Sabemi.PaymentWebhook.Application.DTOs;

namespace Sabemi.PaymentWebhook.Application.Validators;

public sealed class PaymentWebhookRequestValidator : AbstractValidator<PaymentWebhookRequest>
{
    public PaymentWebhookRequestValidator()
    {
        RuleFor(request => request.IdTransacao)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.IdContrato)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Valor)
            .GreaterThan(0);

        RuleFor(request => request.DataPagamento)
            .NotEmpty();

        RuleFor(request => request.Status)
            .NotEmpty()
            .Must(status => status is "SUCESSO" or "ERRO")
            .WithMessage("status must be SUCESSO or ERRO.");
    }
}
