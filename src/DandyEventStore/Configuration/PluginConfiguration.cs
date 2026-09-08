using Microsoft.Extensions.DependencyInjection;

namespace DandyEventStore.Configuration;

public abstract class PluginConfiguration
{
    public abstract string Slot { get; }

    public abstract void ConfigureServices(IServiceCollection services);
}