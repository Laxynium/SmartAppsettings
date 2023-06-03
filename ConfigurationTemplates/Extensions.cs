using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates;

public static partial class Extensions
{
    private const string TemplateMarkerPrefix = "$.";

    public static void AddTemplatesSources(this ConfigurationManager configuration)
    {
        var templates = TemplatesFinder.FindTemplates(configuration, x => x.StartsWith(TemplateMarkerPrefix));

        var result = CreateInMemoryCollection(configuration, templates);

        configuration.AddInMemoryCollection(result!);
    }

    private static Dictionary<string, string> CreateInMemoryCollection(IConfiguration configuration,
        ReadOnlyCollection<IConfigurationSection> templates)
    {
        var result = new Dictionary<string, string>();
        foreach (var template in templates)
        {
            var key = BuildKey(template);
            var value = BuildValue(configuration, template);
            result.Add(key, value);
        }

        return result;
    }

    private static string BuildKey(IConfigurationSection template)
    {
        return template.Path.Replace(template.Key, template.Key[TemplateMarkerPrefix.Length..]);
    }

    private static string BuildValue(IConfiguration configuration, IConfigurationSection template)
    {
        var templateValue = template.Value ?? throw new InvalidCastException("This exception should never be thrown.");
        var result = TemplateRegex().Replace(templateValue, match =>
        {
            var sectionPath = match.Groups[1].Value;
            var section = configuration.GetSection(sectionPath);
            var value = section.Value;
            if (value is null)
            {
                if (section.GetChildren().Any())
                {
                    throw new InvalidOperationException(
                        $"Configuration reference '{sectionPath}' in template '{template.Path}' is pointing to a primitive type");
                }

                throw new InvalidOperationException(
                    $"Found a template '{template.Path}' which is referencing not existing configuration section '{sectionPath}'");
            }

            return value;
        });
        return result;
    }

    [GeneratedRegex("\\{([^\\}]+)\\}")]
    private static partial Regex TemplateRegex();
}