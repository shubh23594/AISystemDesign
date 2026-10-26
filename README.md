# .NET AI System Design Copilot

A hands-on system design assistant built with **C#/.NET**, **Microsoft Agent Framework**, **OpenAI**, **SQL Server 2025 vector search**, and **Model Context Protocol (MCP)**.

The solution explores architecture generation, semantic similarity, retrieval-augmented generation (RAG), and AI tool calling. A separate ASP.NET Core MCP server exposes three reusable engineering tools to compatible MCP clients, including ChatGPT when a custom MCP connection is available.

> **Project scope:** This is an educational proof of concept, not a production-ready architecture evaluation service. Calculations are estimates; architecture validation is rule-based.

## Features

| Feature | Description | Available in |
| --- | --- | --- |
| **System Design Generator** | Generates architecture recommendations from requirements using an AI agent | Console |
| **Architecture Similarity Analyzer** | Creates embeddings and compares design descriptions with cosine similarity | Console |
| **System Design RAG Assistant** | Stores architecture knowledge in SQL Server, retrieves relevant vector matches, and uses the results as context for generated answers | Console |
| **Tool-Assisted Architecture Analysis** | Lets an AI agent invoke C# calculations and a rule-based architecture review | Console |
| **MCP Engineering Tools** | Exposes capacity, storage, and architecture checks through an MCP endpoint | MCP server |

## Solution structure

```text
AISystemDesign/
├── AISystemDesign.Console/
│   ├── Program.cs
│   └── Features/
│       ├── SystemDesignGenerator.cs
│       ├── ArchitectureSimilarityAnalyzer.cs
│       ├── SystemDesignRagAssistant.cs
│       └── ToolAssistedSystemDesignAgent.cs
├── AISystemDesign.Core/
│   └── SystemDesignTools.cs
├── AISystemDesign.McpServer/
│   ├── Program.cs
│   └── Tools/
│       └── SystemDesignMcpTools.cs
└── README.md
```

*Adjust project names or paths above to match your checked-in solution if they differ.*

### How the components interact

```text
Console application
  ├── OpenAI Chat / Agent → Architecture generation
  ├── OpenAI Embeddings → Cosine similarity
  ├── OpenAI Embeddings → SQL Server vector retrieval → Agent response
  └── Agent → Shared .NET tools
                         ↑
                    AISystemDesign.Core
                         ↑
ChatGPT / MCP client → HTTPS MCP endpoint → ASP.NET Core MCP Server
```

**Important:** ChatGPT currently reaches the **three shared engineering tools** through MCP. The Console RAG feature is **not yet exposed as an MCP tool**.

## Technology stack

- C# and .NET; ASP.NET Core for the MCP server
- Microsoft Agent Framework and `Microsoft.Extensions.AI`
- OpenAI .NET SDK (`gpt-4o-mini` for chat, `text-embedding-3-small` for embeddings)
- SQL Server 2025 native `VECTOR(1536)` and `VECTOR_DISTANCE()`
- `Microsoft.Data.SqlClient`
- MCP C# SDK (`ModelContextProtocol.AspNetCore`)

## Getting started

### Prerequisites

- A compatible .NET SDK and Visual Studio 2022 or another .NET IDE
- An OpenAI API key with permissions for **Chat Completions** and **Embeddings** (for Console features)
- SQL Server **2025** for the RAG feature
- An HTTPS-accessible MCP endpoint or secure tunnel **only if connecting remotely**

### 1. Configure local secrets

Set these **user environment variables** on your development machine:

| Variable | Purpose |
| --- | --- |
| `OPENAI_API_KEY` | Calls OpenAI chat and embedding models from the Console App |
| `SQL_CONNECTION_STRING` | Connects the Console RAG feature to SQL Server |

Example **connection string format** for local Windows authentication (replace the server):

```text
Server=YOUR_SERVER_NAME;Database=AISystemDesign;Integrated Security=True;TrustServerCertificate=True;
```

`TrustServerCertificate=True` is meant for local development; use properly validated TLS certificates in production. Restart your IDE after adding environment variables. **Never commit secrets or real connection strings.**

### 2. Prepare the RAG database

Connect to your **SQL Server 2025** instance and create the database/table if they do not already exist:

