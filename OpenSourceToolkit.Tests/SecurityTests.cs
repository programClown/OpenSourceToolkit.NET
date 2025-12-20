using System.Linq;
using System.Security.Claims;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Security;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class SecurityTests
    {
        [TestMethod]
        public void Hmac_Sha256_ReturnsCorrectLength()
        {
            var hmac = HmacGenerator.GenerateHmacSha256("message", "secret");
            Assert.IsNotNull(hmac);
            // SHA256 is 32 bytes, hex is 64 chars
            Assert.AreEqual(64, hmac.Length);
        }

        [TestMethod]
        public void Hash_Sha512_ReturnsCorrectLength()
        {
            var hash = HashGenerator.ComputeSha512("message");
            Assert.IsNotNull(hash);
            // SHA512 is 64 bytes, hex is 128 chars
            Assert.AreEqual(128, hash.Length);
        }

        [TestMethod]
        public void Jwt_GenerateAndValidate_Works()
        {
            string secret = "super_secret_key_1234567890123456"; // must be >= 128 bits usually
            string issuer = "me";
            string audience = "you";

            var token = JwtHelper.GenerateToken(secret, issuer, audience);
            Assert.IsNotNull(token);

            var principal = JwtHelper.ValidateToken(token, secret, issuer, audience);
            Assert.IsNotNull(principal);
            Assert.IsTrue(principal.Identity.IsAuthenticated);
        }

        [TestMethod]
        public void Jwt_DecodeToken_ExposesIssuerAndAudience()
        {
            string secret = "super_secret_key_1234567890123456";
            string issuer = "me";
            string audience = "you";

            var token = JwtHelper.GenerateToken(secret, issuer, audience);
            var decoded = JwtHelper.DecodeToken(token);

            Assert.IsNotNull(decoded);
            Assert.AreEqual(issuer, decoded.Issuer);
            Assert.AreEqual(audience, decoded.Audiences.Single());
        }

        // PasswordGenerator
        [TestMethod]
        public void PasswordGenerator_Generate_RespectsLengthAndRules()
        {
            // Use a longer length to ensure statistical probability of including all types is near 100%
            // The generator does not force inclusion, just expands the pool.
            string pwd = PasswordGenerator.Generate(100, true, true, true);
            Assert.AreEqual(100, pwd.Length);
            Assert.IsTrue(pwd.Any(char.IsUpper));
            Assert.IsTrue(pwd.Any(char.IsDigit));
            Assert.IsTrue(pwd.Any(c => !char.IsLetterOrDigit(c)));
        }

        [TestMethod]
        public void PasswordGenerator_Generate_RespectsOptions()
        {
            var options = new PasswordOptions
            {
                Length = 20,
                IncludeUppercase = true,
                IncludeLowercase = true,
                IncludeNumbers = true,
                IncludeSymbols = true,
                MinUppercase = 2,
                MinLowercase = 2,
                MinNumbers = 2,
                MinSymbols = 2,
                ExcludeSimilar = true
            };

            string pwd = PasswordGenerator.Generate(options);

            Assert.AreEqual(20, pwd.Length);
            Assert.IsTrue(pwd.Count(char.IsUpper) >= 2, "Should have min 2 uppercase");
            Assert.IsTrue(pwd.Count(char.IsLower) >= 2, "Should have min 2 lowercase");
            Assert.IsTrue(pwd.Count(char.IsDigit) >= 2, "Should have min 2 digits");
            Assert.IsTrue(pwd.Any(c => !char.IsLetterOrDigit(c)), "Should have symbols");

            // Check for similar characters (il1Lo0O)
            Assert.IsFalse(pwd.IndexOfAny("il1Lo0O".ToCharArray()) >= 0, "Should exclude similar chars");
        }

        [TestMethod]
        public void PasswordGenerator_GeneratePin_ReturnsOnlyDigits()
        {
            string pin = PasswordGenerator.GeneratePin(6);
            Assert.AreEqual(6, pin.Length);
            Assert.IsTrue(pin.All(char.IsDigit));
        }

        [TestMethod]
        public void PasswordGenerator_GeneratePassphrase_ReturnsWords()
        {
            string passphrase = PasswordGenerator.GeneratePassphrase(4, "-", true, false);
            string[] words = passphrase.Split('-');
            Assert.AreEqual(4, words.Length);
            Assert.IsTrue(char.IsUpper(words[0][0]), "First letter should be capitalized");
        }

        [TestMethod]
        public void PasswordStrengthAnalyzer_Analyze_ReturnsReasonableScore()
        {
            var weak = PasswordStrengthAnalyzer.Analyze("123");
            Assert.IsTrue(weak.Strength <= 1);
            Assert.AreEqual("Very Weak", weak.Label);

            var strong = PasswordStrengthAnalyzer.Analyze("Xy9#mK2$pL5@nR8&vQ3!zT7*wJ4%");
            Assert.IsTrue(strong.Entropy > 50);
        }
    }
}
