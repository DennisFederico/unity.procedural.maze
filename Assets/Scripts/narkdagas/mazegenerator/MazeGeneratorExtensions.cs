using System.Collections.Generic;
using System.Linq;

namespace narkdagas.mazegenerator {
    public static class MazeGeneratorExtensions {
        private static readonly System.Random _rng = new();

        public static void Shuffle<T>(this IList<T> list) {
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = _rng.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]); //SWAP
            }
        }

        public static IList<T> ShuffleNew<T>(this IList<T> list) {
            return list.OrderBy(a => _rng.Next()).ToList();
        }
    }
}