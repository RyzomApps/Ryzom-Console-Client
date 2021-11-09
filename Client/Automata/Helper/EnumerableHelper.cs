using System;
using System.Collections.Generic;
using System.Linq;

namespace Client.Automata.Helper
{
    public static class EnumerableHelper<E>
    {
        private static readonly Random r;

        static EnumerableHelper()
        {
            r = new Random();
        }

        public static T Random<T>(IEnumerable<T> input)
        {
            return input.ElementAt(r.Next(input.Count()));
        }
    }
}