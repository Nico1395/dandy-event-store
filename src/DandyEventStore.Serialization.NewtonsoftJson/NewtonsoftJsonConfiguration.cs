using DandyEventStore.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DandyEventStore.Serialization.NewtonsoftJson;

internal sealed class NewtonsoftJsonConfiguration : PluginConfiguration
{
    public override string Slot => "serialization";
    
    public required JsonSerializerSettings Settings { get; init; }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISerializer, NewtonsoftJsonSerializer>();
        services.AddSingleton(this);
    }
}