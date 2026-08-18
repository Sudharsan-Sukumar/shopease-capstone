namespace Application.Api.Features.Orders.Payment;

public record PaymentChargeResult(bool Succeeded, string? TransactionId);

public class PaymentGatewayException(string message) : Exception(message);

public interface IPaymentGateway
{
    Task<PaymentChargeResult> ChargeAsync(decimal amount, CancellationToken ct = default);
}
