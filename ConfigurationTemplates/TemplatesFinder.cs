using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates;

internal static class TemplatesFinder
{
    internal static ReadOnlyCollection<IConfigurationSection> FindTemplates(
        IConfigurationRoot configuration,
        Func<string, bool> matchKey)
    {
        var flatten = TreeWalker.Flatten(configuration);
        var templates = flatten
            .Where(x => matchKey(x.Key))
            .ToList().AsReadOnly();
        return templates;
    }
}