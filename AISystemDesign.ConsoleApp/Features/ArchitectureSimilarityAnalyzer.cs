using OpenAI.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AISystemDesign.ConsoleApp.Features
{


    public static class ArchitectureSimilarityAnalyzer
    {

        // ================================================
        // Compare Architectures (SYSTEM DESIGN EMBEDDING)
        // ================================================

        public static async Task CompareArchitecturesAsync(
            string apiKey)
        {
            Console.WriteLine(
                "\n=== SYSTEM DESIGN SIMILARITY ===");

            // Step 1: Create Embedding Client
            EmbeddingClient embeddingClient = new(
                model: "text-embedding-3-small",
                apiKey: apiKey
            );

            // Step 2: Take reference architecture from user
            Console.WriteLine(
                "\nEnter your Reference System Design:");

            string? referenceDesign = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(referenceDesign))
            {
                Console.WriteLine(
                    "Reference design cannot be empty.");
                return;
            }

            // Step 3: Generate reference embedding once
            var referenceEmbedding = await embeddingClient
                .GenerateEmbeddingAsync(referenceDesign);

            float[] referenceVector = referenceEmbedding.Value
                .ToFloats()
                .ToArray();

            Console.WriteLine(
                "\nReference embedding generated successfully.");

            // Step 4: Compare multiple designs
            while (true)
            {
                Console.WriteLine(
                    "\nEnter System Design to compare:");
                Console.WriteLine(
                    "(Type 'exit' to return to main menu)");

                string? inputDesign = Console.ReadLine()?.Trim();

                if (string.Equals(
                    inputDesign,
                    "exit",
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(inputDesign))
                {
                    Console.WriteLine(
                        "System design cannot be empty.");
                    continue;
                }

                // Step 5: Generate comparison embedding
                var inputEmbedding = await embeddingClient
                    .GenerateEmbeddingAsync(inputDesign);

                float[] inputVector = inputEmbedding.Value
                    .ToFloats()
                    .ToArray();

                // Step 6: Calculate cosine similarity
                double similarity = CalculateCosineSimilarity(
                    referenceVector,
                    inputVector
                );

                // Step 7: Display result
                Console.WriteLine(
                    $"\nCosine Similarity: {similarity:F4}");

                Console.WriteLine(
                    "Score measures semantic similarity, " +
                    "not architectural correctness.");
            }
        }



        // ==========================================
        // HELPER: COSINE SIMILARITY
        // ==========================================
        private static double CalculateCosineSimilarity(
            float[] vector1,
            float[] vector2)
        {
            if (vector1.Length != vector2.Length)
            {
                throw new ArgumentException(
                    "Vector dimensions must match.");
            }

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                double a = vector1[i];
                double b = vector2[i];

                dotProduct += a * b;
                magnitude1 += a * a;
                magnitude2 += b * b;
            }

            double denominator =
                Math.Sqrt(magnitude1) *
                Math.Sqrt(magnitude2);

            if (denominator == 0)
                return 0;

            return Math.Clamp(
                dotProduct / denominator,
                -1.0,
                1.0);
        }
    }
}
