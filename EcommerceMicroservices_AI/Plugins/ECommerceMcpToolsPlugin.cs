
using EcommerceMicroservices.AI.Configuration;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Text.Json;


namespace EcommerceMicroservices.Ai.Mcp;

//[McpServerToolType]
public class ECommerceMcpToolsPlugin
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ServiceEndpoints _services;
    private readonly IMediator _mediator;

    public ECommerceMcpToolsPlugin(IMediator mediator, IHttpClientFactory httpClientFactory, IOptions<ServiceEndpoints> services,
        IHttpContextAccessor httpContextAccessor)
    {
        _mediator = mediator;
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
        _services = services.Value;
    }

    //[KernelFunction, Description("Queries modern order database records to fetch the live packaging and tracking status details.")]
    //public async Task<string> GetOrderStatusAsync(
    //[KernelFunction, Description("Queries modern order database records to fetch the quantity in order details.")]
    // public async Task<string> GetOrderQuantityAsync(
    //[KernelFunction, Description("Queries modern order database records to fetch the amount in order details.")]
    //[McpServerTool]
    [KernelFunction]
    public async Task<string> GetOrderStatusAsync(
         [Description(
    "Get the current status of a customer's order, " +
    "including fulfillment and delivery information."
    )]string orderId)
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

            var orderGuid = Guid.Parse(orderId);

            var url = $"{_services.OrderService}/api/orders/{orderGuid}";

            var response = await client.GetAsync(url);
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

    [KernelFunction]
    public async Task<string> SearchProductsAsync([Description(
    "Search the product catalog using natural language. " +
    "Use this when the user is looking for a product, " +
    "recommendation, or product matching specific requirements."
    )]string searchTerm)
    {

        /* var result = await _mediator.Send(
             new GetProductRecommendationsQuery(searchTerm));

         return JsonSerializer.Serialize(new
         {
             Products = result.Products,
             Summary = result.AiSummary
         });*/
        return JsonSerializer.Serialize(new
        {
            Products = 12312414,
            Summary = "ccdrgegreg"
        });
    }
    // [McpServerTool]
    /* public async Task<string> GetProductAsync([Description(
    "Get detailed information about a specific product, " +
    "including its name, price, description, and attributes."
    )] string productId)*/
    [KernelFunction]
    [Description(
     "Gets complete product information using a product ID. " +
    "ALWAYS use this function when the user provides a product ID " +
    "and asks to find, get, show, or retrieve that product. " +
    "Returns the product name, price, description, and attributes."
    )]
    public async Task<string> GetProductAsync(string productId)
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

            var url =
                $"{_services.ProductService}/api/products/{productId}";

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

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

    [KernelFunction]
    public async Task<string> CheckCatalogAsync([Description(
    "Check whether a product is currently in stock " +
    "and return its available quantity."
    )]string productId)
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
            var url =
                $"{_services.InventoryService}/api/catalogs/{productId}";

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();

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
