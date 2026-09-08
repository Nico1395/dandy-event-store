using DandyEventStore.Configuration;

namespace DandyEventStore.Serialization.SystemTextJson;

public static class EventStoreConfigurationBuilderExtensions
{
    public static EventStoreConfigurationBuilder UseSystemTextJson(this EventStoreConfigurationBuilder builder, Action<SystemTextJsonConfigurationBuilder>? builderAction = null)
    {
        var systemTextJsonBuilder = new SystemTextJsonConfigurationBuilder();
        builderAction?.Invoke(systemTextJsonBuilder);
        var configuration = systemTextJsonBuilder.Build();

        return builder.UsePlugin(configuration);
    }
}