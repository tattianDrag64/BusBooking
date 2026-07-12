using BusBooking.Entity;
using BusBooking.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace BusBooking.Services
{
    public class PaymentService(IConfiguration configuration) : IPaymentService
    {
        private readonly string _successUrl = configuration["Stripe:SuccessUrl"]
                ?? throw new InvalidOperationException("Stripe:SuccessUrl is not configured.");
        private readonly string _cancelUrl = configuration["Stripe:CancelUrl"]
                ?? throw new InvalidOperationException("Stripe:CancelUrl is not configured.");
        private readonly string _webhookSecret = configuration["Stripe:WebhookSecret"]
                ?? throw new InvalidOperationException("Stripe:WebhookSecret is not configured.");

        public Session CreateCheckoutSession(Order order)
        {
            var options = new SessionCreateOptions
            {
                Mode = "payment",
                PaymentMethodTypes = ["card"],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "eur",
                            UnitAmount = (long)(order.TotalPrice * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Pometco ticket — order {order.OrderCode}",
                            },
                        },
                    },
                ],
                ClientReferenceId = order.Id.ToString(),
                SuccessUrl = $"{_successUrl}?orderId={order.Id}",
                CancelUrl = $"{_cancelUrl}?orderId={order.Id}",
            };

            var service = new SessionService();
            return service.Create(options);
        }

        public Stripe.Event ConstructWebhookEvent(string requestBody, string stripeSignature)
        {
            return EventUtility.ConstructEvent(requestBody, stripeSignature, _webhookSecret);
        }

        public Refund RefundPayment(string paymentIntentId)
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
            };

            var service = new RefundService();
            return service.Create(options);
        }
    }
}
