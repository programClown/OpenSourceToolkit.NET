using System;
using System.Text;

namespace OpenSourceToolkit.TextData
{
    public static class PrivacyPolicyGenerator
    {
        public static string Generate(PrivacyPolicyOptions options, string lastUpdated = null)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            bool md = options.OutputFormat == PrivacyPolicyFormat.Markdown;
            var dateString = lastUpdated ?? DateTime.Now.ToShortDateString();
            int sectionNumber = 0;
            var sb = new StringBuilder();

            sb.AppendLine(md ? $"# Privacy Policy for {options.CompanyName}" : $"Privacy Policy for {options.CompanyName}");
            sb.AppendLine(md ? $"*Last updated: {dateString}*" : $"Last updated: {dateString}");
            sb.AppendLine();

            AppendIntroduction(sb, ref sectionNumber, options, md);
            AppendDataCollection(sb, ref sectionNumber, options, md);
            AppendDataUsage(sb, ref sectionNumber, md);

            if (options.CollectsCookies)
                AppendCookiesSection(sb, ref sectionNumber, md);

            if (options.CollectsAnalytics)
                AppendAnalyticsSection(sb, ref sectionNumber, md);

            if (options.HasThirdPartyServices)
                AppendThirdPartySection(sb, ref sectionNumber, md);

            if (options.HasUserAccounts)
                AppendUserAccountsSection(sb, ref sectionNumber, md);

            AppendDataRetention(sb, ref sectionNumber, md);
            AppendDataSecurity(sb, ref sectionNumber, md);

            if (options.IncludeGdprSection)
                AppendGdprSection(sb, ref sectionNumber, md);

            if (options.IncludeCcpaSection)
                AppendCcpaSection(sb, ref sectionNumber, md);

            AppendChangesToPolicy(sb, ref sectionNumber, md);
            AppendContact(sb, ref sectionNumber, options, md);

            return sb.ToString();
        }

        private static string Heading(int sectionNumber, string title, bool md)
        {
            return md ? $"## {sectionNumber}. {title}" : $"{sectionNumber}. {title}";
        }

        private static string Bullet(string text, bool md)
        {
            return md ? $"- {text}" : $"• {text}";
        }

        private static void AppendIntroduction(StringBuilder sb, ref int sectionNumber, PrivacyPolicyOptions options, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Introduction", md));
            sb.AppendLine();
            sb.AppendLine($"Welcome to {options.CompanyName}. We respect your privacy and are committed to protecting your personal data. This privacy policy explains how we collect, use, and safeguard your information when you visit {options.WebsiteUrl}.");
            sb.AppendLine();
        }

        private static void AppendDataCollection(StringBuilder sb, ref int sectionNumber, PrivacyPolicyOptions options, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Information We Collect", md));
            sb.AppendLine();
            if (options.CollectsPersonalData)
            {
                sb.AppendLine("We may collect the following types of personal information:");
                sb.AppendLine();
                sb.AppendLine(Bullet("Contact information (name, email address, phone number)", md));
                sb.AppendLine(Bullet("Account credentials (username, password)", md));
                sb.AppendLine(Bullet("Demographic information (age, location, preferences)", md));
            }
            sb.AppendLine(Bullet("Technical data (IP address, browser type, device information)", md));
            sb.AppendLine(Bullet("Usage data (pages visited, time spent on site, navigation paths)", md));
            sb.AppendLine();
        }

