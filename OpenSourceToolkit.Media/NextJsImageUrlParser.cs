using System;
using System.Web;

namespace OpenSourceToolkit.Media
{
    public static class NextJsImageUrlParser
    {
        public class NextJsImageInfo
        {
            public string OriginalUrl { get; set; }
            public int Width { get; set; }
            public int Quality { get; set; }
            public bool IsValid { get; set; }
        }

        public static NextJsImageInfo Parse(string url)
        {
            try
            {
                var uri = new Uri(url);
                if (!uri.AbsolutePath.Contains("/_next/image"))
                {
                    return new NextJsImageInfo { IsValid = false };
                }

                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var originalUrl = query.Get("url");
                var w = query.Get("w");
                var q = query.Get("q");

                if (string.IsNullOrEmpty(originalUrl) || string.IsNullOrEmpty(w) || string.IsNullOrEmpty(q))
                {
                    return new NextJsImageInfo { IsValid = false };
                }

                return new NextJsImageInfo
                {
                    OriginalUrl = originalUrl,
                    Width = int.Parse(w),
                    Quality = int.Parse(q),
                    IsValid = true
                };
            }
            catch
            {
                return new NextJsImageInfo { IsValid = false };
            }
        }

        public static string Generate(string baseUrl, string imageUrl, int width, int quality)
        {
            var encodedUrl = System.Web.HttpUtility.UrlEncode(imageUrl);
            return $"{baseUrl.TrimEnd('/')}/_next/image?url={encodedUrl}&w={width}&q={quality}";
        }
    }
}
