using System.ComponentModel;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.SemanticKernel;

namespace EcommerceMicroservices.Ai.Mcp;

public class ECommerceMcpToolsPlugin
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ECommerceMcpToolsPlugin(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    //[KernelFunction, Description("Queries modern order database records to fetch the live packaging and tracking status details.")]
    //public async Task<string> GetOrderStatusAsync(
    //[KernelFunction, Description("Queries modern order database records to fetch the quantity in order details.")]
    // public async Task<string> GetOrderQuantityAsync(
    [KernelFunction, Description("Queries modern order database records to fetch the amount in order details.")]
    public async Task<string> GetOrderAmountAsync(
         [Description("The unique alphanumeric ID token string (GUID) assigned to the order tracking layer.")] string orderId)
    {

        try
        { 
            var client = _httpClientFactory.CreateClient();

            // Securely pass identity tokens downstream 
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.Headers.TryGetValue("Authorization", out var authHeader) == true)
            {
                client.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse(authHeader.ToString());
            }

            var response = await client.GetAsync($"http://xxx/api/orders/{Guid.Parse(orderId)}");
            if (!response.IsSuccessStatusCode)
            {
                return $"Error tracing status context code tracking layer parameter match token error: {response.StatusCode}";
            }

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException httpEx)
        {
            // This catches 502 Bad Gateway, DNS failures, or refused connections
            return $"Network Error: Failed to reach the order microservice backend. Details: {httpEx.Message}";
        }
        catch (Exception ex)
        {
            // Catches any other unexpected processing errors (like malformed GUID parsing)
            return $"Internal Plugin Error: {ex.Message}";
        }
    }
}
