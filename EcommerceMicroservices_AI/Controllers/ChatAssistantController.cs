using EcommerceMicroservices.Ai.Features.CatalogRecommendations;
using EcommerceMicroservices.Ai.Mcp;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace EcommerceMicroservices.Ai.Controllers;

[ApiController]
[Route("api/ai")]
public class ChatAssistantController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatService;

    public ChatAssistantController(IMediator mediator, Kernel kernel, ECommerceMcpToolsPlugin mcpPlugin)
    {
        _mediator = mediator;
        _kernel = kernel;

        // Inject custom server boundaries directly as system executable tools (MCP Strategy)
        _kernel.Plugins.AddFromObject(mcpPlugin, "ECommerceMcpTools");
        _chatService = _kernel.GetRequiredService<IChatCompletionService>();
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ExecuteQuerySession([FromBody] string userMessage)
    {
        // Intercept intent triggers to decide between direct RAG search or Agentic tool execution
        if (userMessage.Contains("find", StringComparison.OrdinalIgnoreCase) || userMessage.Contains("search", StringComparison.OrdinalIgnoreCase))
        {
            var searchDataResults = await _mediator.Send(new GetProductRecommendationsQuery(userMessage));
            return Ok(new { Output = searchDataResults.AiSummary, Data = searchDataResults.Products, Mode = "RAG_VectorSearch" });
        }

        //can add chat history as below for multiple plugin/tool
        /*var systemPrompt = @"You are an advanced platform co-pilot with microservice management permissions.
        When a user asks you to 'fulfill', 'process', or 'sync inventory' for an order:
        1. Call 'GetOrderStatusAsync' to find out which items and quantities are inside that order.
        2. Immediately look at the returned items, and call 'DeductProductQuantityAsync' sequentially for EACH item found.
        3. Summarize the final action status to the user once all steps are complete.";*/
        var chatHistory = new ChatHistory("You are an advanced platform co-pilot. You have deep access to core database endpoints via MCP Tools.");
        chatHistory.AddUserMessage(userMessage);

        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var responseContent = await _chatService.GetChatMessageContentAsync(chatHistory, settings, _kernel);
        return Ok(new { Output = responseContent.Content, Mode = "Agent_McpToolExecuting" });
    }
}
