using UnityEngine;

namespace narkdagas.mazegenerator {
    public class RecursiveDSF : MazeGenerator {
        public int directionRetries = 3;

        public override void GenerateMap() {
            InnerGenerateMap(Random.Range(1, mazeSize.width - 1), Random.Range(1, mazeSize.height - 1));
        }

        private void InnerGenerateMap(int x, int z) {
            if (CountCrossNeighboursOfType(x, z) >= 2) return;
            map[x, z] = 0;

            var tries = directionRetries;
            while (tries > 0) {
                var dirIndex = Random.Range(0, directions.Count);
                InnerGenerateMap(x + directions[dirIndex].x, z + directions[dirIndex].z);
                tries--;
            }
        }
    }
}