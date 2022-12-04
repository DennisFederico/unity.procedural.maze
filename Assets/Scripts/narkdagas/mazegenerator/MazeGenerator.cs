using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace narkdagas.mazegenerator {
    public class MazeGenerator : MonoBehaviour {
        [SerializeField] protected MazeDimension mazeSize;
        [SerializeField] protected int scale = 6;
        protected byte[,] map;
        [SerializeField] protected GameObject playerPrefab;
        [SerializeField] protected GameObject[] straightPieces;
        [SerializeField] protected GameObject[] cornerPieces;
        [SerializeField] protected GameObject[] deadEndPieces;
        [SerializeField] protected GameObject[] junctionPieces;
        [SerializeField] protected GameObject[] intersectionPieces;

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
            AddRooms(3, 4, 6);
            DrawMap();
            PlacePlayer();
        }

        private void InitializeMap() {
            map = new byte[mazeSize.width, mazeSize.height];
            for (int z = 0; z < mazeSize.height; z++) {
                for (int x = 0; x < mazeSize.width; x++) {
                    map[x, z] = 1;
                }
            }
        }

        protected virtual void GenerateMap() {
            map = new byte[mazeSize.width, mazeSize.height];
            for (int z = 0; z < mazeSize.height; z++) {
                for (int x = 0; x < mazeSize.width; x++) {
                    map[x, z] = (byte)(Random.Range(0, 100) < 50 ? MazeCellInfo.CORRIDOR : MazeCellInfo.WALL);
                }
            }
        }

        protected virtual void AddRooms(int numRooms, int minSize, int maxSize) {
            for (int room = 1; room <= numRooms; room++) {
                var width = Random.Range(minSize, maxSize + 1);
                var height = Random.Range(minSize, maxSize + 1);
                var startX = Random.Range(3, mazeSize.width - width - 3);
                var startZ = Random.Range(3, mazeSize.height - height - 3);

                for (var x = startX; x <= startX + width; x++) {
                    for (var z = startZ; z <= startZ + height; z++) {
                        map[x, z] = MazeCellInfo.CORRIDOR;
                    }
                }
            }
        }
        
        private void DrawMap() {
            for (int z = 0; z < mazeSize.height; z++) {
                for (int x = 0; x < mazeSize.width; x++) {
                    Vector3 pos = new Vector3(x * scale, 0, z * scale);
                    if (map[x, z] == MazeCellInfo.CORRIDOR) {
                        var neighbours = map.GetCrossNeighboursForMazePiece(x, z);
                        switch (neighbours.MatchMazePiece()) {
                            case MazePiece.CorridorHorizontal:
                                Instantiate(straightPieces.GetRandomPiece(), pos, Quaternion.Euler(0,90,0));
                                break;
                            case MazePiece.CorridorVertical:
                                Instantiate(straightPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case MazePiece.CornerTopRight:
                                Instantiate(cornerPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case MazePiece.CornerTopLeft:
                                Instantiate(cornerPieces.GetRandomPiece(), pos, Quaternion.Euler(0,270,0));
                                break;
                            case MazePiece.CornerBottomRight:
                                Instantiate(cornerPieces.GetRandomPiece(), pos, Quaternion.Euler(0,90,0));
                                break;
                            case MazePiece.CornerBottomLeft:
                                Instantiate(cornerPieces.GetRandomPiece(), pos, Quaternion.Euler(0,180,0));
                                break;
                            case MazePiece.DeadEndTop:
                                Instantiate(deadEndPieces.GetRandomPiece(), pos, Quaternion.Euler(0,180,0));
                                break;
                            case MazePiece.DeadEndRight:
                                Instantiate(deadEndPieces.GetRandomPiece(), pos, Quaternion.Euler(0,270,0));
                                break;
                            case MazePiece.DeadEndLeft:
                                Instantiate(deadEndPieces.GetRandomPiece(), pos, Quaternion.Euler(0,90,0));
                                break;
                            case MazePiece.DeadEndBottom:
                                Instantiate(deadEndPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case MazePiece.JunctionTop:
                                Instantiate(junctionPieces.GetRandomPiece(), pos, Quaternion.Euler(0,270,0));
                                break;
                            case MazePiece.JunctionRight:
                                Instantiate(junctionPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case MazePiece.JunctionBottom:
                                Instantiate(junctionPieces.GetRandomPiece(), pos, Quaternion.Euler(0,90,0));
                                break;
                            case MazePiece.JunctionLeft:
                                Instantiate(junctionPieces.GetRandomPiece(), pos, Quaternion.Euler(0,180,0));
                                break;
                            case MazePiece.Intersection:
                                Instantiate(intersectionPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case MazePiece.UnknownPiece:
                                var primitive = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                primitive.transform.localScale = Vector3.one * 6;
                                primitive.transform.position = pos;
                                break;
                        }
                    }
                }
            }
        }

        private void PlacePlayer() {
            for (int x = 1; x < mazeSize.width - 1; x++) {
                for (int z = 1; z < mazeSize.height - 1; z++) {
                    if (map[x, z] == MazeCellInfo.CORRIDOR) {
                        Instantiate(playerPrefab, new Vector3(x * 6, 0, z * 6), Quaternion.identity);
                        return;
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