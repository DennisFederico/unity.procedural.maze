using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace narkdagas.mazegenerator {
    public class MazeGenerator : MonoBehaviour {
        [SerializeField] protected MazeDimension mazeSize;
        [SerializeField] protected int scale = 6;
        protected byte[,] map;

        protected List<MazeCellInfo> directions = new() {
            new MazeCellInfo(1, 0),
            new MazeCellInfo(0, 1),
            new MazeCellInfo(-1, 0),
            new MazeCellInfo(0, -1)
        };

        [Serializable]
        protected struct MazeDimension {
            public int width;
            public int height;
        }

        public struct MazeCellInfo {
            public int x, z;
            public byte wallType;
            public const byte WALL = 1;
            public const byte CORRIDOR = 0;
            public const byte MAZE = 2;

            public MazeCellInfo(int x, int z, byte wallType = WALL) {
                this.x = x;
                this.z = z;
                this.wallType = wallType;
            }
        }

        // Start is called before the first frame update
        private void Start() {
            InitializeMap();
            GenerateMap();
            DrawMap();
        }

        private void InitializeMap() {
            map = new byte[(int)mazeSize.width, (int)mazeSize.height];
            for (int z = 0; z < mazeSize.height; z++) {
                for (int x = 0; x < mazeSize.width; x++) {
                    map[x, z] = 1;
                }
            }
        }

        public virtual void GenerateMap() {
            map = new byte[(int)mazeSize.width, (int)mazeSize.height];
            for (int z = 0; z < mazeSize.height; z++) {
                for (int x = 0; x < mazeSize.width; x++) {
                    map[x, z] = (byte)(Random.Range(0, 100) < 50 ? 0 : 1); //1 = wall, 0 = corridor
                }
            }
        }

        protected void DrawMap() {
            for (int z = 0; z < mazeSize.height; z++) {
                for (int x = 0; x < mazeSize.width; x++) {
                    if (map[x, z] == MazeCellInfo.WALL) {
                        Vector3 pos = new Vector3(x * scale, 0, z * scale);
                        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        wall.transform.localScale = new Vector3(scale, scale, scale);
                        wall.transform.position = pos;
                        //Instantiate(cube, pos, Quaternion.identity);
                    }
                }
            }
        }

        protected int CountCrossNeighboursOfType(int x, int z, byte type = MazeCellInfo.CORRIDOR) {
            if (IsOutsideMaze(x, z)) return 5;

            int count = 0;
            count += map[x - 1, z] == type ? 1 : 0;
            count += map[x + 1, z] == type ? 1 : 0;
            count += map[x, z + 1] == type ? 1 : 0;
            count += map[x, z - 1] == type ? 1 : 0;
            return count;
        }

        protected int CountDiagonalNeighboursOfType(int x, int z, byte type = MazeCellInfo.CORRIDOR) {
            if (IsOutsideMaze(x, z)) return 5;

            int count = 0;
            count += map[x - 1, z + 1] == type ? 1 : 0;
            count += map[x - 1, z - 1] == type ? 1 : 0;
            count += map[x + 1, z + 1] == type ? 1 : 0;
            count += map[x + 1, z - 1] == type ? 1 : 0;
            return count;
        }

        protected int CountAllNeighboursCorridors(int x, int z, byte type) {
            return CountCrossNeighboursOfType(x, z, type) + CountDiagonalNeighboursOfType(x, z, type);
        }

        protected bool IsInsideMaze(MazeCellInfo cellInfo) {
            return IsInsideMaze(cellInfo.x, cellInfo.z);
        }

        protected bool IsOutsideMaze(MazeCellInfo cellInfo) {
            return IsOutsideMaze(cellInfo.x, cellInfo.z);
        }

        protected bool IsInsideMaze(int x, int z) {
            return x > 0 &&
                   x < mazeSize.width - 1 &&
                   z > 0 &&
                   z < mazeSize.height - 1;
        }

        protected bool IsOutsideMaze(int x, int z) {
            bool result = x <= 0 ||
                          x >= mazeSize.width - 1 ||
                          z <= 0 ||
                          z >= mazeSize.height - 1;
            return result;
        }
    }
}