        private static void AppendDataUsage(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "How We Use Your Information", md));
            sb.AppendLine();
            sb.AppendLine("We use collected information for the following purposes:");
            sb.AppendLine();
            sb.AppendLine(Bullet("To provide and maintain our services", md));
            sb.AppendLine(Bullet("To improve and personalize user experience", md));
            sb.AppendLine(Bullet("To communicate with you about updates and offers", md));
            sb.AppendLine(Bullet("To comply with legal obligations", md));
            sb.AppendLine(Bullet("To detect and prevent fraud or abuse", md));
            sb.AppendLine();
        }

        private static void AppendCookiesSection(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Cookies and Tracking Technologies", md));
            sb.AppendLine();
            sb.AppendLine("We use cookies and similar tracking technologies to enhance your experience:");
            sb.AppendLine();
            sb.AppendLine(Bullet("**Essential cookies**: Required for basic site functionality", md));
            sb.AppendLine(Bullet("**Performance cookies**: Help us understand how visitors use our site", md));
            sb.AppendLine(Bullet("**Functional cookies**: Remember your preferences and settings", md));
            sb.AppendLine(Bullet("**Marketing cookies**: Used to deliver relevant advertisements", md));
            sb.AppendLine();
            sb.AppendLine("You can control cookie preferences through your browser settings. Note that disabling certain cookies may affect site functionality.");
            sb.AppendLine();
        }

        private static void AppendAnalyticsSection(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Analytics", md));
            sb.AppendLine();
            sb.AppendLine("We use analytics services to understand how visitors interact with our website. These services may collect:");
            sb.AppendLine();
            sb.AppendLine(Bullet("Pages visited and time spent on each page", md));
            sb.AppendLine(Bullet("Referring websites and search terms", md));
            sb.AppendLine(Bullet("Geographic location (country/city level)", md));
            sb.AppendLine(Bullet("Device and browser information", md));
            sb.AppendLine();
            sb.AppendLine("This data helps us improve our services and user experience.");
            sb.AppendLine();
        }

        private static void AppendThirdPartySection(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Third-Party Services", md));
            sb.AppendLine();
            sb.AppendLine("We may use third-party services that collect, monitor, and analyze data. These services have their own privacy policies governing the use of your information. We encourage you to review their policies.");
            sb.AppendLine();
            sb.AppendLine("Third-party services may include:");
            sb.AppendLine();
            sb.AppendLine(Bullet("Payment processors", md));
            sb.AppendLine(Bullet("Analytics providers", md));
            sb.AppendLine(Bullet("Advertising networks", md));
            sb.AppendLine(Bullet("Social media platforms", md));
            sb.AppendLine();
        }

        private static void AppendUserAccountsSection(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "User Accounts", md));
            sb.AppendLine();
            sb.AppendLine("If you create an account with us, we store:");
            sb.AppendLine();
            sb.AppendLine(Bullet("Your registration information", md));
            sb.AppendLine(Bullet("Account preferences and settings", md));
            sb.AppendLine(Bullet("Activity history and usage patterns", md));
            sb.AppendLine();
            sb.AppendLine("You may update or delete your account information at any time through your account settings or by contacting us.");
            sb.AppendLine();
        }

        private static void AppendDataRetention(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Data Retention", md));
            sb.AppendLine();
            sb.AppendLine("We retain your personal information only for as long as necessary to fulfill the purposes outlined in this policy, unless a longer retention period is required by law. When data is no longer needed, we will securely delete or anonymize it.");
            sb.AppendLine();
        }

        private static void AppendDataSecurity(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Data Security", md));
            sb.AppendLine();
            sb.AppendLine("We implement appropriate technical and organizational security measures to protect your personal information against unauthorized access, alteration, disclosure, or destruction. However, no method of transmission over the Internet is 100% secure.");
            sb.AppendLine();
        }

        private static void AppendGdprSection(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Your Rights (GDPR)", md));
            sb.AppendLine();
            sb.AppendLine("If you are located in the European Economic Area, you have the following rights:");
            sb.AppendLine();
            sb.AppendLine(Bullet("**Right to access**: Request a copy of your personal data", md));
            sb.AppendLine(Bullet("**Right to rectification**: Request correction of inaccurate data", md));
            sb.AppendLine(Bullet("**Right to erasure**: Request deletion of your personal data", md));
            sb.AppendLine(Bullet("**Right to restriction**: Request limited processing of your data", md));
            sb.AppendLine(Bullet("**Right to portability**: Receive your data in a portable format", md));
            sb.AppendLine(Bullet("**Right to object**: Object to processing of your personal data", md));
            sb.AppendLine();
            sb.AppendLine("To exercise these rights, please contact us using the information below.");
            sb.AppendLine();
        }

        private static void AppendCcpaSection(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "California Privacy Rights (CCPA)", md));
            sb.AppendLine();
            sb.AppendLine("California residents have specific rights regarding their personal information:");
            sb.AppendLine();
            sb.AppendLine(Bullet("Right to know what personal information is collected", md));
            sb.AppendLine(Bullet("Right to know if personal information is sold or disclosed", md));
            sb.AppendLine(Bullet("Right to opt-out of the sale of personal information", md));
            sb.AppendLine(Bullet("Right to request deletion of personal information", md));
            sb.AppendLine(Bullet("Right to non-discrimination for exercising privacy rights", md));
            sb.AppendLine();
            sb.AppendLine("To exercise these rights, please contact us using the information below.");
            sb.AppendLine();
        }

        private static void AppendChangesToPolicy(StringBuilder sb, ref int sectionNumber, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Changes to This Policy", md));
            sb.AppendLine();
            sb.AppendLine("We may update this privacy policy from time to time. We will notify you of any changes by posting the new policy on this page and updating the \"Last updated\" date. We encourage you to review this policy periodically.");
            sb.AppendLine();
        }

        private static void AppendContact(StringBuilder sb, ref int sectionNumber, PrivacyPolicyOptions options, bool md)
        {
            sb.AppendLine(Heading(++sectionNumber, "Contact Us", md));
            sb.AppendLine();
            sb.AppendLine("If you have any questions about this Privacy Policy, please contact us:");
            sb.AppendLine();
            sb.AppendLine(Bullet($"Website: {options.WebsiteUrl}", md));
            if (!string.IsNullOrEmpty(options.ContactEmail))
                sb.AppendLine(Bullet($"Email: {options.ContactEmail}", md));
        }
    }
}
