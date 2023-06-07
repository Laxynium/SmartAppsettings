using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates;

public static partial class Extensions
{
    public static IConfigurationBuilder AddReferencesReplacedSource(this ConfigurationManager configuration)
    {
        var result = ConfigurationReferencesReplacer.Process(configuration);

        configuration.AddInMemoryCollection(result!);

        return configuration;
    }
}