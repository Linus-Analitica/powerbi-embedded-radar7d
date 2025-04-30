#nullable disable
namespace TemplateAngularCoreSAML.API.Models.Settings
{
    public class JwtSettings
    {
        public const string SectionName = "JwtSettings";
        public string Secrets { get; init; } = null!;
        public short ExpireMinutes { get; init; }
        public string Issuer { get; init; } = null!;
        public string Audience { get; init; }
    }
}
