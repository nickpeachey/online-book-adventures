using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using OnlineBookAdventures.Application.Common.Interfaces;

namespace OnlineBookAdventures.Infrastructure.Services;

/// <summary>
/// Implements <see cref="IStoryGenerationService"/> using the OpenAI chat completions API.
/// Uses a two-phase approach for full story generation:
///   1. Generate the story structure (nodes + choices outline).
///   2. Expand each node with rich narrative content in parallel.
/// </summary>
public sealed class OpenAIStoryGenerationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    : IStoryGenerationService
{
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";
    private const string Model = "gpt-4o";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc/>
    public async Task<GeneratedStoryGraph> GenerateFullStoryAsync(string prompt, CancellationToken cancellationToken = default)
    {
        // ── Phase 1: Generate the story outline (structure only) ─────────────
        const string structureSystemPrompt =
            "You are an expert CYOA (choose your own adventure) story architect. " +
            "Generate a large, branching story structure as JSON. " +
            "Respond with ONLY valid JSON matching this schema exactly: " +
            "{ \"title\": string, \"description\": string, " +
            "\"nodes\": [{\"title\": string, \"summary\": string, \"isStart\": bool, \"isEnd\": bool, " +
            "\"positionX\": number, \"positionY\": number}], " +
            "\"choices\": [{\"fromNodeIndex\": number, \"toNodeIndex\": number, \"label\": string, \"order\": number}] }. " +
            "Rules: " +
            "- Exactly 1 start node (isStart:true). " +
            "- At least 3 distinct end nodes (isEnd:true) with different outcomes. " +
            "- 15 to 20 nodes total to create a rich, branching narrative. " +
            "- Every non-end node must have at least 2 choices leading to different nodes. " +
            "- No orphan nodes; every node must be reachable from the start. " +
            "- 'summary' is 1-2 sentences describing what happens in this node (will be expanded later). " +
            "- Layout nodes in a readable grid: positionX increments of 300, positionY increments of 200, " +
            "  start node at (0,0), branches spread horizontally. " +
            "- Make the story compelling with meaningful choices that affect the outcome.";

        var structureJson = await SendChatRequestAsync(
            structureSystemPrompt, prompt, temperature: 0.8, maxTokens: 4000, cancellationToken)
            .ConfigureAwait(false);

        var outline = TryParseOutline(structureJson)
            ?? throw new InvalidOperationException("Failed to parse AI-generated story structure.");

        // ── Phase 2: Expand each node's content in parallel ──────────────────
        const string contentSystemPrompt =
            "You are a skilled narrative author writing for a CYOA story. " +
            "Given the story title, a node title, and a brief summary of what happens, " +
            "write immersive, engaging narrative content for that story node. " +
            "Write in second person ('You...'). Use vivid descriptions and build atmosphere. " +
            "Respond with ONLY the narrative text — no titles, headings, or metadata. " +
            "Write 2-3 substantial paragraphs (approximately 200-300 words).";

        var expandedContents = await ExpandNodesInParallelAsync(
            outline.Title, outline.Nodes, contentSystemPrompt, cancellationToken)
            .ConfigureAwait(false);

        // ── Assemble final graph ──────────────────────────────────────────────
        var nodes = outline.Nodes.Select((n, i) =>
            new GeneratedStoryNode(n.Title, expandedContents[i], n.IsStart, n.IsEnd, n.PositionX, n.PositionY))
            .ToList();

        var choices = outline.Choices.Select(c =>
            new GeneratedStoryChoice(c.FromNodeIndex, c.ToNodeIndex, c.Label, c.Order))
            .ToList();

        return new GeneratedStoryGraph(outline.Title, outline.Description, nodes, choices);
    }

