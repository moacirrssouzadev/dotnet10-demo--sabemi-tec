using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sabemi.PaymentWebhook.Application.DTOs;
using Sabemi.PaymentWebhook.Application.UseCases;
using Sabemi.PaymentWebhook.Application.Validators;

namespace Sabemi.PaymentWebhook.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<PaymentWebhookRequest>, PaymentWebhookRequestValidator>();
        services.AddScoped<ReceivePaymentWebhookUseCase>();
        services.AddScoped<ProcessPaymentWebhookUseCase>();
        services.AddScoped<ListWebhookEventsUseCase>();

        return services;
    }
}
