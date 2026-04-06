using System.Text.Json;
using System.Text.Json.Serialization;
using GardenAI.Application.Chat.Abstractions;
using GardenAI.Application.Chat.Contracts.Agentic;
using GardenAI.Application.Chat.Contracts.Completions;
using AppAgenticChatResult = GardenAI.Application.Chat.Contracts.Agentic.AgenticChatResult;
using AppChatCompletionRequest = GardenAI.Application.Chat.Contracts.Completions.ChatCompletionRequest;
using AppChatFunctionCall = GardenAI.Application.Chat.Contracts.Agentic.ChatFunctionCall;
using AppChatHistoryMessage = GardenAI.Application.Chat.Contracts.Agentic.ChatHistoryMessage;
using AppChatToolDefinition = GardenAI.Application.Chat.Contracts.Agentic.ChatToolDefinition;

namespace GardenAI.Presentation.Chat.Services;

/// <summary>Chat assistant backed by Ollama's <c>/api/generate</c> endpoint.</summary>
public sealed class OllamaChatAssistant : GardenAI.Application.Chat.Abstractions.IChatAssistant
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OllamaChatAssistant> _logger;

    /// <summary>Creates a new <see cref="OllamaChatAssistant"/>.</summary>
    public OllamaChatAssistant(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaChatAssistant> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string> GetReplyAsync(AppChatCompletionRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Prompt))
            throw new ArgumentException("Prompt must not be empty.", nameof(request));

        var model = _configuration["Ollama:Model"] ?? "llama3.2:3b";
        var messages = new List<OllamaMessage> { new("system", request.SystemPrompt) };

        foreach (var h in request.History)
        {
            if (string.IsNullOrWhiteSpace(h.Content)) continue;
            var role = h.Role is "system" or "user" or "assistant" ? h.Role : "user";
            messages.Add(new OllamaMessage(role, h.Content));
        }
        messages.Add(new OllamaMessage("user", request.Prompt));

        return await CallOllamaForTextAsync(model, messages, [], ct);
    }

    /// <inheritdoc/>
    public async Task<AppAgenticChatResult> GetAgenticReplyAsync(
        string systemPrompt,
        IReadOnlyList<AppChatHistoryMessage> history,
        string userMessage,
        IReadOnlyList<AppChatToolDefinition> tools,
        Func<AppChatFunctionCall, CancellationToken, Task<string>> toolExecutor,
        int maxIterations = 5,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(systemPrompt);
        ArgumentNullException.ThrowIfNull(history);
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);
        ArgumentNullException.ThrowIfNull(tools);
        ArgumentNullException.ThrowIfNull(toolExecutor);

        var model = _configuration["Ollama:Model"] ?? "llama3.2:3b";
        var ollamaTools = tools.Select(MapToOllamaTool).ToList();
        var executedCalls = new List<AppChatFunctionCall>();

        // Build working message list: system + history + new user message
        var messages = new List<OllamaMessage> { new("system", systemPrompt) };
        foreach (var h in history)
        {
            if (!string.IsNullOrWhiteSpace(h.Content))
                messages.Add(new OllamaMessage(h.Role, h.Content));
        }
        messages.Add(new OllamaMessage("user", userMessage));

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var payload = new OllamaChatRequest(model, messages, ollamaTools, Stream: false);

            _logger.LogInformation(
                "Agentic Ollama call (iter {Iter}/{Max}), model={Model}, messages={Count}, tools={Tools}.",
                iteration + 1, maxIterations, model, messages.Count, tools.Count);

            using var response = await _httpClient.PostAsJsonAsync("api/chat", payload, JsonOptions, ct)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                _logger.LogError("Ollama call failed {Status}. Body: {Body}", (int)response.StatusCode, body);
                throw new InvalidOperationException($"Ollama request failed with status {(int)response.StatusCode}.");
            }

            var result = await response.Content
                .ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, ct)
                .ConfigureAwait(false);

            if (result?.Message is null)
                throw new InvalidOperationException("Ollama returned a null message.");

            // ── Tool calls? Execute each and loop back ──────────────────
            if (result.Message.ToolCalls is { Count: > 0 })
            {
                // Add the assistant's tool-call turn to context
                messages.Add(result.Message);

                foreach (var tc in result.Message.ToolCalls)
                {
                    var funcName = tc.Function?.Name ?? string.Empty;
                    var argsJson = tc.Function?.Arguments is not null
                        ? JsonSerializer.Serialize(tc.Function.Arguments, JsonOptions)
                        : "{}";

                    var call = new AppChatFunctionCall(funcName, argsJson);
                    executedCalls.Add(call);

                    _logger.LogInformation("Executing tool call: {FunctionName}({Args}).", funcName, argsJson);
                    var toolResult = await toolExecutor(call, ct).ConfigureAwait(false);

                    // Feed result back as a "tool" role message
                    messages.Add(new OllamaMessage("tool", toolResult));
                }

                continue; // Ask Ollama again with the tool results
            }

            // ── Text response — we're done ───────────────────────────────
            var text = result.Message.Content?.Trim() ?? string.Empty;
            return new AppAgenticChatResult(text, executedCalls.AsReadOnly());
        }

        _logger.LogWarning("Agentic loop hit max iterations ({Max}); returning partial result.", maxIterations);
        return new AppAgenticChatResult(
            "I've processed your request but ran out of reasoning steps. Please try again.",
            executedCalls.AsReadOnly());
    }

    // ── Shared helper ─────────────────────────────────────────────────────

    private async Task<string> CallOllamaForTextAsync(
        string model,
        List<OllamaMessage> messages,
        List<OllamaTool> tools,
        CancellationToken ct)
    {
        var payload = new OllamaChatRequest(model, messages, tools, Stream: false);

        _logger.LogInformation("Sending prompt to Ollama model {Model}.", model);
        using var response = await _httpClient.PostAsJsonAsync("api/chat", payload, JsonOptions, ct)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            _logger.LogError("Ollama call failed {Status}. Body: {Body}", (int)response.StatusCode, body);
            throw new InvalidOperationException($"Ollama request failed with status {(int)response.StatusCode}.");
        }

        var result = await response.Content
            .ReadFromJsonAsync<OllamaChatResponse>(JsonOptions, ct)
            .ConfigureAwait(false);

        if (result?.Message is null || string.IsNullOrWhiteSpace(result.Message.Content))
            throw new InvalidOperationException("Ollama returned an empty response.");

        return result.Message.Content.Trim();
    }

    // ── Mapping helpers ───────────────────────────────────────────────────

    private static OllamaTool MapToOllamaTool(AppChatToolDefinition def)
    {
        var props = def.Properties.ToDictionary(
            kv => kv.Key,
            kv => (object)new
            {
                type = kv.Value.Type,
                description = kv.Value.Description,
                @enum = kv.Value.Enum,
            });

        return new OllamaTool("function", new OllamaToolFunction(
            def.Name,
            def.Description,
            new OllamaToolParameters("object", props, def.Required.ToList())));
    }

    // ── Ollama wire types (internal) ──────────────────────────────────────

    private sealed record OllamaChatRequest(
        string Model,
        IReadOnlyList<OllamaMessage> Messages,
        [property: JsonPropertyName("tools")] IReadOnlyList<OllamaTool> Tools,
        bool Stream = false);

    private sealed record OllamaMessage(
        string Role,
        string Content,
        [property: JsonPropertyName("tool_calls")] IReadOnlyList<OllamaToolCallMessage> ToolCalls = null);

    private sealed record OllamaChatResponse(OllamaMessage Message, bool Done);

    private sealed record OllamaTool(string Type, OllamaToolFunction Function);

    private sealed record OllamaToolFunction(
        string Name,
        string Description,
        OllamaToolParameters Parameters);

    private sealed record OllamaToolParameters(
        string Type,
        IDictionary<string, object> Properties,
        IReadOnlyList<string> Required);

    private sealed record OllamaToolCallMessage(OllamaToolCallFunction Function);

    private sealed record OllamaToolCallFunction(
        string Name,
        [property: JsonPropertyName("arguments")] JsonElement? Arguments);
}

