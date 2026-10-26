using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace AISystemDesign.ConsoleApp.Features
{
    public static class SystemDesignGenerator
    {

        // ==========================================
        //  Generate SystemDesign
        // ==========================================
        public static async Task GenerateSystemDesignAsync(
            string apiKey)
        {
            Console.WriteLine(
                "\n=== SYSTEM DESIGN AGENT ===");

            // Step 1: Create Chat Client
            string modelId = "gpt-4o-mini";

            IChatClient chatClient =
                new ChatClient(modelId, apiKey)
                    .AsIChatClient();

            // Step 2: Create AI Agent
            AIAgent agent = chatClient.AsAIAgent(
                name: "SystemDesignAgent",
                instructions: """
                    You are a Senior Software Architect
                    specializing in .NET and distributed systems.

                    Analyze the user's requirements and
                    recommend a practical system design.

                    Cover the following:

                    1. Functional Requirements
                    2. Non-Functional Requirements
                    3. High-Level Architecture
                    4. Core Services and Responsibilities
                    5. REST API Design
                    6. Database Design
                    7. Caching Strategy
                    8. Scalability and Performance
                    9. Security and Reliability
                    10. Technical Trade-offs

                    Guidelines:
                    - Prefer .NET and C# technologies.
                    - Explain why technologies are chosen.
                    - Recommend microservices only if needed.
                    - Clearly state assumptions.
                    - Do not invent capacity measurements.
                    - Use simple, professional English.
                    - Keep responses concise.
                    - Use short bullet points.
                    """
            );

            // Step 3: Accept system requirements
            Console.WriteLine(
                "\nEnter your system requirements:");

            string? userPrompt = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userPrompt))
            {
                Console.WriteLine(
                    "Requirements cannot be empty.");
                return;
            }

            // Step 4: Execute Agent
            Console.WriteLine(
                "\nGenerating system design...\n");

            var response = await agent.RunAsync(userPrompt);

            // Step 5: Display result
            Console.WriteLine(
                "\n========== SYSTEM DESIGN ==========\n");

            Console.WriteLine(response.Text);
        }
    }
}
