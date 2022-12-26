using System.Collections.Generic;
using UnityEngine;

namespace narkdagas.mazegenerator {
    public class PrimsMazeGenerator : MazeGenerator {
        protected override void GenerateMap() {
            int x = Random.Range(1, mazeConfig.width - 1);
            int z = Random.Range(1, mazeConfig.height - 1);
            map[x, z] = 0;

            List<CellLocation> walls = new List<CellLocation>();
            walls.Add(new CellLocation(x + 1, z));
            walls.Add(new CellLocation(x - 1, z));
            walls.Add(new CellLocation(x, z + 1));
            walls.Add(new CellLocation(x, z - 1));

            int countLoops = 0;
            while (walls.Count > 0) {
                Debug.Log($"Loop {countLoops}");
                int nextWall = Random.Range(0, walls.Count);
                x = walls[nextWall].x;
                z = walls[nextWall].z;
                walls.RemoveAt(nextWall);

                if (CountCrossNeighboursOfType(x, z) == 1) {
                    map[x, z] = 0;
                    walls.Add(new CellLocation(x + 1, z));
                    walls.Add(new CellLocation(x - 1, z));
                    walls.Add(new CellLocation(x, z + 1));
                    walls.Add(new CellLocation(x, z - 1));
                }

                countLoops++;
            }
        }
    }
}