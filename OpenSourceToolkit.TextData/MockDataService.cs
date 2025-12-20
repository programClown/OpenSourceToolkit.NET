using System;
using System.Collections.Generic;

namespace OpenSourceToolkit.TextData
{
    /// <summary>
    /// Describes a mock data type that can be generated.
    /// </summary>
    public class MockDataType
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string FlagKey { get; set; }
    }

    /// <summary>
    /// Reusable service for mock data generation, exposing available data types and a unified Generate method.
    /// </summary>
    public static class MockDataService
    {
        public static readonly MockDataType[] AvailableTypes = new[]
        {
            new MockDataType { Key = "Users", DisplayName = "Users", FlagKey = "User" },
            new MockDataType { Key = "Addresses_US", DisplayName = "Addresses (US)", FlagKey = "US" },
            new MockDataType { Key = "Addresses_DE", DisplayName = "Addresses (Germany)", FlagKey = "DE" },
            new MockDataType { Key = "Addresses_GB", DisplayName = "Addresses (UK)", FlagKey = "GB" },
            new MockDataType { Key = "Addresses_JP", DisplayName = "Addresses (Japan)", FlagKey = "JP" },
            new MockDataType { Key = "Companies", DisplayName = "Companies", FlagKey = null },
            new MockDataType { Key = "CreditCards", DisplayName = "Credit Cards", FlagKey = null },
            new MockDataType { Key = "Products", DisplayName = "Products", FlagKey = null },
            new MockDataType { Key = "Orders", DisplayName = "Orders", FlagKey = null },
            new MockDataType { Key = "Reviews", DisplayName = "Reviews", FlagKey = null },
            new MockDataType { Key = "BlogPosts", DisplayName = "Blog Posts", FlagKey = null },
            new MockDataType { Key = "Comments", DisplayName = "Comments", FlagKey = null },
            new MockDataType { Key = "UUIDs", DisplayName = "UUIDs", FlagKey = null },
            new MockDataType { Key = "Emails", DisplayName = "Emails", FlagKey = null },
            new MockDataType { Key = "URLs", DisplayName = "URLs & Domains", FlagKey = null },
            new MockDataType { Key = "IPs", DisplayName = "IP Addresses", FlagKey = null },
            new MockDataType { Key = "Dates", DisplayName = "Dates & Timestamps", FlagKey = null },
            new MockDataType { Key = "BankAccounts", DisplayName = "Bank Accounts", FlagKey = null },
            new MockDataType { Key = "Transactions", DisplayName = "Transactions", FlagKey = null }
        };

        /// <summary>
        /// Generates mock data for the specified type key.
        /// </summary>
        /// <param name="typeKey">The key identifying the data type (e.g. "Users", "Addresses_US").</param>
        /// <param name="count">Number of items to generate.</param>
        /// <returns>An enumerable of generated data objects.</returns>
        public static IEnumerable<dynamic> Generate(string typeKey, int count)
        {
            switch (typeKey)
            {
                case "Users":
                    return MockDataGenerator.GenerateUsers(count);
                case "Addresses_US":
                    return MockDataGenerator.GenerateUsAddresses(count);
                case "Addresses_DE":
                    return MockDataGenerator.GenerateDeAddresses(count);
                case "Addresses_GB":
                    return MockDataGenerator.GenerateGbAddresses(count);
                case "Addresses_JP":
                    return MockDataGenerator.GenerateJpAddresses(count);
                case "Companies":
                    return MockDataGenerator.GenerateCompanies(count);
                case "CreditCards":
                    return MockDataGenerator.GenerateCreditCards(count);
                case "Products":
                    return MockDataGenerator.GenerateProducts(count);
                case "Orders":
                    return MockDataGenerator.GenerateOrders(count);
                case "Reviews":
                    return MockDataGenerator.GenerateReviews(count);
                case "BlogPosts":
                    return MockDataGenerator.GenerateBlogPosts(count);
                case "Comments":
                    return MockDataGenerator.GenerateComments(count);
                case "UUIDs":
                    return MockDataGenerator.GenerateUuids(count);
                case "Emails":
                    return MockDataGenerator.GenerateEmails(count);
                case "URLs":
                    return MockDataGenerator.GenerateUrls(count);
                case "IPs":
                    return MockDataGenerator.GenerateIps(count);
                case "Dates":
                    return MockDataGenerator.GenerateDates(count);
                case "BankAccounts":
                    return MockDataGenerator.GenerateBankAccounts(count);
                case "Transactions":
                    return MockDataGenerator.GenerateTransactions(count);
                default:
                    return MockDataGenerator.GenerateUsers(count);
            }
        }
    }
}
