using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates;

internal static partial class ConfigurationReferencesReplacer
{
    private const string TemplateMarkerPrefix = "$.";

    public static Dictionary<string, string> Process(ConfigurationManager configuration)
    {
        var templates = ReferencesFinder.FindTemplates(configuration, x => x.StartsWith(TemplateMarkerPrefix));

        var result = CreateInMemoryCollection(configuration, templates);

        return result;
    }

    private static Dictionary<string, string> CreateInMemoryCollection(IConfiguration configuration,
        ReadOnlyCollection<IConfigurationSection> configReferences)
    {
        var result = new Dictionary<string, string>();
        var referenceReplacer = new ReferenceReplacer(configuration);
        foreach (var template in configReferences)
        {
            if (template.Value is null)
            {
                throw new InvalidOperationException($"Section '{template.Path}' cannot be a reference, since it's not a terminal value");
            }

            var key = BuildKey(template);
            var value = BuildValue(template, referenceReplacer);
            result.Add(key, value);
        }

        return result;
    }

    private static string BuildValue(IConfigurationSection template, ReferenceReplacer referenceReplacer)
    {
        var configReference = ConfigReference.From(template);
        var valueR = referenceReplacer.Replace(configReference);

        if (valueR.IsFailure)
        {
            throw new InvalidOperationException(valueR.Error.Reason);
        }

        var value = valueR.Value;
        return value;
    }

    private static string BuildKey(IConfigurationSection template)
    {
        return template.Path.Replace(template.Key, template.Key[TemplateMarkerPrefix.Length..]);
    }
    
    [GeneratedRegex("\\{([^\\}]+)\\}")]
    private static partial Regex TemplateRegex();
}