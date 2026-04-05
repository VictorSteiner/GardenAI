using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Abstractions;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Contracts;
using HomeAssistant.Infrastructure.HomeAssistant.Protocol.Functions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace HomeAssistant.Infrastructure.HomeAssistant.Protocol.Services;

/// <summary>Discovers Semantic Kernel attributed tool methods and exposes them to the garden planner.</summary>
public sealed class HomeAssistantProtocolToolRegistry : IHomeAssistantProtocolToolRegistry
{
    private readonly IReadOnlyDictionary<string, ToolDescriptor> _tools;
    private readonly IReadOnlyList<HomeAssistantProtocolToolDefinition> _definitions;
    private readonly ILogger<HomeAssistantProtocolToolRegistry> _logger;

    /// <summary>Creates a new <see cref="HomeAssistantProtocolToolRegistry"/>.</summary>
    public HomeAssistantProtocolToolRegistry(
        GardenPlannerKernelFunctions gardenPlannerKernelFunctions,
        ILogger<HomeAssistantProtocolToolRegistry> logger)
    {
        ArgumentNullException.ThrowIfNull(gardenPlannerKernelFunctions);
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var descriptors = DiscoverTools(gardenPlannerKernelFunctions).ToList();
        _tools = descriptors.ToDictionary(descriptor => descriptor.Name, StringComparer.OrdinalIgnoreCase);
        _definitions = descriptors
            .Select(descriptor => new HomeAssistantProtocolToolDefinition(descriptor.Name, descriptor.Description, descriptor.Parameters))
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("Registered {ToolCount} Semantic Kernel protocol tools.", _definitions.Count);
    }

    /// <inheritdoc/>
    public IReadOnlyList<HomeAssistantProtocolToolDefinition> GetToolDefinitions() => _definitions;

    /// <inheritdoc/>
    public async Task<string> InvokeAsync(string functionName, string argumentsJson, CancellationToken ct = default)
    {
        if (!_tools.TryGetValue(functionName, out var descriptor))
        {
            return $"Unknown tool '{functionName}'.";
        }

        try
        {
            var arguments = BuildArgumentArray(descriptor.Method, argumentsJson, ct);
            var invocationResult = descriptor.Method.Invoke(descriptor.Target, arguments);
            return await AwaitStringResultAsync(invocationResult).ConfigureAwait(false);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            _logger.LogError(ex.InnerException, "Protocol tool {ToolName} failed.", functionName);
            return $"Error in {functionName}: {ex.InnerException.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Protocol tool {ToolName} failed.", functionName);
            return $"Error in {functionName}: {ex.Message}";
        }
    }

    private static IEnumerable<ToolDescriptor> DiscoverTools(object target)
    {
        var targetType = target.GetType();
        foreach (var method in targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public))
        {
            var kernelAttribute = method.GetCustomAttribute<KernelFunctionAttribute>();
            if (kernelAttribute is null)
            {
                continue;
            }

            var name = string.IsNullOrWhiteSpace(kernelAttribute.Name)
                ? method.Name
                : kernelAttribute.Name;
            var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? name;
            var parameters = method.GetParameters()
                .Where(parameter => parameter.ParameterType != typeof(CancellationToken))
                .Select(parameter => new HomeAssistantProtocolToolParameter(
                    parameter.Name ?? string.Empty,
                    MapType(parameter.ParameterType),
                    parameter.GetCustomAttribute<DescriptionAttribute>()?.Description ?? parameter.Name ?? string.Empty,
                    !parameter.HasDefaultValue && (!IsNullable(parameter.ParameterType))))
                .ToList()
                .AsReadOnly();

            yield return new ToolDescriptor(name, description, target, method, parameters);
        }
    }

    private static object?[] BuildArgumentArray(MethodInfo method, string argumentsJson, CancellationToken ct)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson);
        var root = document.RootElement;
        var arguments = new object?[method.GetParameters().Length];

        for (var index = 0; index < method.GetParameters().Length; index++)
        {
            var parameter = method.GetParameters()[index];
            if (parameter.ParameterType == typeof(CancellationToken))
            {
                arguments[index] = ct;
                continue;
            }

            if (root.ValueKind == JsonValueKind.Object && parameter.Name is not null && root.TryGetProperty(parameter.Name, out var property))
            {
                arguments[index] = ConvertJsonValue(property, parameter.ParameterType);
                continue;
            }

            if (parameter.HasDefaultValue)
            {
                arguments[index] = parameter.DefaultValue;
                continue;
            }

            if (IsNullable(parameter.ParameterType))
            {
                arguments[index] = null;
                continue;
            }

            throw new InvalidOperationException($"Required argument '{parameter.Name}' was not supplied.");
        }

        return arguments;
    }

    private static async Task<string> AwaitStringResultAsync(object? invocationResult)
    {
        switch (invocationResult)
        {
            case null:
                return string.Empty;
            case string text:
                return text;
            case Task<string> stringTask:
                return await stringTask.ConfigureAwait(false);
            case Task task:
                await task.ConfigureAwait(false);
                var resultProperty = task.GetType().GetProperty("Result");
                return resultProperty?.GetValue(task)?.ToString() ?? string.Empty;
            default:
                return invocationResult.ToString() ?? string.Empty;
        }
    }

    private static object? ConvertJsonValue(JsonElement element, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
        {
            return element.ValueKind == JsonValueKind.Null ? null : element.GetString();
        }

        if (underlyingType == typeof(int))
        {
            return element.GetInt32();
        }

        if (underlyingType == typeof(bool))
        {
            return element.GetBoolean();
        }

        if (underlyingType == typeof(DateTimeOffset))
        {
            return element.GetDateTimeOffset();
        }

        if (underlyingType == typeof(Guid))
        {
            return element.GetGuid();
        }

        return JsonSerializer.Deserialize(element.GetRawText(), targetType, new JsonSerializerOptions(JsonSerializerDefaults.Web));
    }

    private static string MapType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
        if (underlyingType == typeof(int))
        {
            return "integer";
        }

        if (underlyingType == typeof(bool))
        {
            return "boolean";
        }

        if (underlyingType == typeof(DateTimeOffset) || underlyingType == typeof(Guid))
        {
            return "string";
        }

        return "string";
    }

    private static bool IsNullable(Type type)
        => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

    private sealed record ToolDescriptor(
        string Name,
        string Description,
        object Target,
        MethodInfo Method,
        IReadOnlyList<HomeAssistantProtocolToolParameter> Parameters);
}

