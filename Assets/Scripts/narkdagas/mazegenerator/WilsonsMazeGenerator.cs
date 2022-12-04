using System.Collections.Generic;
using UnityEngine;

namespace narkdagas.mazegenerator {
    public class WilsonsMazeGenerator : MazeGenerator {

        private List<MazeCellInfo> _availableStartCells = new();

        protected override void GenerateMap() {
            // Starting Cell
            int x = Random.Range(1, mazeSize.width - 1);
            int z = Random.Range(1, mazeSize.height - 1);
            map[x, z] = MazeCellInfo.MAZE;

            int tries = 0;
            while (GetAvailableCells() > 1 && tries < 5000) {
                RandomWalk();
                tries++;
            }

            Debug.Log($"Used {tries} tries");
        }

        private int GetAvailableCells() {
            _availableStartCells.Clear();
            for (int z = 1; z < mazeSize.height - 1; z++) {
                for (int x = 1; x < mazeSize.width - 1; x++) {
                    if (map[x, z] == MazeCellInfo.WALL &&
                        CountCrossNeighboursOfType(x, z) == 0 &&
                        CountCrossNeighboursOfType(x, z, MazeCellInfo.MAZE) == 0) {
                        _availableStartCells.Add(new MazeCellInfo(x, z));
                    }
                }
            }

            return _availableStartCells.Count;
        }


        void RandomWalk() {
            var startPosition = Random.Range(0, _availableStartCells.Count);
            int x = _availableStartCells[startPosition].x;
            int z = _availableStartCells[startPosition].z;

            List<MazeCellInfo> inWalk = new();
            bool isValidPath = false;
            int retry = 0;

            while (IsInsideMaze(x, z) && retry < 10) {

                if (retry == 0) {
                    map[x, z] = MazeCellInfo.CORRIDOR;
                    inWalk.Add(new MazeCellInfo(x, z));
                }

                int randomDirection = Random.Range(0, directions.Count);
                retry++;
                int nx = x + directions[randomDirection].x;
                int nz = z + directions[randomDirection].z;

                if (CountCrossNeighboursOfType(nx, nz) < 2) {
                    x = nx;
                    z = nz;
                    retry = 0;
                }

                //Have we hit a maze?
                if (CountCrossNeighboursOfType(x, z, 2) == 1) {
                    isValidPath = true;
                    break;
                }

                //Have we hit the maze in more than one place?
                if (CountCrossNeighboursOfType(x, z, 2) > 1) break;
            }

            if (isValidPath) {
                inWalk.Add(new MazeCellInfo(x, z, MazeCellInfo.CORRIDOR));
                Debug.Log("Found Path");
                foreach (MazeCellInfo cellInfo in inWalk) {
                    map[cellInfo.x, cellInfo.z] = 2;
                }
            } else {
                foreach (MazeCellInfo cellInfo in inWalk) {
                    map[cellInfo.x, cellInfo.z] = MazeCellInfo.WALL;
                }
            }

            inWalk.Clear();
        }
    }
}