using Amazon.BedrockRuntime;
using EcommerceMicroservices.Ai.IntegrationEvents;
using EcommerceMicroservices.Ai.Mcp;
using EcommerceMicroservices.AI.Configuration;
using EcommerceMicroservices.AI.Services;
using MediatR;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;


var builder = WebApplication.CreateBuilder(args);

//CORS Configuration for local development with React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Trust your React frontend origin
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Crucial to allow passing the secure authentication cookie
    });
});

//swagger configuration
builder.Services.AddSwaggerGen();

//configure strongly typed settings objects
builder.Services.Configure<ServiceEndpoints>(
    builder.Configuration.GetSection("Services"));
/*
//OpenAI API Key Configuration
var openAiConfig = builder.Configuration.GetSection("OpenAI");
var apiKey = openAiConfig["ApiKey"];

// Ensure the key isn't null or still the mock value before registering
if (string.IsNullOrEmpty(apiKey) || apiKey == "mock-key")
{
    throw new InvalidOperationException("A valid OpenAI API key must be configured.");
}

// Register the client and generator
builder.Services.AddSingleton(new OpenAIClient(apiKey));
*/
// Core Services Configuration
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Setup Security Context (Azure Entra ID Token Acceptance)
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AzureAd:Instance"] + builder.Configuration["AzureAd:TenantId"];
        options.Audience = builder.Configuration["AzureAd:Audience"];
    });*/

//AWS Bedrock Configuration
var region =
    builder.Configuration["AWS:Region"]
    ?? "ap-southeast-2";

var bedrockClient =
    new AmazonBedrockRuntimeClient(
        Amazon.RegionEndpoint.GetBySystemName(region));

// Register Bedrock client in ASP.NET DI
builder.Services.AddSingleton<IAmazonBedrockRuntime>(
    bedrockClient);

// Register Qdrant Vector Client
builder.Services.AddSingleton(sp => new QdrantClient("localhost", 6334));

// Register Microsoft Semantic Kernel & AI Engine
/*builder.Services.AddTransient(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    // Wire up Chat Completion Engine
    kernelBuilder.AddOpenAIChatCompletion(
    "gpt-4o",
    apiKey);
    //kernelBuilder.AddOpenAIChatCompletion("gpt-4o", builder.Configuration["OpenAI:ApiKey"] ?? "mock-key");
#pragma warning disable SKEXP0010
    // Modern approach: Registers the standardized IEmbeddingGenerator model
    kernelBuilder.AddOpenAIEmbeddingGenerator("text-embedding-3-small", builder.Configuration["OpenAi:ApiKey"] ?? "mock-key");
#pragma warning restore SKEXP0010
    return kernelBuilder.Build();
});*/


//Register MCP Plugins & Background Event Loops
builder.Services.AddScoped<ECommerceMcpToolsPlugin>();
builder.Services.AddScoped<ChatService>();
//Semantic Kernel Chat Completion Service using AWS Bedrock
builder.Services.AddScoped<Kernel>(sp =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    // Get Bedrock client from ASP.NET Core DI
    var bedrock =
        sp.GetRequiredService<IAmazonBedrockRuntime>();

    // Make Bedrock available to Semantic Kernel
    kernelBuilder.Services.AddSingleton<
        IAmazonBedrockRuntime>(bedrock);

    // Make configuration available to BedrockChatCompletionService
    kernelBuilder.Services.AddSingleton<IConfiguration>(
        builder.Configuration);

    // Register our custom Bedrock Semantic Kernel adapter
    kernelBuilder.Services.AddSingleton<
        IChatCompletionService,
        BedrockChatCompletionService>();

    // Build Kernel
    var kernel = kernelBuilder.Build();

    // IMPORTANT:
    // Resolve the plugin from ASP.NET Core DI,
    // NOT from kernel.GetRequiredService()
    var mcpPlugin =
        sp.GetRequiredService<ECommerceMcpToolsPlugin>();

    // Register plugin exactly once for this Kernel instance
    kernel.Plugins.AddFromObject(
        mcpPlugin,
        "ECommerceTools");

    return kernel;
});
builder.Services.AddHostedService<ProductUpdatedConsumer>(); 
var app = builder.Build();

try
{
    // Resolve the Qdrant client directly from the built application host container
    var qdrantClient = app.Services.GetRequiredService<QdrantClient>();

    Console.WriteLine("Checking Qdrant database collections...");
    var collections = await qdrantClient.ListCollectionsAsync();

    if (!collections.Contains("products"))
    {
        Console.WriteLine("Collection 'products' not found. Creating it now...");
        await qdrantClient.CreateCollectionAsync(
            collectionName: "products",
            vectorsConfig: new VectorParams
            {
                Distance = Distance.Cosine,
                Size = 1536 // Matches the output size of text-embedding-3-small
            }
        );
        Console.WriteLine("Collection 'products' successfully created!");
    }
}
catch (Exception ex)
{
    // Logs the issue if your Ubuntu Docker container is turned off or misconfigured
    Console.WriteLine($"Critical Error initializing Qdrant: {ex.Message}");
    throw;
}
app.UseCors("LocalDevCorsPolicy");

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();