using Amazon.BedrockRuntime;

namespace EcommerceMicroservices.AI.Bedrock
{
    public class BedrockChatService : IBedrockChatService
    {
        private readonly IAmazonBedrockRuntime _bedrockRuntime;

        public BedrockChatService(IAmazonBedrockRuntime bedrockRuntime)
        {
            _bedrockRuntime = bedrockRuntime;
        }

        public async Task<string> ChatAsync(string message)
        {
            // Your Bedrock implementation
            return "Ramya";
        }
    }

}