```sql
CREATE DATABASE AISystemDesign;
GO
USE AISystemDesign;
GO

CREATE TABLE dbo.SystemDesignKnowledge
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Embedding VECTOR(1536) NOT NULL,
    EmbeddingModel VARCHAR(60) NOT NULL
        DEFAULT 'text-embedding-3-small',
    CreatedAt DATETIME2 NOT NULL
        DEFAULT SYSUTCDATETIME()
);
```

Skip objects that already exist. The Console RAG feature lets you add records, generates their embeddings, and retrieves similar records for user questions.

### 3. Build and run

From the solution root:

```bash
dotnet restore
dotnet build
```

To run the learning application:

```bash
dotnet run --project AISystemDesign.Console
```

Console menu:

```text
1. Generate System Architecture
2. Compare System Architectures
3. AI Knowledge Assistant (RAG)
4. AI-Powered Architecture Analysis
0. Exit
```

To run the MCP server **instead**:

```bash
dotnet run --project AISystemDesign.McpServer
```

Check its root health endpoint (if enabled), then use the listening address plus `/mcp` as the MCP endpoint (for example, `http://localhost:5000/mcp`). MCP requests require an MCP client; opening `/mcp` in a browser is not a meaningful protocol test.

## MCP tools

| Tool | Inputs | Output |
| --- | --- | --- |
| `CalculateSystemCapacity` | Daily active users, requests/user/day, peak multiplier | Average and peak requests per second |
| `EstimateStorageRequirements` | Events/day, average event size, retention days, replication factor | Daily, retained, and replicated storage estimates |
| `ValidateArchitecture` | Architecture description | Preliminary safeguard checklist |

Example request to a connected MCP client:

> Design a system for 1 million daily active users, 20 API requests per user per day, and 5× peak load. Estimate storage for 100,000 events per day at 2 KB each, retained for 90 days with three total copies. Review a proposed architecture using Redis, Kafka, and SQL Server.

The client may invoke one or more tools depending on the prompt. **Tool invocation is model-dependent**, and registering a tool does not force every request to use it.

### Remote MCP access

Run `AISystemDesign.McpServer` and expose its `/mcp` endpoint through an appropriately **authenticated HTTPS endpoint or secure tunnel**. Connect a compatible MCP client using that HTTPS URL. The Console App **does not need to be running** for MCP tool calls.

**Security:** Do not expose an unauthenticated development MCP server publicly. Apply authentication/authorization, restrict network access and rate limits, and avoid using sensitive customer data in example knowledge records.

## How RAG works in this project

1. Add architecture knowledge (such as Redis caching or Kafka event handling) through the Console feature.
2. Generate a 1,536-dimensional embedding and store it alongside the text in SQL Server.
3. Convert the user's question into a query embedding.
4. Retrieve nearest records with SQL Server's `VECTOR_DISTANCE('cosine', ...)`.
5. Filter weak matches and pass relevant content to the agent as context.
6. Generate an answer and show which records were retrieved.

**Limitation:** The relevance cutoff is experimental and should be calibrated. Vector similarity does not prove architectural correctness, and the generated response can still be inaccurate.

## Design decisions

- **Separate projects:** Console learning features and HTTP MCP hosting have different responsibilities.
- **Shared Core library:** Capacity, storage, and architecture-checking logic is implemented once and reused.
- **Deterministic calculations:** RPS and storage math run in C# rather than relying on model arithmetic.
- **Provider abstraction:** `IChatClient` separates app-level chat usage from the OpenAI-specific `ChatClient`.
- **Cost awareness:** Uses lower-cost models and avoids regenerating the reference embedding for every similarity comparison.

## Current limitations and next steps

- The architecture validator uses basic keyword/negation rules; it is **not** a formal security or architecture audit.
- The RAG knowledge base currently handles short manually entered records rather than automatic document chunking.
- MCP currently exposes only the **three engineering tools**, not the RAG knowledge search.
- Useful future improvements include unit tests, structured responses, prompt/RAG evaluation, stronger relevance checks, observability, and authorized MCP-based knowledge retrieval.

## Disclaimer

This project is for personal learning and technical demonstration. Capacity and storage figures are estimates based on input assumptions. Review recommendations before applying them to production systems.
