namespace OpenSourceToolkit.TextData
{
    public enum PrivacyPolicyFormat
    {
        PlainText,
        Markdown
    }

    public class PrivacyPolicyOptions
    {
        public string CompanyName { get; set; } = "My Company";
        public string WebsiteUrl { get; set; } = "https://example.com";
        public string ContactEmail { get; set; } = "privacy@example.com";
        public bool CollectsCookies { get; set; } = true;
        public bool CollectsAnalytics { get; set; } = true;
        public bool CollectsPersonalData { get; set; } = true;
        public bool HasThirdPartyServices { get; set; }
        public bool HasUserAccounts { get; set; }
        public bool IncludeGdprSection { get; set; } = true;
        public bool IncludeCcpaSection { get; set; }
        public PrivacyPolicyFormat OutputFormat { get; set; } = PrivacyPolicyFormat.PlainText;
    }
}
