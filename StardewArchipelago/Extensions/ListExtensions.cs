using System;
using System.Collections.Generic;
using System.Linq;

namespace StardewArchipelago.Extensions
{
    public static class ListExtensions
    {
        public static List<T> Shuffle<T>(this IList<T> list, Random random)
        {
            var elements = list.ToList();
            var newList = new List<T>();
            while (elements.Any())
            {
                var index = random.Next(0, elements.Count);
                var element = elements[index];
                newList.Add(element);
                elements.RemoveAt(index);
            }

            return newList;
        }
    }
}
