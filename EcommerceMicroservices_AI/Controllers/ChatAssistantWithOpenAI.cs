
using EcommerceMicroservices.Ai.Mcp;
using EcommerceMicroservices.AI.Dto;
using EcommerceMicroservices.AI.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace EcommerceMicroservices.AI.Controllers;

[ApiController]
[Route("api/openai")]
public class ChatAssistantOpenAIController : ControllerBase
{
   
    private readonly ChatService _chatService;

    public ChatAssistantOpenAIController(
        ChatService chatService)
    {
        _chatService = chatService;
 
    }

    [HttpPost("chat")]
    public async Task<IActionResult> ExecuteQuerySession(
     [FromBody] ChatRequest request,
     CancellationToken cancellationToken)
    {
        var response = await _chatService.ChatAsync(
            request.UserMessage,
            cancellationToken);

        return Ok(new
        {
            response
        });
    }

    
}
    //Kernel in case of multiple tools/plugins, we can intercept the intent triggers to decide between direct RAG search or Agentic tool execution
    /*[HttpPost("chat")]
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
        3. Summarize the final action status to the user once all steps are complete.";//
        var chatHistory = new ChatHistory("You are an advanced platform co-pilot. You have deep access to core database endpoints via MCP Tools.");
        chatHistory.AddUserMessage(userMessage);

        OpenAIPromptExecutionSettings settings = new()
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var responseContent = await _chatService.GetChatMessageContentAsync(chatHistory, settings, _kernel);
        return Ok(new { Output = responseContent.Content, Mode = "Agent_McpToolExecuting" });
    }*/

