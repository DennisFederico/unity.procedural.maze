using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace narkdagas.mazegenerator {
    public class MazeGenerator : MonoBehaviour {
        // [SerializeField] protected int genSeed = 432912345;
        [SerializeField] protected byte width;
        [SerializeField] protected byte depth;
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
        public byte[,] map;
        public PieceData[,] pieces;

        public readonly List<CellLocation> directions = new() {
            new CellLocation(1, 0),
            new CellLocation(0, 1),
            new CellLocation(-1, 0),
            new CellLocation(0, -1)
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
            public float xOffset;
            public float zOffset;

            public MazeConfig(byte width, byte height, byte level = 0, float pieceScale = 6, float heightScale = 2, byte numRooms = 0, bool placePlayer = false) {
                this.width = width;
                this.height = height;
                this.level = level;
                this.pieceScale = pieceScale;
                this.heightScale = heightScale;
                this.numRooms = numRooms;
                this.placePlayer = placePlayer;
                xOffset = 0f;
                zOffset = 0f;
            }
        }

        public enum CellLocationType {
            Corridor = 0, Wall = 1, Maze = 2, Any=99
        }

        public readonly struct CellLocation {
            public readonly int x;
            public readonly int z;

            public CellLocation(int x, int z) {
                this.x = x;
                this.z = z;
            }

            public readonly Vector2 ToVector() {
                return new Vector2(x, z);
            }

            public static CellLocation operator +(CellLocation a, CellLocation b) {
                return new CellLocation(a.x + b.x, a.z + b.z);
            }

            public readonly bool Equals(CellLocation other) {
                return x == other.x && z == other.z;
            }

            public override bool Equals(object obj) {
                return obj is CellLocation other && Equals(other);
            }

            public override int GetHashCode() {
                return HashCode.Combine(x, z);
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
        public void Build(byte level) {
            transform.position = Vector3.zero;
            mazeConfig = new MazeConfig(width, depth, level, pieceScale, heightScale, numRooms, placePlayer);
            InitializeMap();
            GenerateMap();
            AddRooms(numRooms, roomSizeRange.x, roomSizeRange.y);

            var corridorWithAStarPath = GetComponent<CorridorWithAStarPath>();
            if (corridorWithAStarPath) {
                corridorWithAStarPath.Build();
            }
            
            DrawMap();
            if (placePlayer) PlacePlayer();
        }

        public void InitializeMap() {
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
                    map[x, z] = (byte)(Random.Range(0, 100) < 50 ? CellLocationType.Corridor : CellLocationType.Wall);
                }
            }
        }

        protected virtual void AddRooms(int numberOfRooms, int minSize, int maxSize) {
            Debug.Log($"Adding {numberOfRooms} Rooms for map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            for (int room = 1; room <= numberOfRooms; room++) {
                var w = Random.Range(minSize, maxSize + 1);
                var h = Random.Range(minSize, maxSize + 1);
                var startX = Random.Range(3, mazeConfig.width - w - 3);
                var startZ = Random.Range(3, mazeConfig.height - h - 3);

                Debug.Log($"Adding room {room} - Size:[{w}][{h}] - Start:[{startX}][{startZ}]");
                
                for (var x = startX; x <= startX + w; x++) {
                    for (var z = startZ; z <= startZ + h; z++) {
                        map[x, z] = (byte) CellLocationType.Corridor;
                    }
                }
            }
        }

        public void DrawMap() {
            Debug.Log($"Drawing map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            float height = mazeConfig.level * pieceScale * heightScale;
            Dictionary<Vector3, bool> pillarsLocations = new Dictionary<Vector3, bool>();
            for (byte z = 0; z < mazeConfig.height; z++) {
                for (byte x = 0; x < mazeConfig.width; x++) {
                    Vector3 pos = new Vector3(x * pieceScale, height, z * pieceScale);
                    if (map[x, z] == (int)CellLocationType.Corridor) {
                        var neighbours = map.GetAllNeighboursForMazePiece(x, z);
                        PieceType pieceType = neighbours.MatchMazePiece();
                        GameObject pieceInstance;
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
                            default: //CUSTOM
                                //DrawCustomPieceOrig(pos, map.GetCrossNeighboursForMazePiece(x, z));
                                pieceInstance = DrawWallsAndPillars(pos, neighbours, pillarsLocations);
                                break;
                        }
                        pieceInstance.transform.SetParent(transform);
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
                if (neighbours[side] == (int) CellLocationType.Wall) {
                    var wallRotation = Quaternion.Euler(0, rotation * 90, 0);
                    GameObject wall = Instantiate(wallPieces.GetRandomPiece(), pos, wallRotation);
                    wall.name = "Wall";
                    wall.transform.SetParent(transform);

                    Vector3 rightPillarPos = (neighbourRelativeCoords.CircularIndexValue(side + 1) * pieceScale / 2) + posInt;
                    if (neighbours.CircularIndexValue(side + 1) == (int)CellLocationType.Corridor && neighbours.CircularIndexValue(side + 2) == (int)CellLocationType.Corridor &&
                        !pillars.ContainsKey(rightPillarPos)) {
                        GameObject pillar = Instantiate(pillarPieces.GetRandomPiece(), pos, Quaternion.Euler(wallRotation.eulerAngles));
                        pillar.name = wall.name + $"_RightPilar({rightPillarPos.x},{rightPillarPos.y})";
                        pillar.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                        pillar.transform.SetParent(transform);
                        pillars.Add(rightPillarPos, true);
                    }

                    Vector3 leftPillarPos = (neighbourRelativeCoords.CircularIndexValue(side - 1) * pieceScale / 2) + posInt;
                    if (neighbours.CircularIndexValue(side - 2) == (int)CellLocationType.Corridor && neighbours.CircularIndexValue(side - 1) == (int)CellLocationType.Corridor &&
                        !pillars.ContainsKey(leftPillarPos)) {
                        GameObject pillar = Instantiate(pillarPieces.GetRandomPiece(), pos, Quaternion.Euler(wallRotation.eulerAngles + new Vector3(0, -90, 0)));
                        pillar.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                        pillar.name = wall.name + $"_LeftPilar({leftPillarPos.x},{leftPillarPos.y})";
                        pillar.transform.SetParent(transform);
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
                    if (map[x, z] == (int)CellLocationType.Corridor) {
                        Instantiate(playerPrefab, new Vector3(x * 6, 0, z * 6), Quaternion.identity);
                        return;
                    }
                }
            }
        }

        protected int CountCrossNeighboursOfType(int x, int z, byte type = (byte) CellLocationType.Corridor) {
            if (IsOutsideMaze(x, z)) return 5;

            int count = 0;
            count += map[x - 1, z] == type ? 1 : 0;
            count += map[x + 1, z] == type ? 1 : 0;
            count += map[x, z + 1] == type ? 1 : 0;
            count += map[x, z - 1] == type ? 1 : 0;
            return count;
        }

        protected int CountDiagonalNeighboursOfType(int x, int z, byte type = (byte) CellLocationType.Corridor) {
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

        protected bool IsInsideMaze(CellLocation cellLocation) {
            return IsInsideMaze(cellLocation.x, cellLocation.z);
        }

        protected bool IsOutsideMaze(CellLocation cellLocation) {
            return IsOutsideMaze(cellLocation.x, cellLocation.z);
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