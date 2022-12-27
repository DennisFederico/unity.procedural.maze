using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

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
            if (markers.Count() >= 0) {
                foreach (var pathMarker in markers) {
                    if (pathMarker.mapLocation.Equals(location)) {
                        pathMarker.g = g;
                        pathMarker.h = h;
                        pathMarker.f = f;
                        pathMarker.parent = parent;
                        return;
                    }
                }
            }
            markers.Add(new PathMarker(location, g, h, f, parent));
        }

        public static byte[,] CreateOffsetCopy(this byte[,] original, int extraWidth, int extraHeight) {
            int width = original.GetLength(0);
            int height = original.GetLength(1);
            int newWidth = width + extraWidth * 2;
            int newHeight = height + extraHeight * 2;
            byte[,] newArray = new byte[newWidth,newHeight];

            for (int w = 0; w < newWidth; w++) {
                for (int h = 0; h < newHeight; h++) {
                    newArray[w, h] = 1;
                }
            }
            
            for (int w = 0; w < width; w++) {
                for (int h = 0; h < height; h++) {
                    newArray[w + extraWidth, h + extraHeight] = original[w, h];
                }
            }

            return newArray;
        }
    }
}