using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates;

public record ConfigReference(string Path, string Value)
{
    public static ConfigReference From(IConfigurationSection configurationSection)
    {
        if (configurationSection.Value is null)
        {
            throw new ArgumentException("Config reference cannot be created from section without value",
                nameof(configurationSection));
        }

        return new ConfigReference(configurationSection.Path, configurationSection.Value);
    }
}