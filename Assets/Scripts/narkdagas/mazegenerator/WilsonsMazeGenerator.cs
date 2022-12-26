using System.Collections.Generic;
using UnityEngine;

namespace narkdagas.mazegenerator {
    public class WilsonsMazeGenerator : MazeGenerator {

        private List<CellLocation> _availableStartCells = new();

        protected override void GenerateMap() {
            // Starting Cell
            int x = Random.Range(1, mazeConfig.width - 1);
            int z = Random.Range(1, mazeConfig.height - 1);
            map[x, z] = (byte)CellLocationType.Maze;

            int tries = 0;
            while (GetAvailableCells() > 1 && tries < 5000) {
                RandomWalk();
                tries++;
            }

            Debug.Log($"Used {tries} tries");
        }

        private int GetAvailableCells() {
            _availableStartCells.Clear();
            for (int z = 1; z < mazeConfig.height - 1; z++) {
                for (int x = 1; x < mazeConfig.width - 1; x++) {
                    if (map[x, z] == (int)CellLocationType.Wall &&
                        CountCrossNeighboursOfType(x, z) == 0 &&
                        CountCrossNeighboursOfType(x, z, (byte) CellLocationType.Maze) == 0) {
                        _availableStartCells.Add(new CellLocation(x, z));
                    }
                }
            }

            return _availableStartCells.Count;
        }


        void RandomWalk() {
            var startPosition = Random.Range(0, _availableStartCells.Count);
            int x = _availableStartCells[startPosition].x;
            int z = _availableStartCells[startPosition].z;

            List<CellLocation> inWalk = new();
            bool isValidPath = false;
            int retry = 0;

            while (IsInsideMaze(x, z) && retry < 10) {

                if (retry == 0) {
                    map[x, z] = (byte)CellLocationType.Corridor;
                    inWalk.Add(new CellLocation(x, z));
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
                inWalk.Add(new CellLocation(x, z));
                Debug.Log("Found Path");
                foreach (CellLocation cellInfo in inWalk) {
                    map[cellInfo.x, cellInfo.z] = 2;
                }
            } else {
                foreach (CellLocation cellInfo in inWalk) {
                    map[cellInfo.x, cellInfo.z] = (byte) CellLocationType.Wall;
                }
            }

            inWalk.Clear();
        }
    }
}