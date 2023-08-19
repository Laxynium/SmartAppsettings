using System.Collections.ObjectModel;
using System.Text.Json;
using SmartAppsettings;
using Microsoft.Extensions.Configuration;

namespace SmartAppsettings.Tests;

public class TestsBase
{
    protected static ReadOnlyCollection<IConfigurationSection> FindTemplates(Stream config, string templateMarker)
    {
        var configuration = AConfigurationRoot(config);
        return ReferencesFinder.FindTemplates(configuration, x => x.StartsWith($"{templateMarker}"));
    }

    protected static Stream AConfig(Dictionary<string, object> dictionary)
    {
        var memoryStream = new MemoryStream();
        JsonSerializer.Serialize(memoryStream, dictionary);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    protected static IConfigurationRoot AConfigurationRoot(Stream config)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(config)
            .Build();
        return configuration;
    }
}