    /// <inheritdoc/>
    public async Task<string> SuggestNodeContentAsync(
        string storyTitle,
        string nodeTitle,
        string currentContent,
        CancellationToken cancellationToken = default)
    {
        const string systemPrompt =
            "You are a creative writer for CYOA (choose your own adventure) stories. Given a story title, " +
            "node title, and optional existing content, suggest engaging narrative content for this story node. " +
            "Write in second person ('You...'). Respond with ONLY the narrative text (3-5 sentences), no explanations or metadata.";

        var userPrompt =
            $"Story: {storyTitle}\nNode: {nodeTitle}\nExisting content: {currentContent}\n\nWrite engaging narrative content for this node:";

        return await SendChatRequestAsync(systemPrompt, userPrompt, temperature: 0.9, maxTokens: 500, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<string[]> ExpandNodesInParallelAsync(
        string storyTitle,
        List<OutlineNodeDto> nodes,
        string systemPrompt,
        CancellationToken cancellationToken)
    {
        // Expand up to 10 nodes concurrently to avoid rate limits
        var semaphore = new SemaphoreSlim(10, 10);
        var tasks = nodes.Select(async (node, i) =>
        {
            await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var userPrompt =
                    $"Story: \"{storyTitle}\"\n" +
                    $"Node title: \"{node.Title}\"\n" +
                    $"Summary: {node.Summary}\n\n" +
                    $"Write the full narrative content for this node:";

                return await SendChatRequestAsync(systemPrompt, userPrompt, temperature: 0.85, maxTokens: 450, cancellationToken)
                    .ConfigureAwait(false);
            }
            finally
            {
                semaphore.Release();
            }
        });

        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task<string> SendChatRequestAsync(
        string systemPrompt,
        string userPrompt,
        double temperature,
        int maxTokens,
        CancellationToken cancellationToken)
    {
        var apiKey = configuration["OpenAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key is not configured.");

        using var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        client.Timeout = TimeSpan.FromMinutes(5);

        var requestBody = new
        {
            model = Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            },
            temperature,
            max_tokens = maxTokens
        };

        var response = await client
            .PostAsJsonAsync(ApiUrl, requestBody, JsonOptions, cancellationToken)
            .ConfigureAwait(false);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content
            .ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return responseJson
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    private static OutlineDto? TryParseOutline(string json)
    {
        try
        {
            var trimmed = StripMarkdownFences(json);
            return JsonSerializer.Deserialize<OutlineDto>(trimmed, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static GeneratedStoryGraph? TryParseGraph(string json)
    {
        try
        {
            var trimmed = StripMarkdownFences(json);
            var doc = JsonSerializer.Deserialize<OpenAIStoryGraphDto>(trimmed, JsonOptions);
            if (doc is null) return null;

            var nodes = doc.Nodes.Select(n =>
                new GeneratedStoryNode(n.Title, n.Content, n.IsStart, n.IsEnd, n.PositionX, n.PositionY))
                .ToList();

            var choices = doc.Choices.Select(c =>
                new GeneratedStoryChoice(c.FromNodeIndex, c.ToNodeIndex, c.Label, c.Order))
                .ToList();

            return new GeneratedStoryGraph(doc.Title, doc.Description, nodes, choices);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string StripMarkdownFences(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("```", StringComparison.Ordinal)) return trimmed;
        var firstNewline = trimmed.IndexOf('\n');
        var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
        if (firstNewline >= 0 && lastFence > firstNewline)
            trimmed = trimmed[(firstNewline + 1)..lastFence].Trim();
        return trimmed;
    }

    // ── Internal DTO types ────────────────────────────────────────────────────

    private sealed record OutlineDto(
        string Title,
        string Description,
        List<OutlineNodeDto> Nodes,
        List<OutlineChoiceDto> Choices);

    private sealed record OutlineNodeDto(
        string Title,
        string Summary,
        bool IsStart,
        bool IsEnd,
        double PositionX,
        double PositionY);

    private sealed record OutlineChoiceDto(
        int FromNodeIndex,
        int ToNodeIndex,
        string Label,
        int Order);

    private sealed record OpenAIStoryGraphDto(
        string Title,
        string Description,
        List<OpenAINodeDto> Nodes,
        List<OpenAIChoiceDto> Choices);

    private sealed record OpenAINodeDto(
        string Title,
        string Content,
        bool IsStart,
        bool IsEnd,
        double PositionX,
        double PositionY);

    private sealed record OpenAIChoiceDto(
        int FromNodeIndex,
        int ToNodeIndex,
        string Label,
        int Order);
}
