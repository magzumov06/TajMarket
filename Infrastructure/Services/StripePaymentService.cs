using System.Net;
using Domain.Responses;
using Infrastructure.FileStorage;
using Infrastructure.Interfaces;
using Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Infrastructure.Services;

public class StripePaymentService(IOptions<StripeSetting> stripeSettings) : IStripePaymentService
{
    private readonly StripeSetting _stripeSettings = stripeSettings.Value;

    public async Task<Response<StripePaymentIntentResponse>> CreatePaymentIntentAsync(
        decimal amount, 
        string orderId, 
        string description)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = "usd",
                PaymentMethodTypes = new List<string> { "card" },
                Metadata = new Dictionary<string, string>
                {
                    { "OrderId", orderId },
                    { "Description", description }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return new Response<StripePaymentIntentResponse>(new StripePaymentIntentResponse
            {
                Id = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = amount,
                Status = paymentIntent.Status
            });
        }
        catch (StripeException e)
        {
            return new Response<StripePaymentIntentResponse>(
                HttpStatusCode.BadRequest,
                $"Stripe Error: {e.Message}");
        }
        catch (Exception e)
        {
            return new Response<StripePaymentIntentResponse>(
                HttpStatusCode.InternalServerError,
                $"Error creating payment intent: {e.Message}");
        }
    }

    public async Task<Response<bool>> ConfirmPaymentIntentAsync(string paymentIntentId, string paymentMethodId)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new PaymentIntentConfirmOptions
            {
                PaymentMethod = paymentMethodId,
                ReturnUrl = "https://yourapp.com/payment-success" // Update with your actual URL
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.ConfirmAsync(paymentIntentId, options);

            if (paymentIntent.Status == "succeeded" || paymentIntent.Status == "processing")
            {
                return new Response<bool>(true);
            }

            return new Response<bool>(
                HttpStatusCode.BadRequest,
                $"Payment failed: {paymentIntent.Status}");
        }
        catch (StripeException e)
        {
            return new Response<bool>(
                HttpStatusCode.BadRequest,
                $"Stripe Error: {e.Message}");
        }
        catch (Exception e)
        {
            return new Response<bool>(
                HttpStatusCode.InternalServerError,
                $"Error confirming payment: {e.Message}");
        }
    }

    public async Task<Response<StripePaymentIntentResponse>> GetPaymentIntentStatusAsync(string paymentIntentId)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId);

            return new Response<StripePaymentIntentResponse>(new StripePaymentIntentResponse
            {
                Id = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Amount = paymentIntent.Amount / 100m,
                Status = paymentIntent.Status,
                LastPaymentError = paymentIntent.LastPaymentError?.Message
            });
        }
        catch (StripeException e)
        {
            return new Response<StripePaymentIntentResponse>(
                HttpStatusCode.BadRequest,
                $"Stripe Error: {e.Message}");
        }
        catch (Exception e)
        {
            return new Response<StripePaymentIntentResponse>(
                HttpStatusCode.InternalServerError,
                $"Error retrieving payment status: {e.Message}");
        }
    }

    public async Task<Response<bool>> RefundPaymentAsync(string paymentIntentId, decimal? amount = null)
    {
        try
        {
            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;

            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = amount.HasValue ? (long)(amount.Value * 100) : null
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            return refund.Status == "succeeded"
                ? new Response<bool>(true)
                : new Response<bool>(HttpStatusCode.BadRequest, "Refund failed");
        }
        catch (StripeException e)
        {
            return new Response<bool>(
                HttpStatusCode.BadRequest,
                $"Stripe Error: {e.Message}");
        }
        catch (Exception e)
        {
            return new Response<bool>(
                HttpStatusCode.InternalServerError,
                $"Error processing refund: {e.Message}");
        }
    }
}
