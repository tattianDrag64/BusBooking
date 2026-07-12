using BusBooking.Entity;
using BusBooking.Models;
using BusBooking.Repository.UnitOfWork;
using BusBooking.Services.ServiceManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace BusBooking.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PaymentsController(IUnitOfWork unitOfWork, IServiceManager services) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IServiceManager _services = services;

        // POST api/payments/checkout/{orderId}
        [HttpPost("checkout/{orderId}")]
        public IActionResult CreateCheckoutSession(Guid orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }
            var userId = Guid.Parse(userIdClaim.Value);

            var order = _unitOfWork.Order.GetById(orderId);
            if (order == null || order.UserId != userId)
            {
                return NotFound(new { message = "Order not found." });
            }
            if (order.Status != OrderStatus.PendingPayment)
            {
                return Conflict(new { message = $"Order is not payable (status: {order.Status})." });
            }

            Session session;
            try
            {
                session = _services.PaymentService.CreateCheckoutSession(order);
            }
            catch (StripeException)
            {
                return StatusCode(StatusCodes.Status502BadGateway,
                    new { message = "Payment provider is unavailable right now. Please try again shortly." });
            }

            order.StripeSessionId = session.Id;
            _unitOfWork.Order.Update(order);
            _unitOfWork.Save();

            return Ok(new CheckoutSessionVM { CheckoutUrl = session.Url });
        }

        // POST api/payments/webhook — called by Stripe, not by our own frontend.
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> HandleWebhook()
        {
            var requestBody = await new StreamReader(Request.Body).ReadToEndAsync();

            Event stripeEvent;
            try
            {
                stripeEvent = _services.PaymentService.ConstructWebhookEvent(
                    requestBody,
                    Request.Headers["Stripe-Signature"]!);
            }
            catch (StripeException)
            {
                return BadRequest(new { message = "Invalid Stripe webhook signature." });
            }

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;
                if (session?.Id != null)
                {
                    ConfirmOrderForSession(session);
                }
            }

            return Ok();
        }

        private void ConfirmOrderForSession(Session session)
        {
            var order = _unitOfWork.Order.Get(o => o.StripeSessionId == session.Id, tracked: true);
            if (order == null || order.Status != OrderStatus.PendingPayment)
            {
                // Already confirmed/expired, or an order we don't recognize — nothing to do.
                // Stripe retries webhooks, so this path is hit again for the same event on retry.
                return;
            }

            order.Status = OrderStatus.Confirmed;
            // Needed to issue a refund later if the booking gets cancelled after payment.
            order.StripePaymentIntentId = session.PaymentIntentId;
            _unitOfWork.Order.Update(order);

            var seatIds = _unitOfWork.OrderSeat.GetByOrderId(order.Id).Select(os => os.SeatDetailId);
            _unitOfWork.SeatDetail.ConfirmSeats(seatIds);

            _unitOfWork.Save();
        }
    }
}
