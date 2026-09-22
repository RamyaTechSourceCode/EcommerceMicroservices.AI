using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using Amazon.Runtime.Documents;
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
        if (kernel == null)
        {
            throw new ArgumentNullException(
                nameof(kernel),
                "Kernel is required for Bedrock tool calling.");
        }

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

        /*
         * Build Bedrock tool definitions from Semantic Kernel plugins.
         */
        var toolConfig = BuildToolConfiguration(kernel);

        /*
         * Send the initial request.
         */
        var response =
            await SendConverseRequestAsync(
                modelId,
                messages,
                systemInstructions,
                toolConfig,
                cancellationToken);

        /*
         * Handle tool calls until Bedrock returns a normal
         * assistant response.
         */
        const int maxToolRounds = 5;

        for (var round = 0; round < maxToolRounds; round++)
        {
            var responseMessage =
                response.Output?.Message;

            if (responseMessage == null)
            {
                return
                [
                    new ChatMessageContent(
                        AuthorRole.Assistant,
                        string.Empty)
                ];
            }

            var toolUses =
                responseMessage.Content?
                    .Where(x => x.ToolUse != null)
                    .Select(x => x.ToolUse!)
                    .ToList()
                ?? [];

            /*
             * No tool requested.
             * Return the normal Bedrock response.
             */
            if (toolUses.Count == 0)
            {
                var responseText =
                    responseMessage.Content?
                        .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                        .Select(x => x.Text)
                        .FirstOrDefault()
                    ?? string.Empty;

                return
                [
                    new ChatMessageContent(
                        AuthorRole.Assistant,
                        responseText)
                ];
            }

            /*
             * IMPORTANT:
             * Add the assistant ToolUse message to the conversation
             * before sending ToolResult.
             */
            messages.Add(responseMessage);

            var toolResults = new List<ContentBlock>();

            foreach (var toolUse in toolUses)
            {
                var toolResult =
                    await ExecuteKernelFunctionAsync(
                        kernel,
                        toolUse,
                        cancellationToken);

                toolResults.Add(
                    new ContentBlock
                    {
                        ToolResult = toolResult
                    });
            }

            /*
             * Tool results must be sent as a USER message.
             */
            messages.Add(
                new Message
                {
                    Role = ConversationRole.User,
                    Content = toolResults
                });

            /*
             * Send tool results back to Bedrock.
             */
            response =
                await SendConverseRequestAsync(
                    modelId,
                    messages,
                    systemInstructions,
                    toolConfig,
                    cancellationToken);
        }

        throw new InvalidOperationException(
            "Maximum Bedrock tool-calling rounds exceeded.");
    }


    private ToolConfiguration BuildToolConfiguration(
        Kernel kernel)
    {
        var tools = new List<Tool>();

        foreach (var plugin in kernel.Plugins)
        {
            foreach (var function in plugin)
            {
                var metadata = function.Metadata;

               var properties = new Dictionary<string, object>();

var required = new List<string>();

foreach (var parameter in metadata.Parameters)
{
    properties[parameter.Name] = new
    {
        type = "string",
        description = parameter.Description ?? parameter.Name
    };

    if (parameter.IsRequired)
    {
        required.Add(parameter.Name);
    }
}

var inputSchema = new
{
    type = "object",
    properties = properties,
    required = required.ToArray()
};

var inputDocument =
    Document.FromObject(inputSchema);

var toolSpecification =
    new ToolSpecification
    {
        Name =
            BuildToolName(
                plugin.Name,
                metadata.Name),

        Description =
            string.IsNullOrWhiteSpace(metadata.Description)
                ? $"Executes {metadata.Name}."
                : metadata.Description,

        InputSchema =
            new ToolInputSchema
            {
                Json = inputDocument
            }
    };
                tools.Add(
                    new Tool
                    {
                        ToolSpec = toolSpecification
                    });
            }
        }

        return new ToolConfiguration
        {
            Tools = tools
        };
    }


    private static string BuildToolName(
        string pluginName,
        string functionName)
    {
        /*
         * Bedrock tool names allow:
         * a-z, A-Z, 0-9, _ and -
         *
         * Example:
         * ECommerceTools_CheckInventoryAsync
         */
        return $"{pluginName}_{functionName}"
            .Replace(".", "_")
            .Replace(" ", "_");
    }


    private async Task<ToolResultBlock>
        ExecuteKernelFunctionAsync(
            Kernel kernel,
            ToolUseBlock toolUse,
            CancellationToken cancellationToken)
    {
        var toolName = toolUse.Name;

        /*
         * Find the corresponding Semantic Kernel function.
         */
        KernelFunction? selectedFunction = null;

        foreach (var plugin in kernel.Plugins)
        {
            foreach (var function in plugin)
            {
                var expectedName =
                    BuildToolName(
                        plugin.Name,
                        function.Metadata.Name);

                if (string.Equals(
                        expectedName,
                        toolName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    selectedFunction = function;
                    break;
                }
            }

            if (selectedFunction != null)
                break;
        }

        if (selectedFunction == null)
        {
            return new ToolResultBlock
            {
                ToolUseId = toolUse.ToolUseId,

                Status = ToolResultStatus.Error,

                Content =
                [
                    new ToolResultContentBlock
                    {
                        Text =
                            $"Semantic Kernel function '{toolName}' was not found."
                    }
                ]
            };
        }

        try
        {
            /*
             * Bedrock gives us JSON arguments.
             */
            var arguments =
                new KernelArguments();

            var inputData = toolUse.Input.AsDictionary();

            foreach (var property in inputData)
            {
                arguments[property.Key] =
                    ConvertDocumentValue(property.Value);
            }

            /*
             * Invoke the actual Semantic Kernel function.
             */
            var result =
                await kernel.InvokeAsync(
                    selectedFunction,
                    arguments,
                    cancellationToken);

            var resultText =
                result.GetValue<object?>()?.ToString()
                ?? string.Empty;

            return new ToolResultBlock
            {
                ToolUseId = toolUse.ToolUseId,

                Status = ToolResultStatus.Success,

                Content =
                [
                    new ToolResultContentBlock
                    {
                        Text = resultText
                    }
                ]
            };
        }
        catch (Exception ex)
        {
            return new ToolResultBlock
            {
                ToolUseId = toolUse.ToolUseId,

                Status = ToolResultStatus.Error,

                Content =
                [
                    new ToolResultContentBlock
                    {
                        Text =
                            $"Tool execution failed: {ex.Message}"
                    }
                ]
            };
        }
    }

    private static object? ConvertDocumentValue(
    Document document)
    {
        return document.ToString();
    }

    private async Task<ConverseResponse>
        SendConverseRequestAsync(
            string modelId,
            List<Message> messages,
            List<string> systemInstructions,
            ToolConfiguration toolConfig,
            CancellationToken cancellationToken)
    {
        var request = new ConverseRequest
        {
            ModelId = modelId,

            Messages = messages,

            InferenceConfig =
                new InferenceConfiguration
                {
                    MaxTokens = 1000,
                    Temperature = 0.2F
                },

            ToolConfig = toolConfig
        };

        if (systemInstructions.Count > 0)
        {
            request.System =
            [
                new SystemContentBlock
                {
                    Text =
                        string.Join(
                            "\n\n",
                            systemInstructions)
                }
            ];
        }

        return await _client.ConverseAsync(
            request,
            cancellationToken);
    }


    public async IAsyncEnumerable<StreamingChatMessageContent>
        GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? executionSettings = null,
            Kernel? kernel = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation]
            CancellationToken cancellationToken = default)
    {
        /*
         * For now we use the non-streaming Converse API.
         *
         * Tool calling is handled correctly by
         * GetChatMessageContentsAsync().
         */
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
