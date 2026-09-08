using Newtonsoft.Json;

namespace DandyEventStore.Serialization.NewtonsoftJson;

public sealed class NewtonsoftJsonConfigurationBuilder
{
    public JsonSerializerSettings Settings { get; set; } = new();

    internal NewtonsoftJsonPluginConfiguration Build()
    {
        return new NewtonsoftJsonPluginConfiguration
        {
            Settings = Settings,
        };
    }
}