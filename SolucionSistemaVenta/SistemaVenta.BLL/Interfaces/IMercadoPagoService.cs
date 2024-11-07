using MercadoPago.Resource.Payment;

namespace SistemaVenta.BLL.Interfaces
{
    public interface IMercadoPagoService
    {
        Task<Payment> CreatePaymentAsync(decimal amount, string description, string customerId, string cardToken, string securityCode, string email, string cardType);
        Task<string> CreateCustomerAsync(string email);
        Task<string> CreateCardAsync(string customerId, string cardToken);
        Task<string> GetCustomerIdByEmailAsync(string email);
        Task<string> GenerateCardTokenAsync(string cardNumber, int expirationMonth, int expirationYear, string cardholderName, string securityCode);
    }
}
