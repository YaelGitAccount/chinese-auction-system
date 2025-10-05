using Stripe;
using server_NET.DTOs;

namespace server_NET.Services
{
    public interface IStripeService
    {
        Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(PaymentIntentRequestDto request);
        Task<PaymentIntent> ConfirmPaymentAsync(string paymentIntentId);
        Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId);
    }

    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Set Stripe API key
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(PaymentIntentRequestDto request)
        {
            try
            {
                var service = new PaymentIntentService();
                
                // Convert amount to agorot (multiply by 100 for ILS)
                var amountInAgorot = (long)(request.Amount * 100);

                var options = new PaymentIntentCreateOptions
                {
                    Amount = amountInAgorot,
                    Currency = request.Currency,
                    Description = request.Description ?? "Chinese Auction Purchase",
                    Metadata = new Dictionary<string, string>
                    {
                        { "purchase_ids", string.Join(",", request.PurchaseIds) },
                        { "timestamp", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                    },
                    PaymentMethodTypes = new List<string> { "card" },
                    CaptureMethod = "automatic"
                };

                var paymentIntent = await service.CreateAsync(options);

                _logger.LogInformation("Payment intent created: {PaymentIntentId} for amount: {Amount} {Currency}", 
                    paymentIntent.Id, request.Amount, request.Currency.ToUpper());

                return new PaymentIntentResponseDto
                {
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = paymentIntent.Id,
                    Amount = request.Amount,
                    Currency = request.Currency
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error creating payment intent: {Error}", ex.Message);
                throw new Exception($"Payment processing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment intent");
                throw;
            }
        }

        public async Task<PaymentIntent> ConfirmPaymentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                _logger.LogInformation("Payment intent status: {PaymentIntentId} - {Status}", 
                    paymentIntentId, paymentIntent.Status);

                return paymentIntent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error confirming payment: {PaymentIntentId} - {Error}", 
                    paymentIntentId, ex.Message);
                throw new Exception($"Payment confirmation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming payment: {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

        public async Task<PaymentIntent> GetPaymentIntentAsync(string paymentIntentId)
        {
            try
            {
                var service = new PaymentIntentService();
                return await service.GetAsync(paymentIntentId);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe error getting payment intent: {PaymentIntentId} - {Error}", 
                    paymentIntentId, ex.Message);
                throw new Exception($"Payment retrieval error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment intent: {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }
    }
}
