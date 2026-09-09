using System.Text.Json;
using DandyEventStore.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Serialization.SystemTextJson;

internal sealed class SystemTextJsonConfiguration : PluginConfiguration
{
    public override string Slot => "serialization";

    public required JsonSerializerOptions Options { get; init; }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ISerializer, SystemTextJsonSerializer>();
        services.AddSingleton(this);
    }
}