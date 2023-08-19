using SmartAppsettings;
using FluentAssertions;

namespace SmartAppsettings.Tests.Internal;

public class ReferenceReplacingTests : TestsBase
{
    [Fact]
    public void fails_when_referencing_a_missing_section()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    { "Postgres", "SomeConnectionString" }
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result =
            replacer.Replace(new ConfigReference("ModuleA:Db:$.ConnectionString", "{ConnectionStrings:Mssql}"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReferenceReplacer.ReplaceError.SectionNotFound(string.Empty));
    }

    [Fact]
    public void fails_when_referencing_a_section_with_object()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    { "Postgres", "SomeConnectionString" }
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result = replacer.Replace(new ConfigReference("ModuleA:Db:$.ConnectionString", "{ConnectionStrings}"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReferenceReplacer.ReplaceError.InvalidSectionType(string.Empty));
    }

    [Fact]
    public void fails_when_referencing_a_section_with_array()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    {
                        "Postgres", new List<string>
                        {
                            "Con1", "Con2"
                        }
                    }
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result =
            replacer.Replace(new ConfigReference("ModuleA:Db:$.ConnectionString", "{ConnectionStrings:Postgres}"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReferenceReplacer.ReplaceError.InvalidSectionType(string.Empty));
    }

    [Fact]
    public void fails_when_referencing_itself()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    { "$.Postgres", "{ConnectionStrings:$.Postgres}" }
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result =
            replacer.Replace(new ConfigReference("ConnectionStrings:$.Postgres", "{ConnectionStrings:$.Postgres}"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReferenceReplacer.ReplaceError.SelfReferenceDetected(string.Empty));
    }

    [Fact]
    public void fails_when_does_not_contains_a_reference()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    { "Postgres", "SomeCS" }
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result = replacer.Replace(new ConfigReference("ConnectionStrings:$.Postgres", "SomeConnectionString"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReferenceReplacer.ReplaceError.ReferenceMissing(string.Empty));
    }

    [Fact]
    public void fails_when_references_are_nested()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    { "Postgres", "CS1" },
                    { "Mssql", "CS2" },
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result = replacer.Replace(new ConfigReference("$.Postgres", "{Prefix{ConnectionStrings:Postgres}Suffix}"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReferenceReplacer.ReplaceError.SectionNotFound(string.Empty));
    }

    [Fact]
    public void replaces_with_reference_value_single_reference()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    { "Postgres", "Cs1" }
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result = replacer.Replace(new ConfigReference("$.Postgres", "{ConnectionStrings:Postgres}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Cs1");
    }

    [Fact]
    public void replaces_with_reference_value_two_references()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    { "Postgres", "Cs1" },
                    { "Mssql", "Cs2" },
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result =
            replacer.Replace(new ConfigReference("$.Postgres",
                "{ConnectionStrings:Postgres}--{ConnectionStrings:Mssql}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Cs1--Cs2");
    }

    [Fact]
    public void replaces_with_reference_value_array_index()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "ConnectionStrings", new Dictionary<string, object>
                {
                    {
                        "Postgres", new List<string>
                        {
                            "Cs1", "Cs2"
                        }
                    },
                }
            }
        });
        var root = AConfigurationRoot(config);
        var replacer = new ReferenceReplacer(root);

        var result = replacer.Replace(new ConfigReference("$.Postgres",
            "{ConnectionStrings:Postgres:0}--{ConnectionStrings:Postgres:1}"));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Cs1--Cs2");
    }
}