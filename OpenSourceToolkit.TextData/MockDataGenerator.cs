using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSourceToolkit.TextData
{
    public class MockDataGenerator
    {
        // Internal helper classes for Bogus to reflect upon
        public class UserData
        {
            public Guid Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Avatar { get; set; }
        }

        // US Address format
        public class UsAddressData
        {
            public string Name { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public string ZipCode { get; set; }
            public string County { get; set; }
            public string Country { get; set; }
        }

        // German Address format
        public class DeAddressData
        {
            public string Name { get; set; }
            public string Strasse { get; set; }
            public string Plz { get; set; }
            public string Ort { get; set; }
            public string Bundesland { get; set; }
            public string Land { get; set; }
        }

        // UK Address format
        public class GbAddressData
        {
            public string Name { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string County { get; set; }
            public string Postcode { get; set; }
            public string Country { get; set; }
        }

        // Japanese Address format
        public class JpAddressData
        {
            public string Name { get; set; }
            public string PostalCode { get; set; }
            public string Prefecture { get; set; }
            public string City { get; set; }
            public string Address { get; set; }
            public string Country { get; set; }
        }

        public class CompanyData
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Industry { get; set; }
            public int EmployeeCount { get; set; }
            public int FoundedYear { get; set; }
            public string Website { get; set; }
        }

        public class CreditCardData
        {
            public string Brand { get; set; }
            public string Last4 { get; set; }
            public int ExpMonth { get; set; }
            public int ExpYear { get; set; }
        }

        public class ProductData
        {
            public string Sku { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public decimal Price { get; set; }
            public string ImageUrl { get; set; }
        }

        public class OrderData
        {
            public Guid OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public string Status { get; set; }
            public int ItemsCount { get; set; }
            public decimal Total { get; set; }
        }

        public class ReviewData
        {
            public int Rating { get; set; }
            public string Title { get; set; }
            public string Body { get; set; }
            public string Author { get; set; }
            public DateTime Date { get; set; }
        }

        public class BlogPostData
        {
            public string Title { get; set; }
            public string Excerpt { get; set; }
            public string Author { get; set; }
            public DateTime PublishDate { get; set; }
            public string[] Tags { get; set; }
        }

        public class CommentData
        {
            public string Author { get; set; }
            public string Body { get; set; }
            public DateTime Timestamp { get; set; }
            public int Likes { get; set; }
        }

        public class UuidData
        {
            public Guid Uuid { get; set; }
        }

        public class EmailData
        {
            public string Email { get; set; }
        }

        public class UrlData
        {
            public string Url { get; set; }
            public string Domain { get; set; }
        }

        public class IpData
        {
            public string Ipv4 { get; set; }
            public string Ipv6 { get; set; }
        }

        public class DateSampleData
        {
            public DateTime Past { get; set; }
            public DateTime Future { get; set; }
            public DateTime Recent { get; set; }
        }

        public class BankAccountData
        {
            public string AccountHolder { get; set; }
            public string Iban { get; set; }
            public string Bic { get; set; }
        }

        public class TransactionData
        {
            public Guid Id { get; set; }
            public DateTime Date { get; set; }
            public decimal Amount { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public string Type { get; set; }
        }

        public static IEnumerable<dynamic> GenerateUsers(int count, string locale = "en")
        {
            var faker = new Faker<UserData>(locale)
                .RuleFor(u => u.Id, f => f.Random.Guid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.Avatar, f => f.Internet.Avatar());

            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateUsAddresses(int count)
        {
            var faker = new Faker<UsAddressData>("en_US")
                .RuleFor(a => a.Name, f => f.Name.FullName())
                .RuleFor(a => a.Street, f => f.Address.StreetAddress())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.State, f => f.Address.StateAbbr())
                .RuleFor(a => a.ZipCode, f => f.Address.ZipCode("#####"))
                .RuleFor(a => a.County, f => f.Address.County())
                .RuleFor(a => a.Country, _ => "United States");

            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateDeAddresses(int count)
        {
            var faker = new Faker<DeAddressData>("de")
                .RuleFor(a => a.Name, f => f.Name.FullName())
                .RuleFor(a => a.Strasse, f => f.Address.StreetAddress())
                .RuleFor(a => a.Plz, f => f.Random.Replace("#####"))
                .RuleFor(a => a.Ort, f => f.Address.City())
                .RuleFor(a => a.Bundesland, f => f.Address.State())
                .RuleFor(a => a.Land, _ => "Deutschland");

            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateGbAddresses(int count)
        {
            var faker = new Faker<GbAddressData>("en_GB")
                .RuleFor(a => a.Name, f => f.Name.FullName())
                .RuleFor(a => a.Street, f => f.Address.StreetAddress())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.County, f => f.Address.County())
                .RuleFor(a => a.Postcode, f => f.Address.ZipCode())
                .RuleFor(a => a.Country, _ => "United Kingdom");

            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateJpAddresses(int count)
        {
            var faker = new Faker<JpAddressData>("ja")
                .RuleFor(a => a.Name, f => f.Name.FullName())
                .RuleFor(a => a.PostalCode, f => f.Address.ZipCode("###-####"))
                .RuleFor(a => a.Prefecture, f => f.Address.State())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address, f => f.Address.StreetAddress())
                .RuleFor(a => a.Country, _ => "日本");

            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateCompanies(int count, string locale = "en")
        {
            var faker = new Faker<CompanyData>(locale)
                .RuleFor(c => c.Id, f => f.Random.Guid())
                .RuleFor(c => c.Name, f => f.Company.CompanyName())
                .RuleFor(c => c.Industry, f => f.Commerce.Department())
                .RuleFor(c => c.EmployeeCount, f => f.Random.Int(5, 5000))
                .RuleFor(c => c.FoundedYear, f => f.Date.Past(50).Year)
                .RuleFor(c => c.Website, f => f.Internet.Url());
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateCreditCards(int count, string locale = "en")
        {
            var brands = new[] { "Visa", "MasterCard", "American Express", "Discover", "Diners Club", "JCB", "Maestro" };
            var faker = new Faker<CreditCardData>(locale)
                .RuleFor(c => c.Brand, f => f.PickRandom(brands))
                .RuleFor(c => c.Last4, f =>
                {
                    var num = f.Finance.CreditCardNumber();
                    var digits = new string(num.Where(char.IsDigit).ToArray());
                    return digits.Length >= 4 ? digits.Substring(digits.Length - 4) : digits;
                })
                .RuleFor(c => c.ExpMonth, f => f.Random.Int(1, 12))
                .RuleFor(c => c.ExpYear, f => f.Date.Future(5).Year);
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateProducts(int count, string locale = "en")
        {
            var faker = new Faker<ProductData>(locale)
                .RuleFor(p => p.Sku, f => f.Commerce.Ean13())
                .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                .RuleFor(p => p.Category, f => f.Commerce.Categories(1).First())
                .RuleFor(p => p.Price, f => decimal.Parse(f.Commerce.Price(1, 999, 2)))
                .RuleFor(p => p.ImageUrl, f => f.Image.PicsumUrl());
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateOrders(int count, string locale = "en")
        {
            var statuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
            var faker = new Faker<OrderData>(locale)
                .RuleFor(o => o.OrderId, f => f.Random.Guid())
                .RuleFor(o => o.OrderDate, f => f.Date.Past(2))
                .RuleFor(o => o.Status, f => f.PickRandom(statuses))
                .RuleFor(o => o.ItemsCount, f => f.Random.Int(1, 7))
                .RuleFor(o => o.Total, f => Math.Round(f.Random.Decimal(10, 2000), 2));
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateReviews(int count, string locale = "en")
        {
            var faker = new Faker<ReviewData>(locale)
                .RuleFor(r => r.Rating, f => f.Random.Int(1, 5))
                .RuleFor(r => r.Title, f => f.Lorem.Sentence(3))
                .RuleFor(r => r.Body, f => f.Lorem.Paragraph())
                .RuleFor(r => r.Author, f => f.Name.FullName())
                .RuleFor(r => r.Date, f => f.Date.Recent(120));
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateBlogPosts(int count, string locale = "en")
        {
            var faker = new Faker<BlogPostData>(locale)
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(5))
                .RuleFor(b => b.Excerpt, f => f.Lorem.Paragraph())
                .RuleFor(b => b.Author, f => f.Name.FullName())
                .RuleFor(b => b.PublishDate, f => f.Date.Past(2))
                .RuleFor(b => b.Tags, f => f.Lorem.Words(f.Random.Int(2, 5)).ToArray());
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateComments(int count, string locale = "en")
        {
            var faker = new Faker<CommentData>(locale)
                .RuleFor(c => c.Author, f => f.Name.FullName())
                .RuleFor(c => c.Body, f => f.Lorem.Sentences(f.Random.Int(1, 3)))
                .RuleFor(c => c.Timestamp, f => f.Date.Recent(60))
                .RuleFor(c => c.Likes, f => f.Random.Int(0, 500));
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateUuids(int count)
        {
            var faker = new Faker<UuidData>()
                .RuleFor(u => u.Uuid, f => f.Random.Guid());
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateEmails(int count, string locale = "en")
        {
            var faker = new Faker<EmailData>(locale)
                .RuleFor(e => e.Email, f => f.Internet.Email());
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateUrls(int count, string locale = "en")
        {
            var faker = new Faker<UrlData>(locale)
                .RuleFor(u => u.Url, f => f.Internet.Url())
                .RuleFor(u => u.Domain, f => f.Internet.DomainName());
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateIps(int count, string locale = "en")
        {
            var faker = new Faker<IpData>(locale)
                .RuleFor(i => i.Ipv4, f => f.Internet.Ip())
                .RuleFor(i => i.Ipv6, f => f.Internet.Ipv6());
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateDates(int count)
        {
            var faker = new Faker<DateSampleData>()
                .RuleFor(d => d.Past, f => f.Date.Past(10))
                .RuleFor(d => d.Future, f => f.Date.Future(10))
                .RuleFor(d => d.Recent, f => f.Date.Recent(30));
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateBankAccounts(int count, string locale = "en")
        {
            var faker = new Faker<BankAccountData>(locale)
                .RuleFor(b => b.AccountHolder, f => f.Name.FullName())
                .RuleFor(b => b.Iban, f => f.Finance.Iban())
                .RuleFor(b => b.Bic, f => f.Finance.Bic());
            return faker.Generate(count).Cast<dynamic>();
        }

        public static IEnumerable<dynamic> GenerateTransactions(int count, string locale = "en")
        {
            var faker = new Faker<TransactionData>(locale)
                .RuleFor(t => t.Id, f => f.Random.Guid())
                .RuleFor(t => t.Date, f => f.Date.Recent(365))
                .RuleFor(t => t.Amount, f => Math.Round(f.Random.Decimal(-500, 5000), 2))
                .RuleFor(t => t.Description, f => f.Commerce.ProductName())
                .RuleFor(t => t.Category, f => f.Commerce.Department())
                .RuleFor(t => t.Type, f => f.Finance.TransactionType());
            return faker.Generate(count).Cast<dynamic>();
        }
    }
}
