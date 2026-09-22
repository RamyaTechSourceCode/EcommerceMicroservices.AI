# 🛒 EcommerceMicroservices.AI

> **AI-Powered E-Commerce Microservices Platform using ASP.NET Core, Next.js, Semantic Kernel, AWS Bedrock, RAG, Qdrant, Kafka and AI Function Calling**

An enterprise-style e-commerce platform enhanced with **Generative AI and Agentic AI capabilities**.

The project combines a traditional **.NET microservices architecture** with an AI orchestration layer built using **Microsoft Semantic Kernel** and **Amazon Bedrock**.

The AI assistant can understand natural-language requests and decide when it should:

* Search product information
* Retrieve product recommendations
* Check inventory
* Check order status
* Retrieve order-related information
* Invoke backend microservice functions
* Use semantic/vector search for product discovery
* Generate a natural-language response using an LLM

The architecture is designed to demonstrate how **AI can be introduced into an existing enterprise microservices platform without tightly coupling the AI layer to individual business services**.

---

## 🎯 Architecture 

The project demonstrates:

* **Generative AI integration**
* **Agentic AI / Function Calling**
* **Semantic Kernel orchestration**
* **AWS Bedrock integration**
* **RAG and Vector Search**
* **Event-driven architecture**
* **AI integration with microservices**
* **Cloud-ready enterprise architecture**

---

# 📌 Project Overview

The platform follows this high-level architecture:

```text
                         ┌──────────────────────────────┐
                         │        NEXT.JS FRONTEND      │
                         │                              │
                         │   AI Chat / E-Commerce UI    │
                         └──────────────┬───────────────┘
                                        │
                                        │ HTTP
                                        ▼
                         ┌──────────────────────────────┐
                         │     ASP.NET CORE GATEWAY     │
                         │                              │
                         │ Authentication / Routing     │
                         │ AI Chat API                  │
                         └──────────────┬───────────────┘
                                        │
                                        ▼
                         ┌──────────────────────────────┐
                         │    AI ORCHESTRATION LAYER    │
                         │                              │
                         │     Semantic Kernel          │
                         │                              │
                         │ FunctionChoiceBehavior.Auto  │
                         └──────────────┬───────────────┘
                                        │
                     ┌──────────────────┴──────────────────┐
                     │                                     │
                     ▼                                     ▼
          ┌─────────────────────┐              ┌──────────────────────┐
          │    AWS BEDROCK      │              │      RAG LAYER       │
          │                     │              │                      │
          │ Amazon Nova Lite    │              │ Embeddings           │
          │ Converse API        │              │ Qdrant               │
          │ Tool Use            │              │ Vector Search        │
          └──────────┬──────────┘              └──────────┬───────────┘
                     │                                    │
                     │                                    │
                     ▼                                    ▼
          ┌─────────────────────┐              ┌──────────────────────┐
          │ Semantic Kernel     │              │ Product Knowledge    │
          │ Plugins / Functions │              │ / Product Vectors    │
          └──────────┬──────────┘              └──────────────────────┘
                     │
          ┌──────────┼───────────────┐
          │          │               │
          ▼          ▼               ▼
     Product      Inventory        Order
     Service      Service          Service
          │          │               │
          └──────────┼───────────────┘
                     │
                     ▼
              Existing E-Commerce
              Microservices
```

---

# 🎯 Project Objective

The objective of this project is to demonstrate how an existing **distributed e-commerce system can be enhanced with Generative AI**.

Instead of implementing an AI chatbot as a standalone application, the AI layer is integrated with real business capabilities.

For example, a user can ask:

```text
Is product 5772C2A6-CF1F-42E0-9218-40E264EF126A in stock?
```

The AI model does not need to know the inventory directly.

Instead:

```text
User
 ↓
Next.js
 ↓
ASP.NET Core
 ↓
Semantic Kernel
 ↓
AWS Bedrock
 ↓
Function Selection
 ↓
Inventory Function
 ↓
Inventory Microservice
 ↓
Function Result
 ↓
AWS Bedrock
 ↓
Natural Language Response
```

This demonstrates **LLM-powered orchestration over real enterprise APIs**.

---

# 🚀 Key Features

## 1. AI-Powered E-Commerce Assistant

Users interact with the platform using natural language.

Examples:

