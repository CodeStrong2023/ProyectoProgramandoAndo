using Microsoft.AspNetCore.Mvc;
using SistemaVenta.BLL.Interfaces;

namespace SistemaVenta.AplicacionWeb.Controllers
{
    public class PaymentController : Controller
    {

        private readonly IMercadoPagoService _mercadoPagoService;

        public PaymentController(IMercadoPagoService mercadoPagoService)
        {
            _mercadoPagoService = mercadoPagoService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomerId(string email)
        {
            try
            {
                var customerId = await _mercadoPagoService.GetCustomerIdByEmailAsync(email);

                if (customerId != null)
                {
                    return Ok(customerId);
                }
                else
                {
                    return NotFound($"No se encontró el customer_id para el email: {email}");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener el customer_id: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest paymentRequest)
        {
            // Validar que el paymentRequest no sea nulo
            if (paymentRequest == null)
            {
                return BadRequest("La solicitud de pago es inválida.");
            }

            // Validar que el formato de amount sea válido
            if (!decimal.TryParse(paymentRequest.Amount, out decimal amountDecimal))
            {
                return BadRequest("El formato de amount no es válido.");
            }

            try
            {
                // Generar el cardToken
                var cardToken = await _mercadoPagoService.GenerateCardTokenAsync(
                    paymentRequest.CardNumber,
                    paymentRequest.ExpirationMonth,
                    paymentRequest.ExpirationYear,
                    paymentRequest.CardholderName,
                    paymentRequest.SecurityCode
                );

                // Obtener el customerId del email proporcionado
                var customerId = await _mercadoPagoService.GetCustomerIdByEmailAsync(paymentRequest.Email);

                if (customerId == null)
                {
                    // Si no se encuentra el customerId, crear un nuevo cliente en MercadoPago
                    customerId = await _mercadoPagoService.CreateCustomerAsync(paymentRequest.Email);
                }

                // Asociar la tarjeta al cliente (opcional, dependiendo de la lógica de tu aplicación)
                // var cardId = await _mercadoPagoService.CreateCardAsync(customerId, cardToken);

                // Crear el pago utilizando los datos del formulario y el cardToken generado
                var payment = await _mercadoPagoService.CreatePaymentAsync(
                    amountDecimal,
                    paymentRequest.Description,
                    customerId,
                    cardToken,
                    paymentRequest.SecurityCode,
                    paymentRequest.Email,
                    paymentRequest.CardType
                );

                return Ok(payment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error al procesar el pago: {ex.Message}");
            }
        }

    }
}


public class PaymentRequest
{
    public string? Amount { get; set; }
    public string? Description { get; set; }
    public string? CardNumber { get; set; }
    public int ExpirationMonth { get; set; }
    public int ExpirationYear { get; set; }
    public string? CardholderName { get; set; }
    public string? SecurityCode { get; set; }
    public string? Email { get; set; }
    public string? CardType { get; set; }
}