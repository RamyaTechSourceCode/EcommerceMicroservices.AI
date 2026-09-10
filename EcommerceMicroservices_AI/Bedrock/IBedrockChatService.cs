namespace EcommerceMicroservices.AI.Bedrock
{
    public interface IBedrockChatService
    {
        Task<string> ChatAsync(string message);
    }
}
