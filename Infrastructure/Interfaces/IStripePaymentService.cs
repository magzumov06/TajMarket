using Domain.Responses;

namespace Infrastructure.Interfaces;

public interface IStripePaymentService
{
    /// <summary>
    /// Creates a Stripe payment intent for the given amount
    /// </summary>
    Task<Response<StripePaymentIntentResponse>> CreatePaymentIntentAsync(decimal amount, string orderId, string description);

    /// <summary>
    /// Confirms a Stripe payment intent using a payment method
    /// </summary>
    Task<Response<bool>> ConfirmPaymentIntentAsync(string paymentIntentId, string paymentMethodId);

    /// <summary>
    /// Retrieves the status of a Stripe payment intent
    /// </summary>
    Task<Response<StripePaymentIntentResponse>> GetPaymentIntentStatusAsync(string paymentIntentId);

    /// <summary>
    /// Refunds a payment by transaction ID
    /// </summary>
    Task<Response<bool>> RefundPaymentAsync(string paymentIntentId, decimal? amount = null);
}

public class StripePaymentIntentResponse
{
    public string Id { get; set; }
    public string ClientSecret { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }
    public string? LastPaymentError { get; set; }
}
