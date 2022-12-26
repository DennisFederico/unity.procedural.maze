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
            return list.OrderBy(_ => Rnd.Next()).ToList();
        }

        public static GameObject GetRandomPiece(this GameObject[] pieces) {
            if (pieces.Length == 1) return pieces[0];
            return pieces[Random.Range(0, pieces.Length)];
        }
        
        public static bool ContainsMapLocation(this IList<PathMarker> markers, MazeGenerator.CellLocation location) {
            foreach (var pathMarker in markers) {
                if (pathMarker.mapLocation.Equals(location)) return true;
            }

            return false;
        }
        
        public static void UpdateOrAdd(this IList<PathMarker> markers, MazeGenerator.CellLocation location, float g, float h, float f, PathMarker parent) {
            foreach (var pathMarker in markers) {
                if (pathMarker.mapLocation.Equals(location)) {
                    pathMarker.g = g;
                    pathMarker.h = h;
                    pathMarker.f = f;
                    pathMarker.parent = parent;
                    return;
                }
            }

            markers.Add(new PathMarker(location, g, h, f, parent));
        }
    }
}