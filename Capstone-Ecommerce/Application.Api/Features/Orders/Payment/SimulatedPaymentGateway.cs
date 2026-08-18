namespace Application.Api.Features.Orders.Payment;

/// <summary>
/// Stands in for a real external payment provider - deliberately flaky
/// (simulated latency + a real failure rate) so Retry/CircuitBreaker/
/// Fallback (registered as the "payment-gateway" resilience pipeline in
/// Program.cs) have something genuine to protect against. None of those
/// patterns mean anything wrapped around a call that never fails.
/// </summary>
public class SimulatedPaymentGateway : IPaymentGateway
{
    private static readonly Random Rng = new();
    private const double FailureRate = 0.35;

    public async Task<PaymentChargeResult> ChargeAsync(decimal amount, CancellationToken ct = default)
    {
        await Task.Delay(Rng.Next(50, 300), ct); // simulated network latency

        if (Rng.NextDouble() < FailureRate)
            throw new PaymentGatewayException("Simulated payment gateway timeout.");

        return new PaymentChargeResult(true, $"txn_{Guid.NewGuid():N}");
    }
}
