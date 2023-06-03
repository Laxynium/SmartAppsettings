using System.Collections.ObjectModel;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates.Tests;

public class TemplatesFindingTests
{
    [Fact]
    public void finds_templates_on_any_level()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {"A", new Dictionary<string, object>
            {
                {"a", "v1"},
                {"b", "v2"},
                {"$.c", "t1"},
                {"d", new Dictionary<string , object>
                {
                    {"1", new Dictionary<string, object>
                    {
                        {"$.a", "t3"}
                    }}
                }}
            }},
            {"$.B", "t2"}
        });
        
        var result = FindTemplates(config, "$.");

        result.Should().HaveCount(3);
        result.Should().Contain(x => x.Key == "$.B");
        result.Should().Contain(x => x.Key == "$.c");
        result.Should().Contain(x => x.Key == "$.a");
    }

    [Fact]
    public void empty_when_templates_are_not_found()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {"A", new Dictionary<string, object>
            {
                {"a", "v1"},
                {"b", "v2"},
                {"c", "t1"},
                {"d", new Dictionary<string , object>
                {
                    {"1", new Dictionary<string, object>
                    {
                        {"a", "t3"}
                    }}
                }}
            }},
            {"B", "t2"}
        });
 
        var result = FindTemplates(config, "$.");

        result.Should().BeEmpty();
    }

    [Fact]
    public void shortest_possible_template_marker()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {"$.", "t1"}
        });
        
        var result = FindTemplates(config, "$.");

        result.Should().HaveCount(1);

    }

    [Fact]
    public void empty_config()
    {
        var config = AConfig(new Dictionary<string, object>());
        
        var result = FindTemplates(config, "$.");

        result.Should().BeEmpty();
    }

    private static ReadOnlyCollection<IConfigurationSection> FindTemplates(Stream config, string templateMarker)
    {
        var configuration = AConfigurationRoot(config);
        return TemplatesFinder.FindTemplates(configuration, x => x.StartsWith($"{templateMarker}"));
    }

    private static Stream AConfig(Dictionary<string, object> dictionary)
    {
        var memoryStream = new MemoryStream();
        JsonSerializer.Serialize(memoryStream, dictionary);
        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    private static IConfigurationRoot AConfigurationRoot(Stream config)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(config)
            .Build();
        return configuration;
    }
}