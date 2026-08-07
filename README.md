# E-COMMERCE MICROSERVICES AI ASSISTANT

This repository houses an advanced **Autonomous Gateway Service** seamlessly integrated into an e-commerce microservices ecosystem. Leveraging modern Artificial Intelligence methodologies, the service functions as an intelligent routing and orchestration hub right behind the platform's API gateway. It dynamically manages Retrieval-Augmented Generation (RAG) and Agentic tool workflows using the Model Context Protocol (MCP). Built entirely on the latest .NET framework, it bridges natural language interactions with backend systems to perform autonomous vector searches, live transaction tracing, and operational inventory modifications within a single conversation turn.

---

## SYSTEM ARCHITECTURE OVERVIEW

The application utilizes a hybrid execution topology. It dynamically transitions between structured Vector Database queries (RAG pattern) and autonomous agent workflows (ReAct loops) based on real-time classification of user semantic intent.

```text
                      [ USER PROMPT VIA NEXT.JS FRONTEND ]
                                       │
                                       ▼
                            [ ASP.NET CORE GATEWAY ]
                                       │
         ┌─────────────────────────────┴─────────────────────────────┐
         ▼                                                           ▼
  Intent Contains:                                            Intent Contains:
 "Find" / "Search"                                          Operational Requests
         │                                                           │
         ▼ (RAG Path)                                                ▼ (Agentic Path)
┌─────────────────┐                                         ┌──────────────────────────────────┐
│  OpenAI Embed   │                                         │   ChatHistory State Container    │
│  Model (1536d)  │                                         └─────────────────┬────────────────┘
└────────┬────────┘                                                           │
         │ (Vector float[])                                                   ▼
         ▼                                                  ┌──────────────────────────────────┐
┌─────────────────┐                                         │   Semantic Kernel Orchestration  │
│Qdrant Vector DB │                                         │ with FunctionChoiceBehavior.Auto │
└────────┬────────┘                                         └─────────────────┬────────────────┘
         │ Cosine Search                                                      │
         ▼                                                                    ▼
┌─────────────────┐                                         ┌──────────────────────────────────┐
│ Structured RAG  │                                         │     ECommerceMcpToolsPlugin      │
│Product Payloads │                                         └────────┬────────────────┬────────┘
└─────────────────┘                                                  │                │
                                                                     ▼                ▼
                                                            ┌────────────────┐┌────────────────┐
                                                            │ Order Service  ││ Stock Service  │
                                                            │(HTTP/gRPC API) ││(HTTP/gRPC API) │
                                                            └────────────────┘└────────────────┘


```

## CORE TECHNICAL STACK

* **FRONTEND PLATFORM:** Next.js 14+ (App Router Topology), React 18, Tailwind CSS UI Framework.
* **BACKEND ARCHITECTURE:** .NET 8 ASP.NET Core Minimal APIs / Web APIs, MediatR (CQRS Pattern),Event driven architecture
* **AI INTERACTION ENGINES:** Microsoft Semantic Kernel, `Microsoft.Extensions.AI` Abstractions.
* **FOUNDATIONAL MODEL SERVICES:** OpenAI GPT-4o (Reasoning & Complete Session Logic), OpenAI `text-embedding-3-small` (1536-Dimensional Vector Geometries).
* **VECTOR STORAGE ENVIRONMENT:** Qdrant DB Container Cluster running via native Linux gRPC subchannels.
* **SECURITY LAYERS:** Azure Entra ID Token Validation, Secure BFF Cross-Origin Resource Policies.

---

## DESIGN PATTERNS & PRINCIPLES

* **RETRIEVAL-AUGMENTED GENERATION (RAG):** User searches are intercept-mapped, transformed into dense embeddings via `IEmbeddingGenerator`, and processed using Cosine similarity scoring arrays to extract contextual catalog data directly from Qdrant.
* **MODEL CONTEXT PROTOCOL & PLUGINS:** Methods are decorated with `[KernelFunction]` and semantic argument definitions. This enables the LLM to inspect system manifests and execute low-level database lookups dynamically.
* **STATELESS CHATHISTORY CONTEXT:** System and session payloads are managed via sequential context layers, tracking state profiles across execution loops.
* **FAIL-FAST DATA INITIALIZATION:** Database parameters and collection geometries are automatically created via `VectorParams` configuration steps on backend process initialization.

---

## REGISTRATION & INTEGRATION CODES

### PROGRAM.CS APPLICATION DEPENDENCY SETUP
```csharp
#pragma warning disable SKEXP0010 

var builder = WebApplication.CreateBuilder(args);

// Configure Secure Core Tokens
var openAiKey = builder.Configuration["OpenAI:ApiKey"];

// Register Semantic Kernel & AI Service Contracts
var kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIEmbeddingGenerator("text-embedding-3-small", openAiKey);
kernelBuilder.AddOpenAIChatCompletion("gpt-4o", openAiKey);

Kernel kernel = kernelBuilder.Build();
builder.Services.AddSingleton(kernel);

// Register High Performance Qdrant Database Subchannel Engine
builder.Services.AddSingleton(new QdrantClient("localhost", 6334));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

var app = builder.Build();

// Ensure Automated Collection Initialization Prior to App Runtime Boot
using (var scope = app.Services.CreateScope())
{
    var qdrant = scope.ServiceProvider.GetRequiredService<QdrantClient>();
    var collections = await qdrant.ListCollectionsAsync();
    if (!collections.Contains("products"))
    {
        await qdrant.CreateCollectionAsync("products", new VectorParams { Size = 1536, Distance = Distance.Cosine });
    }
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## REPOSITORY SETUP & RUNTIME INSTRUCTIONS

### 1. RUN OR START VIRTUAL DATABASES VIA UBUNTU TERMINAL
Pull down and spin up persistent storage containers utilizing the GitHub Container Registry mirrors:
```bash
sudo docker run -d \
  -p 6333:6333 \
  -p 6334:6334 \
  -v qdrant_storage:/qdrant/storage \
  ghcr.io/qdrant/qdrant:latest
```

### 2. CONFIGURING DEVELOPMENT CONFIGURATIONS
Create an `appsettings.Development.json` configuration inside the API project directory root layer:
```json
{
  "OpenAI": {
    "ApiKey": "sk-proj-YOUR_ACTUAL_DEVELOPER_KEY_HERE"
  }
}
```

### 3. LAUNCHING BACKEND MICROSERVICES
```bash
cd EcommerceMicroservices_AI
dotnet restore
dotnet run
```

### 4. LAUNCHING CLIENT REACT INTERFACES
```bash
cd EcommerceBFF
npm install
npm run dev
```

---

## POSTMAN INTEGRATION TESTING ENDPOINTS

### COMPLEX CONVERSATIONAL CHAT ENGINE (AGENT TRACING / INVENTORY ADJUSTMENT)
* **METHOD:** `POST`
* **ENDPOINT:** `http://localhost:3000/api/chat`
* **HEADERS:** `Content-Type: application/json`
* **SAMPLE TEXT PAYLOAD BODY:**
```json
{
  "userMessage": "Please check the ordered quantity for order 45f8e22b-8a21-4f19-b2c7-742a84d43611."
}
```

### CATALOG SEMANTIC DISCOVERY INTERCEPT (RAG SEARCH METHOD)
* **METHOD:** `POST`
* **ENDPOINT:** `http://localhost:3000/api/chat`
* **HEADERS:** `Content-Type: application/json`
* **SAMPLE TEXT PAYLOAD BODY:**
```json
{
  "userMessage": "Find me some ergonomic mechanical keyboards that are quiet for shared office environments."
}
```
