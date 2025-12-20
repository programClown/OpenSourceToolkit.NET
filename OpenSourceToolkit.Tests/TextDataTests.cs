using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.TextData;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class TextDataTests
    {
        [TestMethod]
        public void LoremIpsum_GenerateWords_ReturnsCorrectCount()
        {
            var generator = new LoremIpsumGenerator();
            var words = generator.GenerateWords(5);
            Assert.IsNotNull(words);
            // Basic check: spaces should be count-1
            Assert.AreEqual(4, words.Count(c => c == ' '));
        }

        [TestMethod]
        public void MockData_GenerateUsers_ReturnsUsers()
        {
            var users = MockDataGenerator.GenerateUsers(10);
            Assert.IsNotNull(users);
            Assert.AreEqual(10, users.Count());

            var first = users.First();
            // Dynamic check
            Assert.IsNotNull(first.Email);
            Assert.IsNotNull(first.FirstName);
        }

        [TestMethod]
        public void QrCode_GeneratePng_ReturnsBytes()
        {
            var bytes = QrCodeGenerator.GeneratePng("test");
            Assert.IsNotNull(bytes);
            Assert.IsTrue(bytes.Length > 0);
        }

        [TestMethod]
        public void PrivacyPolicy_Generate_ContainsCompanyName()
        {
            var options = new PrivacyPolicyOptions
            {
                CompanyName = "Test Corp",
                WebsiteUrl = "http://test.com"
            };
            var policy = PrivacyPolicyGenerator.Generate(options, "2023-01-01");
            Assert.IsTrue(policy.Contains("Test Corp"));
            Assert.IsTrue(policy.Contains("http://test.com"));
        }

        [TestMethod]
        public void MockData_GenerateUsAddresses_ReturnsAddresses()
        {
            var addresses = MockDataGenerator.GenerateUsAddresses(5);
            Assert.IsNotNull(addresses);
            Assert.AreEqual(5, addresses.Count());

            var first = addresses.First();
            Assert.IsNotNull(first.Street);
            Assert.IsNotNull(first.City);
        }

        [TestMethod]
        public void QrCode_GenerateSvg_ReturnsSvgMarkup()
        {
            var svg = QrCodeGenerator.GenerateSvg("test");
            Assert.IsNotNull(svg);
            Assert.IsTrue(svg.StartsWith("<svg", StringComparison.OrdinalIgnoreCase));
        }

        // DiffChecker
        [TestMethod]
        public void DiffChecker_Compare_IdentifiesDifferences()
        {
            // Requires DiffPlex. Assuming it is available since the class exists.
            // If fails at runtime due to missing DLL, we need to add package.
            var checker = new DiffChecker();
            var model = checker.Compare("hello world", "hello there");

            Assert.AreEqual(1, model.OldText.Lines.Count);
            Assert.AreEqual(1, model.NewText.Lines.Count);

            // DiffPlex details might vary, but we expect *some* change detection
            // Usually it marks lines as Modified or Inserted/Deleted
            Assert.IsTrue(model.OldText.Lines[0].SubPieces.Count > 0 || model.OldText.Lines[0].Type != DiffPlex.DiffBuilder.Model.ChangeType.Unchanged);
        }

        // RegexTester
        [TestMethod]
        public void Regex_Test_Works()
        {
            Assert.IsTrue(RegexTester.Test(@"^\d+$", "12345"));
            Assert.IsFalse(RegexTester.Test(@"^\d+$", "12a45"));
        }

        [TestMethod]
        public void Regex_Matches_ReturnsCollection()
        {
            var matches = RegexTester.Matches(@"\d+", "a123b456");
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("123", matches[0].Value);
            Assert.AreEqual("456", matches[1].Value);
        }

        // VCardGenerator
        [TestMethod]
        public void VCardGenerator_Generate_FormatsCorrectly()
        {
            string vcard = VCardGenerator.Generate("John", "Doe", "john@example.com", "555-1234", "Acme Inc");

            Assert.IsTrue(vcard.Contains("BEGIN:VCARD"));
            Assert.IsTrue(vcard.Contains("FN:John Doe"));
            Assert.IsTrue(vcard.Contains("EMAIL:john@example.com"));
            Assert.IsTrue(vcard.Contains("ORG:Acme Inc"));
            Assert.IsTrue(vcard.Contains("END:VCARD"));
        }

        // MarkdownLinter Tests

        [TestMethod]
        public void MarkdownLinter_Lint_EmptyInput_ReturnsEmpty()
        {
            var violations = MarkdownLinter.Lint("");
            Assert.AreEqual(0, violations.Count);

            violations = MarkdownLinter.Lint(null);
            Assert.AreEqual(0, violations.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD004_DetectsAsteriskList()
        {
            var md = "* Item 1\n* Item 2\n";
            var violations = MarkdownLinter.Lint(md);
            var md004 = violations.Where(v => v.RuleId == "MD004").ToList();
            Assert.AreEqual(2, md004.Count);
            Assert.IsTrue(md004[0].Description.Contains("asterisk"));
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD004_DetectsPlusList()
        {
            var md = "+ Item 1\n+ Item 2\n";
            var violations = MarkdownLinter.Lint(md);
            var md004 = violations.Where(v => v.RuleId == "MD004").ToList();
            Assert.AreEqual(2, md004.Count);
            Assert.IsTrue(md004[0].Description.Contains("plus"));
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD004_AllowsDash()
        {
            var md = "- Item 1\n- Item 2\n";
            var violations = MarkdownLinter.Lint(md);
            var md004 = violations.Where(v => v.RuleId == "MD004").ToList();
            Assert.AreEqual(0, md004.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD009_DetectsTrailingSpaces()
        {
            var md = "Line with trailing spaces   \n";
            var violations = MarkdownLinter.Lint(md);
            var md009 = violations.Where(v => v.RuleId == "MD009").ToList();
            Assert.AreEqual(1, md009.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD010_DetectsHardTabs()
        {
            var md = "Line with\ttab\n";
            var violations = MarkdownLinter.Lint(md);
            var md010 = violations.Where(v => v.RuleId == "MD010").ToList();
            Assert.AreEqual(1, md010.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD012_DetectsMultipleBlanks()
        {
            var md = "Line 1\n\n\nLine 2\n";
            var violations = MarkdownLinter.Lint(md);
            var md012 = violations.Where(v => v.RuleId == "MD012").ToList();
            Assert.AreEqual(1, md012.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD022_DetectsMissingBlankAroundHeading()
        {
            var md = "Some text\n## Heading\nMore text\n";
            var violations = MarkdownLinter.Lint(md);
            var md022 = violations.Where(v => v.RuleId == "MD022").ToList();
            Assert.AreEqual(2, md022.Count); // Above and Below
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD022_NoViolationWithBlanks()
        {
            var md = "Some text\n\n## Heading\n\nMore text\n";
            var violations = MarkdownLinter.Lint(md);
            var md022 = violations.Where(v => v.RuleId == "MD022").ToList();
            Assert.AreEqual(0, md022.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD023_DetectsIndentedHeading()
        {
            var md = "  ## Indented Heading\n";
            var violations = MarkdownLinter.Lint(md);
            var md023 = violations.Where(v => v.RuleId == "MD023").ToList();
            Assert.AreEqual(1, md023.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD027_DetectsMultipleSpacesAfterBlockquote()
        {
            var md = ">  Multiple spaces\n";
            var violations = MarkdownLinter.Lint(md);
            var md027 = violations.Where(v => v.RuleId == "MD027").ToList();
            Assert.AreEqual(1, md027.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD030_DetectsMultipleSpacesAfterListMarker()
        {
            var md = "-  Item with extra space\n";
            var violations = MarkdownLinter.Lint(md);
            var md030 = violations.Where(v => v.RuleId == "MD030").ToList();
            Assert.AreEqual(1, md030.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD031_DetectsMissingBlankAroundCodeFence()
        {
            var md = "Some text\n```\ncode\n```\nMore text\n";
            var violations = MarkdownLinter.Lint(md);
            var md031 = violations.Where(v => v.RuleId == "MD031").ToList();
            Assert.AreEqual(2, md031.Count); // Above and Below
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD032_DetectsMissingBlankAroundList()
        {
            var md = "Some text\n- Item 1\n- Item 2\nMore text\n";
            var violations = MarkdownLinter.Lint(md);
            var md032 = violations.Where(v => v.RuleId == "MD032").ToList();
            Assert.IsTrue(md032.Count >= 1);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD037_DetectsSpacesInEmphasis()
        {
            // Single-word content with spaces (actual violation)
            var md = "This is * bold * text\n";
            var violations = MarkdownLinter.Lint(md);
            var md037 = violations.Where(v => v.RuleId == "MD037").ToList();
            Assert.AreEqual(1, md037.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD037_NoFalsePositiveCommaSeparatedBold()
        {
            // Comma-separated bold spans (common in docs) should not trigger MD037
            var md = "DaisyLoading provides **27 different animation styles**, **5 size options**, and **9 color variants**.\n";
            var violations = MarkdownLinter.Lint(md);
            var md037 = violations.Where(v => v.RuleId == "MD037").ToList();
            Assert.AreEqual(0, md037.Count, "Comma-separated bold spans should not trigger MD037");
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD038_DetectsSpacesInCodeSpan()
        {
            // Single-word content with both leading and trailing spaces
            var md = "This is ` code ` in text\n";
            var violations = MarkdownLinter.Lint(md);
            var md038 = violations.Where(v => v.RuleId == "MD038").ToList();
            Assert.AreEqual(1, md038.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD038_DetectsLeadingSpaceOnly()
        {
            var md = "This is ` leading`\n";
            var violations = MarkdownLinter.Lint(md);
            var md038 = violations.Where(v => v.RuleId == "MD038").ToList();
            Assert.AreEqual(1, md038.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD038_DetectsTrailingSpaceOnly()
        {
            var md = "This is `trailing `\n";
            var violations = MarkdownLinter.Lint(md);
            var md038 = violations.Where(v => v.RuleId == "MD038").ToList();
            Assert.AreEqual(1, md038.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD038_NoFalsePositiveInTable()
        {
            // This is the key test - table cells with code spans should NOT trigger MD038
            var md = "| Property | Type | Default | Description |\n" +
                     "|----------|------|---------|-------------|\n" +
                     "| `AccessibleText` | `string` | `\"Loading\"` | The text announced by screen readers. |\n";
            var violations = MarkdownLinter.Lint(md);
            var md038 = violations.Where(v => v.RuleId == "MD038").ToList();
            Assert.AreEqual(0, md038.Count, "Table with code spans should not trigger MD038");
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD038_NoFalsePositiveMultipleCodeSpans()
        {
            // Multiple code spans on same line should not be matched across spans
            var md = "Use `foo` and `bar` together.\n";
            var violations = MarkdownLinter.Lint(md);
            var md038 = violations.Where(v => v.RuleId == "MD038").ToList();
            Assert.AreEqual(0, md038.Count, "Multiple valid code spans should not trigger MD038");
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD038_NoFalsePositiveCommaSeparatedCodeSpans()
        {
            // Comma-separated code spans (common in docs) should not trigger MD038
            var md = "| Counter position: `Top`, `Bottom`, `Start`, `End` |\n";
            var violations = MarkdownLinter.Lint(md);
            var md038 = violations.Where(v => v.RuleId == "MD038").ToList();
            Assert.AreEqual(0, md038.Count, "Comma-separated code spans should not trigger MD038");
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_FixesMD038()
        {
            // Use single-word code content since multi-word is excluded to prevent false positives
            var md = "This is ` code ` and ` text `\n";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsTrue(fixed_md.Contains("`code`"), "Leading/trailing spaces should be removed");
            Assert.IsTrue(fixed_md.Contains("`text`"), "Leading/trailing spaces should be removed");
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_PreservesTableCodeSpans()
        {
            // Ensure auto-fix does not corrupt table code spans
            var md = "| `Property` | `Type` |\n|------------|--------|\n";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsTrue(fixed_md.Contains("`Property`"), "Table code span should be preserved");
            Assert.IsTrue(fixed_md.Contains("`Type`"), "Table code span should be preserved");
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD046_DetectsMultipleFencesOnSameLine()
        {
            var md = "```text```xml\n";
            var violations = MarkdownLinter.Lint(md);
            var md046 = violations.Where(v => v.RuleId == "MD046").ToList();
            Assert.AreEqual(1, md046.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_MD047_DetectsMissingFinalNewline()
        {
            var md = "Some text";
            var violations = MarkdownLinter.Lint(md);
            var md047 = violations.Where(v => v.RuleId == "MD047").ToList();
            Assert.AreEqual(1, md047.Count);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_SkipsCodeBlockContent()
        {
            var md = "```\n* asterisk list inside code\n```\n";
            var violations = MarkdownLinter.Lint(md);
            var md004 = violations.Where(v => v.RuleId == "MD004").ToList();
            Assert.AreEqual(0, md004.Count);
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_FixesMD004()
        {
            var md = "* Item 1\n* Item 2\n";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsTrue(fixed_md.Contains("- Item 1"));
            Assert.IsTrue(fixed_md.Contains("- Item 2"));
            Assert.IsFalse(fixed_md.Contains("* Item"));
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_FixesMD009()
        {
            var md = "Line with trailing spaces   \n";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsFalse(fixed_md.Contains("   \n"));
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_FixesMD010()
        {
            var md = "Line with\ttab\n";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsFalse(fixed_md.Contains("\t"));
            Assert.IsTrue(fixed_md.Contains("    ")); // 4 spaces
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_FixesMD022()
        {
            var md = "Some text\n## Heading\nMore text\n";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsTrue(fixed_md.Contains("Some text\n\n## Heading\n\nMore text"));
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_FixesMD023()
        {
            var md = "  ## Indented Heading\n";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsTrue(fixed_md.StartsWith("## Indented Heading"));
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_FixesMD047()
        {
            var md = "Some text";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsTrue(fixed_md.EndsWith("\n"));
        }

        [TestMethod]
        public void MarkdownLinter_AutoFix_PreservesCodeBlocks()
        {
            var md = "```\n* asterisk in code\n```\n";
            var fixed_md = MarkdownLinter.AutoFix(md);
            Assert.IsTrue(fixed_md.Contains("* asterisk in code"));
        }

        [TestMethod]
        public void MarkdownLinter_AutoFixBatch_FixesMultipleIssues()
        {
            var md = "* Item   \n## Heading\nMore text";
            var fixed_md = MarkdownLinter.AutoFixBatch(md);
            Assert.IsTrue(fixed_md.Contains("- Item"));
            Assert.IsFalse(fixed_md.Contains("   \n"));
            Assert.IsTrue(fixed_md.EndsWith("\n"));
        }

        [TestMethod]
        public void MarkdownLinter_FixSingleViolation_FixesMD004()
        {
            var md = "* Item 1\n";
            var violation = new MarkdownLintViolation
            {
                RuleId = "MD004",
                LineNumber = 1,
                CanAutoFix = true
            };
            var fixed_md = MarkdownLinter.FixSingleViolation(md, violation);
            Assert.IsTrue(fixed_md.Contains("- Item 1"));
        }

        [TestMethod]
        public void MarkdownLinter_FixSingleViolation_NullInput_ReturnsOriginal()
        {
            var result = MarkdownLinter.FixSingleViolation(null, null);
            Assert.IsNull(result);

            result = MarkdownLinter.FixSingleViolation("test", null);
            Assert.AreEqual("test", result);
        }

        [TestMethod]
        public void MarkdownLinter_Lint_ComplexDocument_FindsAllIssues()
        {
            var md = "Some text\n## Heading\n* Item 1\n* Item 2\nMore text   \n```\ncode\n```\nFinal line";

            var violations = MarkdownLinter.Lint(md);
            Assert.IsTrue(violations.Count > 0);
            Assert.IsTrue(violations.Any(v => v.RuleId == "MD022")); // Heading blanks
            Assert.IsTrue(violations.Any(v => v.RuleId == "MD004")); // Asterisk list
            Assert.IsTrue(violations.Any(v => v.RuleId == "MD009")); // Trailing spaces
            Assert.IsTrue(violations.Any(v => v.RuleId == "MD047")); // Final newline
        }
    }
}
