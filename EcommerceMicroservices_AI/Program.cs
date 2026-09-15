using Amazon.BedrockRuntime;
using EcommerceMicroservices.Ai.IntegrationEvents;
using EcommerceMicroservices.Ai.Mcp;
using EcommerceMicroservices.AI.Bedrock;
using EcommerceMicroservices.AI.Configuration;
using EcommerceMicroservices.AI.Services;
using MediatR;
using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using OpenAI;
using Qdrant.Client;
using Qdrant.Client.Grpc;


var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddSwaggerGen();

builder.Services.Configure<ServiceEndpoints>(
    builder.Configuration.GetSection("Services"));

var openAiConfig = builder.Configuration.GetSection("OpenAI");
var apiKey = openAiConfig["ApiKey"];

// Ensure the key isn't null or still the mock value before registering
if (string.IsNullOrEmpty(apiKey) || apiKey == "mock-key")
{
    throw new InvalidOperationException("A valid OpenAI API key must be configured.");
}

// Register the client and generator
builder.Services.AddSingleton(new OpenAIClient(apiKey));

// 1. Core Services Configuration
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// 2. Setup Security Context (Azure Entra ID Token Acceptance)
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AzureAd:Instance"] + builder.Configuration["AzureAd:TenantId"];
        options.Audience = builder.Configuration["AzureAd:Audience"];
    });*/

// 3. Register Qdrant Vector Client
builder.Services.AddSingleton(sp => new QdrantClient("localhost", 6334));

// 4. Register Microsoft Semantic Kernel & AI Engine
builder.Services.AddTransient(sp =>
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
});


// 5. Register MCP Plugins & Background Event Loops
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<ECommerceMcpToolsPlugin>();
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