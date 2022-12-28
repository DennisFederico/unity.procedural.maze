using UnityEngine;

namespace narkdagas.mazegenerator {
    public class CompleteRecursiveDSF : Maze {
        protected override void GenerateMap() {
            InnerGenerateMap(Random.Range(1, mazeConfig.width - 1), Random.Range(1, mazeConfig.height - 1));
        }

        private void InnerGenerateMap(int x, int z) {
            if (CountCrossNeighboursOfType(x, z) >= 2) return;
            map[x, z] = 0;

            var randomDirection = directions.ShuffleAsNewList();
            foreach (var dir in randomDirection) {
                InnerGenerateMap(x + dir.x, z + dir.z);
            }
        }
    }
}