using AISystemDesign.Core;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISystemDesign.ConsoleApp.Features
{
    public static class ToolAssistedSystemDesignAgent
    {
        public static async Task AnalyzeArchitectureAsync(
            string apiKey)
        {
            Console.WriteLine(
                "\n=== MULTI-TOOL CALLING ===");

            // Step 1: Create OpenAI Chat Client
            IChatClient chatClient =
                new ChatClient("gpt-4o-mini", apiKey)
                    .AsIChatClient();

            // Step 2: Create Agent with three tools
            AIAgent agent = chatClient.AsAIAgent(
                name: "SystemDesignAgent",

                instructions: """
            You are a Senior Software Architect
            specializing in .NET and distributed systems.

            Help users design scalable, reliable,
            secure and cost-effective systems.

            You have access to three C# tools:

            1. CalculateSystemCapacity
               Use when daily active users,
               requests per user per day and
               peak multiplier are provided.

            2. EstimateStorageRequirements
               Use when daily event count,
               event size, retention period
               and replication factor are provided.

            3. ValidateArchitecture
               Use when the user describes a
               proposed software architecture.

            Important rules:

            - Call relevant tools when inputs
              are available.
            - You may call multiple tools for
              a single user request.
            - Never invent missing numeric inputs.
            - Ask for missing required inputs
              if a calculation is requested.
            - Pass the user's proposed architecture
              to the validation tool without
              adding unmentioned components.
            - Explain calculated results clearly.
            - Treat architecture validation as
              a preliminary text-based review.
            - Do not claim an unspecified control
              is definitely absent.
            - Do not invent tool results.
            - Explain assumptions and trade-offs.
            - Keep the final response structured.
            """,

                // Step 3: Register C# tools
                tools:
                [
                    AIFunctionFactory.Create(
                SystemDesignTools.CalculateSystemCapacity),

            AIFunctionFactory.Create(
                SystemDesignTools.EstimateStorageRequirements),

            AIFunctionFactory.Create(
                SystemDesignTools.ValidateArchitecture)
                ]
            );

            // Step 4: Accept user requirements
            Console.WriteLine(
                "\nDescribe your system requirements:");

            string userPrompt = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                Console.WriteLine(
                    "System requirements cannot be empty.");
                return;
            }

            // Step 5: Execute agent
            Console.WriteLine(
                "\nAnalyzing system requirements...\n");

            var response = await agent.RunAsync(userPrompt);

            // Step 6: Display final response
            Console.WriteLine(
                "\n=== SYSTEM DESIGN ANALYSIS ===\n");

            Console.WriteLine(response.Text);
        }
    }
}
