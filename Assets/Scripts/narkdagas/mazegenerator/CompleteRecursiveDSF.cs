using UnityEngine;

namespace narkdagas.mazegenerator {
    public class CompleteRecursiveDSF : MazeGenerator {
        protected override void GenerateMap() {
            InnerGenerateMap(Random.Range(1, mazeSize.width - 1), Random.Range(1, mazeSize.height - 1));
        }

        private void InnerGenerateMap(int x, int z) {
            if (CountCrossNeighboursOfType(x, z) >= 2) return;
            map[x, z] = 0;

            var randomDirection = directions.ShuffleNew();
            foreach (var dir in randomDirection) {
                InnerGenerateMap(x + dir.x, z + dir.z);
            }
        }
    }
}