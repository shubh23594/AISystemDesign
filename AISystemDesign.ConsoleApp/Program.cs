
using AISystemDesign.ConsoleApp.Features;

namespace AISystemDesign.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Read API key from Windows Environment Variables
            string? apiKey = Environment
                .GetEnvironmentVariable("OPENAI_API_KEY");

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine(
                    "Error: OPENAI_API_KEY is missing.");
                return;
            }

            while (true)
            {
                Console.WriteLine(
                    "\n========== AI SYSTEM DESIGN ==========");

                Console.WriteLine("1. Generate System Architecture");
                Console.WriteLine("2. Compare System Architectures");
                Console.WriteLine("3. AI Knowledge Assistant – SQL Server RAG");
                Console.WriteLine("4. Multi-Tool Calling – AI System Design Assistant");
                Console.WriteLine("0. Exit");

                Console.Write("\nSelect an option: ");
                string? choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            await SystemDesignGenerator.GenerateSystemDesignAsync(apiKey);
                            break;

                        case "2":
                            await ArchitectureSimilarityAnalyzer.CompareArchitecturesAsync(apiKey);
                            break;

                        case "3":
                            await SystemDesignRagAssistant.RunKnowledgeSearchAsync(apiKey);
                            break;

                        case "4":
                            await ToolAssistedSystemDesignAgent.AnalyzeArchitectureAsync(apiKey);
                            break;

                        case "0":
                            Console.WriteLine("Application closed.");
                            return;

                        default:
                            Console.WriteLine("Invalid option.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"\nError: {ex.Message}");
                }
            }
        }
    }
}
