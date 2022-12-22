using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace narkdagas.mazegenerator {
    public static class MazeGeneratorExtensions {
        private static readonly System.Random Rnd = new();

        public static void ShuffleCurrent<T>(this IList<T> list) {
            var n = list.Count;
            while (n > 1) {
                n--;
                var k = Rnd.Next(n + 1);
                (list[k], list[n]) = (list[n], list[k]); //SWAP
            }
        }

        public static IList<T> ShuffleAsNewList<T>(this IList<T> list) {
            return list.OrderBy(a => Rnd.Next()).ToList();
        }

        public static GameObject GetRandomPiece(this GameObject[] pieces) {
            if (pieces.Length == 1) return pieces[0];
            return pieces[Random.Range(0, pieces.Length)];
        }
    }
}