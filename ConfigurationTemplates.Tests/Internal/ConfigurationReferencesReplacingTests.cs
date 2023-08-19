using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates.Tests.Internal;

public class ConfigurationReferencesReplacingTests : TestsBase
{
    [Fact]
    public void throws_exception_when_reference_is_invalid()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "Secrets", new Dictionary<string, object>
                {
                    {
                        "Postgres", new Dictionary<string, object>
                        {
                            { "Password", "SomePassword" }
                        }
                    }
                }
            },
            {
                "ModuleA", new Dictionary<string, object>
                {
                    {
                        "ConnectionStrings", new Dictionary<string, object>
                        {
                            { "$.Postgres", "Password={Secrets:Postgres};" }
                        }
                    }
                }
            }
        });
        var configurationManager = new ConfigurationManager();
        configurationManager.AddJsonStream(config);

        var action = () => ConfigurationReferencesReplacer.Process(configurationManager);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void throws_exception_when_reference_is_applied_on_non_terminal_value()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "Secrets", new Dictionary<string, object>
                {
                    {
                        "Postgres", new Dictionary<string, object>
                        {
                            { "Password", "SomePassword" }
                        }
                    }
                }
            },
            {
                "ModuleA", new Dictionary<string, object>
                {
                    {
                        "$.ConnectionStrings", new Dictionary<string, object>
                        {
                            { "$.Postgres", "Password={Secrets:Postgres};" }
                        }
                    }
                }
            }
        });
        var configurationManager = new ConfigurationManager();
        configurationManager.AddJsonStream(config);

        var action = () => ConfigurationReferencesReplacer.Process(configurationManager);

        action.Should().Throw<InvalidOperationException>().WithMessage("*$.ConnectionStrings*");
    }

    [Fact]
    public void returns_configuration_with_replaced_references()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "Secrets", new Dictionary<string, object>
                {
                    {
                        "Postgres", new Dictionary<string, object>
                        {
                            { "Password", "SomePassword" }
                        }
                    }
                }
            },
            {
                "ModuleA", new Dictionary<string, object>
                {
                    {
                        "ConnectionStrings", new Dictionary<string, object>
                        {
                            { "$.Postgres", "Password={Secrets:Postgres:Password};" }
                        }
                    }
                }
            }
        });
        var configurationManager = new ConfigurationManager();
        configurationManager.AddJsonStream(config);

        var result = ConfigurationReferencesReplacer.Process(configurationManager);

        result.Should().HaveCount(1);
        result.Should().Contain(x =>
            x.Key == "ModuleA:ConnectionStrings:Postgres" && x.Value == "Password=SomePassword;");
    }

    [Fact]
    public void returns_configuration_with_replaced_references_when_there_are_many_references()
    {
        var config = AConfig(new Dictionary<string, object>
        {
            {
                "Secrets", new Dictionary<string, object>
                {
                    {
                        "Postgres", new Dictionary<string, object>
                        {
                            { "Password", "SomePassword" }
                        }
                    }
                }
            },
            {
                "ModuleA", new Dictionary<string, object>
                {
                    {
                        "ConnectionStrings", new Dictionary<string, object>
                        {
                            { "$.Postgres", "Password={Secrets:Postgres:Password};" }
                        }
                    },
                    {
                        "ConnectionStrings2",
                        new Dictionary<string, object>
                        {
                            { "$.Postgres", "Password={Secrets:Postgres:Password};" }
                        }
                    }
                }
            }
        });
        var configurationManager = new ConfigurationManager();
        configurationManager.AddJsonStream(config);

        var result = ConfigurationReferencesReplacer.Process(configurationManager);

        result.Should().HaveCount(2);
        result.Should().Contain(x =>
            x.Key == "ModuleA:ConnectionStrings:Postgres" && x.Value == "Password=SomePassword;");
        result.Should().Contain(x =>
            x.Key == "ModuleA:ConnectionStrings2:Postgres" && x.Value == "Password=SomePassword;");
    }
}