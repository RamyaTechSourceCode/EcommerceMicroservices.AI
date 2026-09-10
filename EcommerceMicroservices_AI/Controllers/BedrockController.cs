using EcommerceMicroservices.AI.Bedrock;
using Microsoft.AspNetCore.Mvc;


namespace EcommerceMicroservices.AI.Controllers;

[ApiController]
[Route("api/bedrock")]
public class BedrockController : ControllerBase
{
    private readonly IBedrockChatService _bedrockChatService;

    public BedrockController(IBedrockChatService bedrockChatService)
    {
        _bedrockChatService = bedrockChatService;
    }

    [HttpPost("test")]
    public async Task<IActionResult> Test([FromBody] BedrockTestRequest request)
    {
        var response = await _bedrockChatService.ChatAsync(request.Message);

        return Ok(new
        {
            response
        });
    }
}

public class BedrockTestRequest
{
    public string Message { get; set; } = "";
}


