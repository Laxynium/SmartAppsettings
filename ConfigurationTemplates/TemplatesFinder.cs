using System.Collections.ObjectModel;
using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates;

internal static class TemplatesFinder
{
    internal static ReadOnlyCollection<IConfigurationSection> FindTemplates(
        IConfigurationRoot configuration,
        Func<string, bool> matchKey)
    {
        var flatten = TreeWalker.IterativeFlatten(configuration);
        var templates = flatten
            .Where(x => matchKey(x.Key))
            .ToList().AsReadOnly();
        return templates;
    }

    private static class TreeWalker
    {
        public static IReadOnlyList<IConfigurationSection> IterativeFlatten(IConfigurationRoot root)
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

        public static IReadOnlyList<IConfigurationSection> RecursiveFlatten(IConfigurationRoot root)
        {
            var result = new List<IConfigurationSection>();
            foreach (var c in root.GetChildren())
            {
                Walk(c, result);
            }

            return result;
        }

        private static void Walk(IConfigurationSection root, ICollection<IConfigurationSection> result)
        {
            result.Add(root);
            var children = root.GetChildren();
            foreach (var c in children)
            {
                Walk(c, result);
            }
        }
    }
}