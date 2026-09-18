using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace EcommerceMicroservices.AI.Services;

public class BedrockChatCompletionService : IChatCompletionService
{
    private readonly IAmazonBedrockRuntime _client;
    private readonly IConfiguration _configuration;

    public IReadOnlyDictionary<string, object?> Attributes { get; }
        = new Dictionary<string, object?>();

    public BedrockChatCompletionService(
        IAmazonBedrockRuntime client,
        IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<ChatMessageContent>>
    GetChatMessageContentsAsync(
        ChatHistory chatHistory,
        PromptExecutionSettings? executionSettings = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        var modelId =
            _configuration["Bedrock:ModelId"]
            ?? throw new InvalidOperationException(
                "Bedrock:ModelId is not configured.");

        var messages = new List<Message>();
        var systemInstructions = new List<string>();

        foreach (var message in chatHistory)
        {
            if (string.IsNullOrWhiteSpace(message.Content))
                continue;

            // Bedrock Converse requires system messages
            // to be sent separately from Messages.
            if (message.Role == AuthorRole.System)
            {
                systemInstructions.Add(message.Content);
                continue;
            }

            var role =
                message.Role == AuthorRole.User
                    ? ConversationRole.User
                    : ConversationRole.Assistant;

            messages.Add(
                new Message
                {
                    Role = role,
                    Content =
                    [
                        new ContentBlock
                    {
                        Text = message.Content
                    }
                    ]
                });
        }

        // Bedrock requires the first conversation message
        // to be from the user.
        if (messages.Count == 0 ||
            messages[0].Role != ConversationRole.User)
        {
            throw new InvalidOperationException(
                "Bedrock conversation must start with a user message.");
        }

        var request = new ConverseRequest
        {
            ModelId = modelId,

            Messages = messages,

            InferenceConfig = new InferenceConfiguration
            {
                MaxTokens = 1000,
                Temperature = 0.2F
            }
        };

        // Add system instructions separately
        if (systemInstructions.Count > 0)
        {
            request.System =
            [
                new SystemContentBlock
            {
                Text = string.Join(
                    "\n\n",
                    systemInstructions)
            }
            ];
        }

        var response =
            await _client.ConverseAsync(
                request,
                cancellationToken);

        var responseText =
            response.Output?
                .Message?
                .Content?
                .FirstOrDefault()?
                .Text
            ?? string.Empty;

        return new List<ChatMessageContent>
        {
            new ChatMessageContent(
                AuthorRole.Assistant,
                responseText)
        };
    }

    public async IAsyncEnumerable<StreamingChatMessageContent>
        GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        var result =
            await GetChatMessageContentsAsync(
                chatHistory,
                executionSettings,
                kernel,
                cancellationToken);

        foreach (var message in result)
        {
            yield return new StreamingChatMessageContent(
                message.Role,
                message.Content);
        }
    }
}