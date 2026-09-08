using System.Text.Json;

namespace DandyEventStore.Serialization.SystemTextJson;

public sealed class SystemTextJsonConfigurationBuilder
{
    public JsonSerializerOptions Options { get; set; } = new();

    internal SystemTextJsonConfiguration Build()
    {
        return new SystemTextJsonConfiguration
        {
            Options = Options,
        };
    }
}