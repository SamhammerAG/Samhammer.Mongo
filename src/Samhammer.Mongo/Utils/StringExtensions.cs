using System.Linq;

namespace Samhammer.Mongo.Utils
{
    public static class StringExtensions
    {
        public static string RemoveString(this string value, string removeValue)
        {
            return value?.Replace(removeValue, string.Empty);
        }

        public static string ToLowerFirstChar(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.First().ToString().ToLower() + value.Substring(1);
        }

        public static string Truncate(this string value, int truncateValue)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.Length > truncateValue ? value.Substring(0, truncateValue) : value;
        }
    }
}
