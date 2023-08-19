using FluentAssertions;

namespace ConfigurationTemplates.Tests.Internal;

public class TemplatesFindingTests : TestsBase
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
        result.Should().Contain(x => x.Key == "$.B" && x.Path == "$.B");
        result.Should().Contain(x => x.Key == "$.c" && x.Path == "A:$.c");
        result.Should().Contain(x => x.Key == "$.a" && x.Path == "A:d:1:$.a");
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
}