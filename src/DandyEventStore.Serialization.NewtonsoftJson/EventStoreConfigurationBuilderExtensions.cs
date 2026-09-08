using DandyEventStore.Configuration;

namespace DandyEventStore.Serialization.NewtonsoftJson;

public static class EventStoreConfigurationBuilderExtensions
{
    public static EventStoreConfigurationBuilder UseNewtonsoftJson(this EventStoreConfigurationBuilder builder, Action<NewtonsoftJsonConfigurationBuilder>? builderAction = null)
    {
        var newtonSoftJsonBuilder = new NewtonsoftJsonConfigurationBuilder();
        builderAction?.Invoke(newtonSoftJsonBuilder);
        var configuration = newtonSoftJsonBuilder.Build();

        return builder.UsePlugin(configuration);
    }
}