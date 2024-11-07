using MercadoPago.Client.Payment;
using MercadoPago.Config;
using MercadoPago.Resource.Payment;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SistemaVenta.BLL.Interfaces;
using System.Text;
using System.Threading.Tasks;

public class MercadoPagoService : IMercadoPagoService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public MercadoPagoService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        MercadoPagoConfig.AccessToken = _configuration["MercadoPagoConfig:AccessToken"];
    }

    public async Task<string> GetCustomerIdByEmailAsync(string email)
    {
        try
        {
            var accessToken = _configuration["MercadoPagoConfig:AccessToken"];
            var url = $"https://api.mercadopago.com/v1/customers/search?access_token={accessToken}&email={email}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var customers = JObject.Parse(responseBody)["results"] as JArray;

                if (customers != null && customers.Any())
                {
                    var customer = customers.First();
                    return customer["id"].ToString();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar cliente en MercadoPago por email: {ex.Message}");
            throw;
        }
    }

    public async Task<string> CreateCustomerAsync(string email)
    {
        try
        {
            var url = "https://api.mercadopago.com/v1/customers";
            var accessToken = _configuration["MercadoPagoConfig:AccessToken"];
            var requestBody = new
            {
                email = email
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<dynamic>(responseBody);
                return customer["id"].ToString();
            }

            throw new Exception($"Error al crear el cliente: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear cliente en MercadoPago: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GenerateCardTokenAsync(string cardNumber, int expirationMonth, int expirationYear, string cardholderName, string securityCode)
    {
        try
        {
            var url = "https://api.mercadopago.com/v1/card_tokens";
            var accessToken = _configuration["MercadoPagoConfig:AccessToken"];

            var requestData = new
            {
                card_number = cardNumber,
                expiration_month = expirationMonth,
                expiration_year = expirationYear,
                cardholder = new
                {
                    name = cardholderName
                },
                security_code = securityCode
            };

            var jsonRequest = JsonConvert.SerializeObject(requestData);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(responseBody);
                return result.id; // Devuelve el cardToken generado
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al generar el cardToken: {errorResponse}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar el cardToken en MercadoPago: {ex.Message}");
            throw;
        }
    }

    public async Task<string> CreateCardAsync(string customerId, string cardToken)
    {
        try
        {
            var url = $"https://api.mercadopago.com/v1/customers/{customerId}/cards";
            var requestBody = new
            {
                token = cardToken
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var card = JsonConvert.DeserializeObject<dynamic>(responseBody);
                return card["id"].ToString();
            }

            throw new Exception($"Error al asociar la tarjeta: {response.ReasonPhrase}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al asociar tarjeta en MercadoPago: {ex.Message}");
            throw;
        }
    }

    public async Task<Payment> CreatePaymentAsync(decimal amount, string description, string customerId, string cardToken, string securityCode, string email, string cardType)
    {
        try
        {
            var paymentRequest = new PaymentCreateRequest
            {
                TransactionAmount = amount,
                Token = cardToken, // Usar el cardToken generado
                Description = description,
                Installments = 1,
                PaymentMethodId = cardType, 
                Payer = new PaymentPayerRequest
                {
                    Email = email,
                }
            };

            var client = new PaymentClient();
            Payment payment = await client.CreateAsync(paymentRequest);

            return payment;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear el pago en MercadoPago: {ex.Message}");
            throw;
        }
    }
}