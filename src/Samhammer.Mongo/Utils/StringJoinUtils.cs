using System.Collections.Generic;
using System.Linq;

namespace Samhammer.Mongo.Utils
{
    public static class StringJoinUtils
    {
        public static string JoinIgnoreEmpty(string separator, params string[] stringList)
        {
            return JoinIgnoreEmpty(separator, stringList.ToList());
        }

        public static string JoinIgnoreEmpty(string separator, List<string> stringList)
        {
            var filteredList = stringList.Where(s => !string.IsNullOrEmpty(s));
            return string.Join(separator, filteredList);
        }
    }
}
