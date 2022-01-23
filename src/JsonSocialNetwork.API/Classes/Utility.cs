using System.Collections;
using System.Collections.Generic;

namespace JsonSocialNetwork.API.Classes
{
    public static class Utility
    {
        public static List<TSource> GetRange<TSource>(this IEnumerable<TSource> source, int index, int count)
        {
            int i = 0, c = 0;
            List<TSource> result = new();
            foreach (TSource item in source)
            {
                if (i >= index && c < count)
                {
                    result.Add(item);
                    c++;
                }
                i++;
            }
            return result;
        }
    }
}
