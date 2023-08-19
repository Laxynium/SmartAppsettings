using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;

namespace SmartAppsettings;

internal static class ReferencesFinder
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

    private static class TreeWalker
    {
        public static IReadOnlyList<IConfigurationSection> Flatten(IConfigurationRoot root)
        {
            var result = new List<IConfigurationSection>();
            var queue = new Queue<IConfigurationSection>(root.GetChildren());
            while (queue.TryDequeue(out var config))
            {
                result.Add(config);
                foreach (var child in config.GetChildren())
                {
                    queue.Enqueue(child);
                }
            }

            return result;
        }
    }
}