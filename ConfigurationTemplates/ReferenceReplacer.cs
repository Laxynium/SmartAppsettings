using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Configuration;

namespace ConfigurationTemplates;

internal partial class ReferenceReplacer
{
    private readonly IConfiguration _configuration;

    public ReferenceReplacer(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Result<string, ReplaceError> Replace(ConfigReference configReference)
    {
        var regex = ReferenceRegex();
        var matches = regex.Matches(configReference.Value);
        if (matches.Count == 0)
        {
            return ReplaceError.ReferenceMissing($"Template '{configReference.Path}' does not container reference");
        }

        foreach (Match match in matches)
        {
            var sectionPath = match.Groups[1].Value;

            if (sectionPath == configReference.Path)
                return ReplaceError.SelfReferenceDetected($"Template '{configReference.Path}' contains reference to itself");

            var section = _configuration.GetSection(sectionPath);

            if (section.Value is not null)
            {
                continue;
            }

            if (section.GetChildren().Any())
            {
                return ReplaceError.InvalidSectionType(
                    $"Template '{configReference.Path} has reference to not terminal configuration");
            }

            return ReplaceError.SectionNotFound($"Template '{configReference.Path}' has reference to not existing section");
        }

        var result = regex.Replace(configReference.Value, match =>
        {
            var sectionPath = match.Groups[1].Value;
            var section = _configuration.GetSection(sectionPath);
            if (section.Value is null)
            {
                throw new InvalidOperationException("Should have been already handled above");
            }

            return section.Value!;
        });
        return result;
    }

    [GeneratedRegex(@"\{([^\}]+)\}")]
    private static partial Regex ReferenceRegex();

    public class ReplaceError : ValueObject
    {
        private string Code { get; }
        public string Reason { get; }

        private ReplaceError(string code, string reason)
        {
            Code = code;
            Reason = reason;
        }

        public static ReplaceError SectionNotFound(string reason) => new(nameof(SectionNotFound), reason);
        public static ReplaceError InvalidSectionType(string reason) => new(nameof(InvalidSectionType), reason);
        public static ReplaceError SelfReferenceDetected(string reason) => new(nameof(SelfReferenceDetected), reason);
        public static ReplaceError ReferenceMissing(string reason) => new(nameof(ReferenceMissing), reason);


        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Code;
        }
    }
}