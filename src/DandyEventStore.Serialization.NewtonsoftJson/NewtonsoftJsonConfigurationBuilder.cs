using Newtonsoft.Json;

namespace DandyEventStore.Serialization.NewtonsoftJson;

public sealed class NewtonsoftJsonConfigurationBuilder
{
    public JsonSerializerSettings Settings { get; set; } = new();

    internal NewtonsoftJsonConfiguration Build()
    {
        return new NewtonsoftJsonConfiguration
        {
            Settings = Settings,
        };
    }
}