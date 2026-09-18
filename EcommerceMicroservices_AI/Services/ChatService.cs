using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EcommerceMicroservices.AI.Services;

public class ChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public ChatService(Kernel kernel)
    {
        _kernel = kernel;

        _chatService =
            _kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string?> ChatAsync(
        string userMessage,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            return "Please provide a question.";
        }

        var chatHistory = new ChatHistory();

        chatHistory.AddSystemMessage(
            """
            You are an advanced ecommerce platform AI assistant.

            You have access to ecommerce functions for:
            - Product information
            - Product search
            - Inventory availability
            - Order status
            - Product catalog recommendations

            IMPORTANT TOOL RULES:

            1. If the user provides a specific product ID and asks
               about that product, use the product information function.

            2. If the user asks whether a product is in stock,
               available, or asks about inventory, use the inventory
               function.

            3. If the user asks about an order, shipping status,
               packaging, tracking, or order status, use the order
               function.

            4. If the user asks for product recommendations,
               similar products, or products suitable for a need,
               use the product catalog search/recommendation function.

            5. Use functions for live ecommerce information.
               Do not invent product, inventory, or order information.

            6. Do not claim that you searched the product catalog
               unless you actually called the appropriate function.

            After receiving function results, explain the answer
            clearly and concisely to the user.
            """
        );

        chatHistory.AddUserMessage(userMessage);

        var response =
            await _chatService.GetChatMessageContentAsync(
                chatHistory,
                kernel: _kernel,
                cancellationToken: cancellationToken);

        return response.Content;
    }
}