```text
Show me information about product ABC.

Is product ABC available?

How many units are available?

Where is my order?

What is the status of order 123?

Find products similar to this dress.

Do you have this product in stock?
```

The AI determines how the request should be handled.

---

# 2. Semantic Kernel

**Microsoft Semantic Kernel** is used as the AI orchestration layer.

Responsibilities include:

* AI service abstraction
* Chat history
* Kernel management
* Plugin registration
* Function calling
* Function invocation
* AI orchestration
* Tool/function metadata
* RAG integration

The project uses:

```text
Semantic Kernel
        │
        ├── Chat Completion Service
        │
        ├── Kernel
        │
        ├── Plugins
        │
        ├── Kernel Functions
        │
        └── FunctionChoiceBehavior.Auto()
```

Semantic Kernel's function-calling mechanism serializes available functions and their parameters, sends them to the model, processes the model's tool/function request, invokes the corresponding function, and sends the result back into the conversation.

This creates an AI orchestration loop rather than simply generating text.

---

# 3. AWS Bedrock Integration

The project integrates **Amazon Bedrock** as the LLM provider.

Current target configuration:

```text
AWS
 └── Amazon Bedrock
      └── Amazon Nova Lite
```

The project uses a custom Semantic Kernel implementation:

```text
BedrockChatCompletionService
            │
            implements
            ▼
IChatCompletionService
            │
            ▼
AWS Bedrock Runtime
```

This allows the rest of the application to interact with Bedrock through the Semantic Kernel abstraction.

The application therefore avoids coupling the controller directly to the AWS SDK.

Architecture:

```text
Controller
    │
    ▼
Semantic Kernel
    │
    ▼
IChatCompletionService
    │
    ▼
BedrockChatCompletionService
    │
    ▼
IAmazonBedrockRuntime
    │
    ▼
Amazon Bedrock
    │
    ▼
Amazon Nova Lite
```

Amazon Bedrock is a managed service for accessing foundation models from Amazon and other providers.

---
---

# 4. Amazon Nova Lite

The current Bedrock implementation is designed around Amazon Nova Lite.

AWS inference profiles can be used as the model identifier for supported Bedrock invocation APIs. This is particularly useful when a model is accessed through a cross-Region inference profile rather than directly through a foundation-model identifier.

Current project configuration follows the APAC inference-profile approach.

Example:
```text
{
  "AWS": {
    "Region": ""
  },
  "Bedrock": {
    "ModelId": ""
  }
}
```
Do not commit AWS access keys, secret keys or other credentials to GitHub.

---

# 5. Custom IChatCompletionService

One of the important architectural features of this project is the custom Bedrock adapter.

BedrockChatCompletionService
        :
        : implements
        ▼
IChatCompletionService

This means Semantic Kernel does not need to know that the underlying provider is AWS Bedrock.

The application can use the standard Semantic Kernel abstraction:

IChatCompletionService

while the implementation internally communicates with:

IAmazonBedrockRuntime

This provides a clean separation between:
```text
AI Orchestration
       │
       ▼
AI Provider
```
rather than:
```text
Controller
       │
       ▼
AWS SDK
```
---

# 6. AI Function Calling

The project uses Semantic Kernel plugins to expose business capabilities to the AI model.

Conceptually:

                    AI MODEL
                       │
              "I need inventory"
                       │
                       ▼
             Function Selection
                       │
                       ▼
             Kernel Function
                       │
                       ▼
              Inventory Service

The application enables automatic function selection using:

FunctionChoiceBehavior.Auto()

Semantic Kernel documents Auto() as the behavior that allows the model to decide whether to call available functions and which functions to call.

---

# 7. E-Commerce AI Plugin

The project contains an e-commerce tool/plugin layer.

Conceptually:
```text
ECommerceMcpToolsPlugin
          │
          ├── Product Functions
          │
          ├── Inventory Functions
          │
          └── Order Functions
```
---

# 8. RAG — Retrieval Augmented Generation

The project also contains a RAG architecture for product knowledge.

The RAG flow is:
```text
User Query
     │
     ▼
Embedding Generation
     │
     ▼
Vector Representation
     │
     ▼
Qdrant
     │
     ▼
Cosine / Vector Similarity Search
     │
     ▼
Relevant Product Information
     │
     ▼
LLM
     │
     ▼
Grounded Response
```

RAG is primarily used for product discovery and semantic search scenarios.
These functions act as the bridge between the AI model and the existing microservices.
