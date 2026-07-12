using BusBooking.Entity;
using Stripe;
using Stripe.Checkout;

namespace BusBooking.Services.Interfaces
{
    public interface IPaymentService
    {
        // Creates a Stripe Checkout session for the given order and returns its hosted URL.
        // The session id is stored on the order so the webhook can match it back later.
        Session CreateCheckoutSession(Order order);

        // Verifies the Stripe-Signature header and parses the raw webhook payload.
        // Throws if the signature doesn't match — callers must not process unverified payloads.
        Stripe.Event ConstructWebhookEvent(string requestBody, string stripeSignature);

        // Refunds a previously captured payment in full. Throws StripeException on failure
        // (e.g. already refunded, invalid intent id) — callers must handle it explicitly.
        Refund RefundPayment(string paymentIntentId);
    }
}
