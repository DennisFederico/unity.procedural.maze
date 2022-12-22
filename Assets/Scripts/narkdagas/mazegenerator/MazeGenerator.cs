using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace narkdagas.mazegenerator {
    public class MazeGenerator : MonoBehaviour {
        [SerializeField] protected int genSeed = 432912345;
        [SerializeField] private float pieceScale = 6;
        [SerializeField] private float heightScale = 2f;
        [SerializeField] protected GameObject playerPrefab;
        [SerializeField] protected GameObject flooredCeiling;
        [SerializeField] protected GameObject[] wallPieces;
        [SerializeField] protected GameObject[] doorwayPieces;
        [SerializeField] protected GameObject[] pillarPieces;
        [SerializeField] protected GameObject[] straightPieces;
        [SerializeField] protected GameObject[] cornerPieces;
        [SerializeField] protected GameObject[] deadEndPieces;
        [SerializeField] protected GameObject[] junctionPieces;
        [SerializeField] protected GameObject[] intersectionPieces;
        [SerializeField] private byte numRooms = 3;
        [SerializeField] public Vector2Int roomSizeRange = new(3, 6);
        [SerializeField] public Vector2Int numLaddersRange = new (1,3);
        [SerializeField] protected bool placePlayer;
        public MazeConfig mazeConfig;
        protected byte[,] map;
        public PieceData[,] pieces;

        protected readonly List<MazeCellInfo> directions = new() {
            new MazeCellInfo(1, 0),
            new MazeCellInfo(0, 1),
            new MazeCellInfo(-1, 0),
            new MazeCellInfo(0, -1)
        };

        [Serializable]
        public struct MazeConfig {
            public byte width;
            public byte height;
            public byte level;
            public float pieceScale;
            public float heightScale;
            public byte numRooms;
            public bool placePlayer;

            public MazeConfig(byte width, byte height, byte level = 0, float pieceScale = 6, float heightScale = 2, byte numRooms = 0, bool placePlayer = false) {
                this.width = width;
                this.height = height;
                this.level = level;
                this.pieceScale = pieceScale;
                this.heightScale = heightScale;
                this.numRooms = numRooms;
                this.placePlayer = placePlayer;
            }
        }

        public struct MazeCellInfo {
            public readonly int x;
            public readonly int z;
            public const byte Wall = 1;
            public const byte Corridor = 0;
            public const byte Maze = 2;
            public const byte Any = 9;

            public MazeCellInfo(int x, int z) {
                this.x = x;
                this.z = z;
            }
        }

        //The relative position of neighbours
        // MATCHING CLOCKWISE FROM TOP-LEFT
        //          |---|---|---|
        //          | 0 | 1 | 2 |
        //          |---|---|---|
        //          | 7 | X | 3 |
        //          |---|---|---|
        //          | 6 | 5 | 4 |
        //          |---|---|---|
        protected readonly Vector3[] neighbourRelativeCoords = {
            new(-1, 0, 1),
            new(0, 0, 1),
            new(1, 0, 1),
            new(1, 0, 0),
            new(1, 0, -1),
            new(0, 0, -1),
            new(-1, 0, -1),
            new(-1, 0, 0)
        };

        // private void Awake() {
        //     //Random.InitState(genSeed);
        // }

        // Start is called before the first frame update
        public void Build(byte width, byte height, byte level) {
            mazeConfig = new MazeConfig(width, height, level, pieceScale, heightScale, numRooms, placePlayer);
            InitializeMap();
            GenerateMap();
            AddRooms(numRooms, roomSizeRange.x, roomSizeRange.y);
            DrawMap();
            if (placePlayer) PlacePlayer();
        }

        private void InitializeMap() {
            Debug.Log($"Initializing map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            map = new byte[mazeConfig.width, mazeConfig.height];
            pieces = new PieceData[mazeConfig.width, mazeConfig.height];
            for (int z = 0; z < mazeConfig.height; z++) {
                for (int x = 0; x < mazeConfig.width; x++) {
                    map[x, z] = 1;
                }
            }
        }

        protected virtual void GenerateMap() {
            Debug.Log($"Generate map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            for (int z = 0; z < mazeConfig.height; z++) {
                for (int x = 0; x < mazeConfig.width; x++) {
                    map[x, z] = (Random.Range(0, 100) < 50 ? MazeCellInfo.Corridor : MazeCellInfo.Wall);
                }
            }
        }

        protected virtual void AddRooms(int numberOfRooms, int minSize, int maxSize) {
            Debug.Log($"Adding {numberOfRooms} Rooms for map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            for (int room = 1; room <= numberOfRooms; room++) {
                var width = Random.Range(minSize, maxSize + 1);
                var height = Random.Range(minSize, maxSize + 1);
                var startX = Random.Range(3, mazeConfig.width - width - 3);
                var startZ = Random.Range(3, mazeConfig.height - height - 3);

                Debug.Log($"Adding room {room} - Size:[{width}][{height}] - Start:[{startX}][{startZ}]");
                
                for (var x = startX; x <= startX + width; x++) {
                    for (var z = startZ; z <= startZ + height; z++) {
                        map[x, z] = MazeCellInfo.Corridor;
                    }
                }
            }
        }

        private void DrawMap() {
            Debug.Log($"Drawing map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            float height = mazeConfig.level * pieceScale * heightScale;
            Dictionary<Vector3, bool> pillarsLocations = new Dictionary<Vector3, bool>();
            for (byte z = 0; z < mazeConfig.height; z++) {
                for (byte x = 0; x < mazeConfig.width; x++) {
                    Vector3 pos = new Vector3(x * pieceScale, height, z * pieceScale);
                    if (map[x, z] == MazeCellInfo.Corridor) {
                        var neighbours = map.GetAllNeighboursForMazePiece(x, z);
                        PieceType pieceType = neighbours.MatchMazePiece();
                        GameObject pieceInstance = null;
                        switch (pieceType) {
                            case PieceType.CorridorHorizontal:
                                pieceInstance = Instantiate(straightPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 90, 0));
                                break;
                            case PieceType.CorridorVertical:
                                pieceInstance = Instantiate(straightPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case PieceType.CornerTopRight:
                                pieceInstance = Instantiate(cornerPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case PieceType.CornerTopLeft:
                                pieceInstance = Instantiate(cornerPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 270, 0));
                                break;
                            case PieceType.CornerBottomRight:
                                pieceInstance = Instantiate(cornerPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 90, 0));
                                break;
                            case PieceType.CornerBottomLeft:
                                pieceInstance = Instantiate(cornerPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 180, 0));
                                break;
                            case PieceType.DeadEndTop:
                                pieceInstance = Instantiate(deadEndPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 180, 0));
                                break;
                            case PieceType.DeadEndRight:
                                pieceInstance = Instantiate(deadEndPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 270, 0));
                                break;
                            case PieceType.DeadEndLeft:
                                pieceInstance = Instantiate(deadEndPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 90, 0));
                                break;
                            case PieceType.DeadEndBottom:
                                pieceInstance = Instantiate(deadEndPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case PieceType.JunctionTop:
                                pieceInstance = Instantiate(junctionPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 270, 0));
                                break;
                            case PieceType.JunctionRight:
                                pieceInstance = Instantiate(junctionPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case PieceType.JunctionBottom:
                                pieceInstance = Instantiate(junctionPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 90, 0));
                                break;
                            case PieceType.JunctionLeft:
                                pieceInstance = Instantiate(junctionPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 180, 0));
                                break;
                            case PieceType.Intersection:
                                pieceInstance = Instantiate(intersectionPieces.GetRandomPiece(), pos, Quaternion.identity);
                                break;
                            case PieceType.OpenRoom:
                                pieceInstance = Instantiate(flooredCeiling, pos, Quaternion.identity);
                                break;
                            case PieceType.Custom:
                                //DrawCustomPieceOrig(pos, map.GetCrossNeighboursForMazePiece(x, z));
                                pieceInstance = DrawWallsAndPillars(pos, neighbours, pillarsLocations);
                                break;
                        }

                        pieces[x, z] = new (x, z, pieceType, pieceInstance);
                    }
                }
            }
        }

        // private void DrawCustomPieceOrig(Vector3 pos, byte[] neighbours) {
        //     Instantiate(flooredCeiling, pos, Quaternion.identity);
        //     for (int i = 0; i < neighbours.Length; i++) {
        //         if (neighbours[i] == MazeCellInfo.Wall) {
        //             Instantiate(wallPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 90 * i, 0));
        //         }
        //     }
        // }

        private GameObject DrawWallsAndPillars(Vector3 pos, byte[] neighbours, IDictionary<Vector3, bool> pillars) {
            
            //TOP = 1 / RIGHT = 3 / DOWN = 5 / LEFT = 7
            //The left pillar is y-rotated -90, and each pillar should have the same rotation of its wall
            //Using circular Index to reference neighbours
            //To keep track of the pillars we assume they will be added in the respective "diagonal" coordinate of the cell
            //TODO DEBUG PILLAR AT COORDINATE 102.72
            Vector3 posInt = new Vector3((int)pos.x, 0, (int)pos.z);
            int rotation = 0;
            foreach (int side in new[] { 1, 3, 5, 7 }) {
                if (neighbours[side] == MazeCellInfo.Wall) {
                    var wallRotation = Quaternion.Euler(0, rotation * 90, 0);
                    GameObject wall = Instantiate(wallPieces.GetRandomPiece(), pos, wallRotation);
                    wall.name = "Wall";

                    Vector3 rightPillarPos = (neighbourRelativeCoords.CircularIndexValue(side + 1) * pieceScale / 2) + posInt;
                    if (neighbours.CircularIndexValue(side + 1) == MazeCellInfo.Corridor && neighbours.CircularIndexValue(side + 2) == MazeCellInfo.Corridor &&
                        !pillars.ContainsKey(rightPillarPos)) {
                        GameObject pillar = Instantiate(pillarPieces.GetRandomPiece(), pos, Quaternion.Euler(wallRotation.eulerAngles));
                        pillar.name = wall.name + $"_RightPilar({rightPillarPos.x},{rightPillarPos.y})";
                        pillar.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                        pillars.Add(rightPillarPos, true);
                    }

                    Vector3 leftPillarPos = (neighbourRelativeCoords.CircularIndexValue(side - 1) * pieceScale / 2) + posInt;
                    if (neighbours.CircularIndexValue(side - 2) == MazeCellInfo.Corridor && neighbours.CircularIndexValue(side - 1) == MazeCellInfo.Corridor &&
                        !pillars.ContainsKey(leftPillarPos)) {
                        GameObject pillar = Instantiate(pillarPieces.GetRandomPiece(), pos, Quaternion.Euler(wallRotation.eulerAngles + new Vector3(0, -90, 0)));
                        pillar.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                        pillar.name = wall.name + $"_LeftPilar({leftPillarPos.x},{leftPillarPos.y})";
                        pillars.Add(leftPillarPos, true);
                    }
                }

                rotation++;
            }
            
            return Instantiate(flooredCeiling, pos, Quaternion.identity);
        }

        private void PlacePlayer() {
            for (int x = 1; x < mazeConfig.width - 1; x++) {
                for (int z = 1; z < mazeConfig.height - 1; z++) {
                    if (map[x, z] == MazeCellInfo.Corridor) {
                        Instantiate(playerPrefab, new Vector3(x * 6, 0, z * 6), Quaternion.identity);
                        return;
                    }
                }
            }
        }

        protected int CountCrossNeighboursOfType(int x, int z, byte type = MazeCellInfo.Corridor) {
            if (IsOutsideMaze(x, z)) return 5;

            int count = 0;
            count += map[x - 1, z] == type ? 1 : 0;
            count += map[x + 1, z] == type ? 1 : 0;
            count += map[x, z + 1] == type ? 1 : 0;
            count += map[x, z - 1] == type ? 1 : 0;
            return count;
        }

        protected int CountDiagonalNeighboursOfType(int x, int z, byte type = MazeCellInfo.Corridor) {
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
                   x < mazeConfig.width - 1 &&
                   z > 0 &&
                   z < mazeConfig.height - 1;
        }

        protected bool IsOutsideMaze(int x, int z) {
            bool result = x <= 0 ||
                          x >= mazeConfig.width - 1 ||
                          z <= 0 ||
                          z >= mazeConfig.height - 1;
            return result;
        }
    }
}