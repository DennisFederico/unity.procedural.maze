using UnityEngine;

namespace narkdagas.mazegenerator {
    public class RecursiveDsf : MazeGenerator {
        public int directionRetries = 3;

        protected override void GenerateMap() {
            Debug.Log($"RecursiveDfs Generate map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            InnerGenerateMap(Random.Range(1, mazeConfig.width - 1), Random.Range(1, mazeConfig.height - 1));
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