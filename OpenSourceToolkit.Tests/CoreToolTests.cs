using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.Security;
using OpenSourceToolkit.TextData;
using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.Tests
{
    [TestClass]
    public class CoreToolTests
    {
        [TestMethod]
        public void Uuid_GeneratePlain_ReturnsValidGuid()
        {
            var uuid = UuidGenerator.GenerateFormatted(GuidFormat.Plain);
            Assert.IsTrue(Guid.TryParse(uuid, out _));
            Assert.IsTrue(uuid.Contains("-"));
        }

        [TestMethod]
        public void Hash_Md5_ReturnsCorrectHash()
        {
            // MD5("hello") = 5d41402abc4b2a76b9719d911017c592
            var result = HashGenerator.ComputeMd5("hello");
            Assert.AreEqual("5d41402abc4b2a76b9719d911017c592", result);
        }

        [TestMethod]
        public void Base64_EncodeDecode_RoundTrip()
        {
            string original = "OpenSourceToolkit";
            string encoded = Base64Converter.Encode(original);
            string decoded = Base64Converter.Decode(encoded);
            Assert.AreEqual(original, decoded);
        }

        [TestMethod]
        public void Timestamp_Conversion_IsAccurate()
        {
            // 2023-01-01 00:00:00 UTC
            var date = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long expectedTs = 1672531200;

            var ts = TimestampConverter.ToUnixTimeSeconds(date);
            Assert.AreEqual(expectedTs, ts);

            var back = TimestampConverter.FromUnixTimeSeconds(ts);
            Assert.AreEqual(date, back);
        }

        [TestMethod]
        public void Uuid_GenerateRegistry_ReturnsUppercaseWithBraces()
        {
            var uuid = UuidGenerator.GenerateFormatted(GuidFormat.Registry);

            Assert.IsTrue(uuid.StartsWith("{"));
            Assert.IsTrue(uuid.EndsWith("}"));
            Assert.AreEqual(uuid.ToUpperInvariant(), uuid);
        }

        [TestMethod]
        public void Uuid_GenerateBatch_ProducesUniqueGuids()
        {
            var list = UuidGenerator.GenerateBatch(GuidFormat.Plain, 10);

            Assert.AreEqual(10, list.Count);
            Assert.AreEqual(10, new HashSet<string>(list).Count);
        }

        [TestMethod]
        public void Uuid_GenerateShort_Returns22Chars()
        {
            var uuid = UuidGenerator.GenerateFormatted(GuidFormat.Short);
            Assert.AreEqual(22, uuid.Length);
        }

        [TestMethod]
        public void Hash_Sha256_KnownVector()
        {
            var result = HashGenerator.ComputeSha256("hello");
            Assert.AreEqual("2cf24dba5fb0a30e26e83b2ac5b9e29e1b161e5c1fa7425e73043362938b9824", result);
        }

        [TestMethod]
        public void Timestamp_Milliseconds_RoundTrip_WithLocalKind()
        {
            var local = new DateTime(2023, 1, 1, 12, 34, 56, 789, DateTimeKind.Local);
            var ms = TimestampConverter.ToUnixTimeMilliseconds(local);
            var back = TimestampConverter.FromUnixTimeMilliseconds(ms);

            Assert.AreEqual(local.ToUniversalTime(), back);
        }
    }
}
