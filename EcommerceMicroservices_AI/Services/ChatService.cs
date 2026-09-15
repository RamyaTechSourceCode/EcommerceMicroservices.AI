using EcommerceMicroservices.Ai.Mcp;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace EcommerceMicroservices.AI.Services
{
    public class ChatService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatService;

        public ChatService(Kernel kernel, ECommerceMcpToolsPlugin mcpPlugin)
        {
            _kernel = kernel;
          
            _kernel.Plugins.AddFromObject(
                mcpPlugin,
                "ECommerceTools");
            _chatService = kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string?> ChatAsync(string userMessage)
        {
            var chatHistory = new ChatHistory(
                "You are an advanced ecommerce platform co-pilot. " +
            "You have access to product search, product information, inventory, " +
            "and order management tools. " +

            "When the user provides a specific product ID and asks to find, " +
            "get, show, or retrieve the product, you MUST call GetProductAsync. " +

            "When the user asks whether a product is in stock, " +
            "you MUST call CheckCatalogAsync. " +

            "When the user asks about an order status, " +
            "you MUST call GetOrderStatusAsync. " +

            "Use tools to retrieve live ecommerce data instead of guessing."
            );

            chatHistory.AddUserMessage(userMessage);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior =
                    FunctionChoiceBehavior.Auto()
            };

            var response =
                await _chatService.GetChatMessageContentAsync(
                    chatHistory,
                    settings,
                    _kernel);

            return response.Content;
        }
    }
}
