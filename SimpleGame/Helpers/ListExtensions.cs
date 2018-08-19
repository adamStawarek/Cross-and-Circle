using System.Collections.Generic;

namespace SimpleGame.Helpers
{
    public static class ListExtensions
    {
        public static bool ContainsArray<T>(this IEnumerable<T> lst, T[] arr)
        {
            List<T> tmp = new List<T>(arr);
            foreach (var j in lst)
            {
                if (tmp.Contains(j))
                    tmp.Remove(j);
            }

            return tmp.Count == 0;

        }
    }
}