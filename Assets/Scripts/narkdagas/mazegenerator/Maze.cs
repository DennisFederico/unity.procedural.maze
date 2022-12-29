using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace narkdagas.mazegenerator {
    public class Maze : MonoBehaviour {
        // [SerializeField] protected int genSeed = 432912345;
        [SerializeField] protected byte width;
        [SerializeField] protected byte depth;
        [SerializeField] private float pieceScale = 6;
        [SerializeField] private float heightScale = 2f;
        [SerializeField] protected GameObject flooredCeiling;
        [SerializeField] protected GameObject[] wallPieces;
        [SerializeField] protected GameObject[] doorwayPieces;
        [SerializeField] protected GameObject[] pillarPieces;
        [SerializeField] protected GameObject[] straightPieces;
        public bool iterateStraightPieces;
        [SerializeField] protected GameObject[] cornerPieces;
        [SerializeField] protected GameObject[] deadEndPieces;
        [SerializeField] protected GameObject[] junctionPieces;
        [SerializeField] protected GameObject[] intersectionPieces;
        [SerializeField] private byte numRooms = 3;
        [SerializeField] public Vector2Int roomSizeRange = new(3, 6);
        [SerializeField] public Vector2Int numLaddersRange = new(1, 3);
        public MazeConfig mazeConfig;
        public byte[,] map;
        public PieceData[,] pieces;
        public List<MapLocation> startLocations = new();
        public List<MapLocation> exitLocations = new();

        [Serializable]
        public struct MazeConfig {
            public int width;
            public int height;
            public byte level;
            public float pieceScale;
            public float heightScale;
            public byte numRooms;
            public float xOffset;
            public float zOffset;

            public MazeConfig(int width, int height, byte level = 0, float pieceScale = 6, float heightScale = 2, byte numRooms = 0, bool placePlayer = false) {
                this.width = width;
                this.height = height;
                this.level = level;
                this.pieceScale = pieceScale;
                this.heightScale = heightScale;
                this.numRooms = numRooms;
                xOffset = 0f;
                zOffset = 0f;
            }
        }

        public enum MapLocationType {
            Corridor = 0,
            Wall = 1,
            Maze = 2,
            Any = 99
        }

        public readonly struct MapLocation {
            public readonly int x;
            public readonly int z;

            public MapLocation(int x, int z) {
                this.x = x;
                this.z = z;
            }

            public readonly Vector2 ToVector() {
                return new Vector2(x, z);
            }

            public static MapLocation operator +(MapLocation a, MapLocation b) {
                return new MapLocation(a.x + b.x, a.z + b.z);
            }

            public readonly bool Equals(MapLocation other) {
                return x == other.x && z == other.z;
            }

            public override bool Equals(object obj) {
                return obj is MapLocation other && Equals(other);
            }

            public override int GetHashCode() {
                return HashCode.Combine(x, z);
            }
        }
        
        public readonly List<MapLocation> directions = new() {
            new MapLocation(1, 0),
            new MapLocation(0, 1),
            new MapLocation(-1, 0),
            new MapLocation(0, -1)
        };

        //The relative position of neighbours
        // MATCHING CLOCKWISE FROM TOP-LEFT
        //          |---|---|---|
        //          | 0 | 1 | 2 |
        //          |---|---|---|
        //          | 7 | X | 3 |
        //          |---|---|---|
        //          | 6 | 5 | 4 |
        //          |---|---|---|
        private readonly Vector3[] _neighbourRelativeCoords = {
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
            mazeConfig = new MazeConfig(width, depth, level, pieceScale, heightScale, numRooms);
            InitializeMap();
            GenerateMap();
            AddRooms(numRooms, roomSizeRange.x, roomSizeRange.y);
            GrowMap(3, 3);

            var corridorWithAStarPath = GetComponent<CorridorWithAStarPath>();
            if (corridorWithAStarPath) {
                (PathMarker startNode, PathMarker goalNode) nodes = corridorWithAStarPath.Build();
                //DRAW A CORRIDOR TO THE LEFT AND RIGHT - FIND WHICH END IS CLOSER TO WHICH EDGE
                var leftNode = nodes.startNode.mapLocation.x < nodes.goalNode.mapLocation.x ? nodes.startNode : nodes.goalNode;
                var rightNode = nodes.startNode.mapLocation.x < nodes.goalNode.mapLocation.x ? nodes.goalNode : nodes.startNode;
                var leftX = leftNode.mapLocation.x;
                var leftZ = leftNode.mapLocation.z;
                var rightX = rightNode.mapLocation.x;
                var rightZ = rightNode.mapLocation.z;
                while (leftX > 1) {
                    map[leftX--, leftZ] = 0;
                }
                while (rightX < mazeConfig.width - 2) {
                    map[rightX++, rightZ] = 0;
                }
            } else {
                //START AT RANDOM FROM EACH EDGE AND DIG UNTIL YOU FIND A CORRIDOR
                int digMax = Math.Max(mazeConfig.width, mazeConfig.height);
                int startLeftX = 1;
                int startLeftZ = Random.Range(mazeConfig.height / 4, mazeConfig.height - mazeConfig.height / 4);
                int startRightX = mazeConfig.width - 2;
                int startRightZ = Random.Range(mazeConfig.height / 4, mazeConfig.height - mazeConfig.height / 4);
                int startBottomX = Random.Range(mazeConfig.width / 4, mazeConfig.width - mazeConfig.width / 4);
                int startBottomZ = 1;
                int startTopX = Random.Range(mazeConfig.width / 4, mazeConfig.width - mazeConfig.width / 4);
                int startTopZ = mazeConfig.height - 2;
                bool doneLeft = false, doneRight = false, doneBottom = false, doneTop = false;
                int step = 1;
                
                while ((!doneLeft || !doneRight || !doneBottom || !doneTop) && step < digMax) {
                    if (!doneLeft) {
                        map[startLeftX++, startLeftZ] = 0;
                        doneLeft = map[startLeftX, startLeftZ] == 0;
                    }
                    if (!doneRight) {
                        map[startRightX--, startRightZ] = 0;
                        doneRight = map[startRightX, startRightZ] == 0;
                    }
                
                    if (!doneBottom) {
                        map[startBottomX, startBottomZ++] = 0;
                        doneBottom = map[startBottomX, startBottomZ] == 0;
                    }
                
                    if (!doneTop) {
                        map[startTopX, startTopZ--] = 0;
                        doneTop = map[startTopX, startTopZ] == 0;
                    }

                    step++;
                }
                Debug.Log($"Corridors carved in {step}/{digMax} steps");
            }

            DrawMap();

            var placeItems = GetComponents<PlaceItems>();
            foreach (var placeItem in placeItems) {
                placeItem.PlaceItemsForMaze();
            }
        }

        private void GrowMap(int extraWidth, int extraHeight) {
            map = map.CreateOffsetCopy(extraWidth, extraHeight);
            mazeConfig.width += (extraWidth * 2);
            mazeConfig.height += (extraHeight * 2);
        }

        public void InitializeMap() {
            Debug.Log($"Initializing map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            map = new byte[mazeConfig.width, mazeConfig.height];
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
                    map[x, z] = (byte)(Random.Range(0, 100) < 50 ? MapLocationType.Corridor : MapLocationType.Wall);
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
                        map[x, z] = (byte)MapLocationType.Corridor;
                    }
                }
            }
        }

        private void DrawMap() {
            Debug.Log($"Drawing map level {mazeConfig.level} - [{mazeConfig.width}][{mazeConfig.height}]");
            pieces = new PieceData[mazeConfig.width, mazeConfig.height];
            int straightPieceCount = 0;
            float height = mazeConfig.level * pieceScale * heightScale;
            Dictionary<Vector3, bool> pillarsLocations = new Dictionary<Vector3, bool>();
            for (byte z = 0; z < mazeConfig.height; z++) {
                for (byte x = 0; x < mazeConfig.width; x++) {
                    Vector3 pos = new Vector3(x * pieceScale, height, z * pieceScale);
                    if (map[x, z] == (int)MapLocationType.Corridor) {
                        var neighbours = map.GetAllNeighboursForMazePiece(x, z);
                        PieceType pieceType = neighbours.MatchMazePiece();
                        GameObject pieceInstance;
                        switch (pieceType) {
                            case PieceType.CorridorHorizontal:
                                pieceInstance = iterateStraightPieces ? Instantiate(straightPieces[straightPieceCount++ % straightPieces.Length], pos, Quaternion.Euler(0, 90, 0)) : Instantiate(straightPieces.GetRandomPiece(), pos, Quaternion.Euler(0, 90, 0));
                                break;
                            case PieceType.CorridorVertical:
                                pieceInstance = iterateStraightPieces ? Instantiate(straightPieces[straightPieceCount++ % straightPieces.Length], pos, Quaternion.identity) : Instantiate(straightPieces.GetRandomPiece(), pos, Quaternion.identity);
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
                            default:
                                pieceInstance = null;
                                break;
                        }

                        if (pieceInstance) {
                            pieceInstance.transform.SetParent(transform);
                            var pieceLocation = pieceInstance.AddComponent<PieceLocation>();
                            pieceLocation.location = new MapLocation(x, z);
                        }

                        pieces[x, z] = new(new MapLocation(x, z), pieceType, pieceInstance);
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
                if (neighbours[side] == (int)MapLocationType.Wall) {
                    var wallRotation = Quaternion.Euler(0, rotation * 90, 0);
                    GameObject wall = Instantiate(wallPieces.GetRandomPiece(), pos, wallRotation);
                    wall.name = "Wall";
                    wall.transform.SetParent(transform);

                    Vector3 rightPillarPos = (_neighbourRelativeCoords.CircularIndexValue(side + 1) * pieceScale / 2) + posInt;
                    if (neighbours.CircularIndexValue(side + 1) == (int)MapLocationType.Corridor && neighbours.CircularIndexValue(side + 2) == (int)MapLocationType.Corridor &&
                        !pillars.ContainsKey(rightPillarPos)) {
                        GameObject pillar = Instantiate(pillarPieces.GetRandomPiece(), pos, Quaternion.Euler(wallRotation.eulerAngles));
                        pillar.name = wall.name + $"_RightPilar({rightPillarPos.x},{rightPillarPos.y})";
                        pillar.transform.localScale = new Vector3(1.01f, 1, 1.01f);
                        pillar.transform.SetParent(transform);
                        pillars.Add(rightPillarPos, true);
                    }

                    Vector3 leftPillarPos = (_neighbourRelativeCoords.CircularIndexValue(side - 1) * pieceScale / 2) + posInt;
                    if (neighbours.CircularIndexValue(side - 2) == (int)MapLocationType.Corridor && neighbours.CircularIndexValue(side - 1) == (int)MapLocationType.Corridor &&
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

        protected int CountCrossNeighboursOfType(int x, int z, byte type = (byte)MapLocationType.Corridor) {
            if (IsOutsideMaze(x, z)) return 5;

            int count = 0;
            count += map[x - 1, z] == type ? 1 : 0;
            count += map[x + 1, z] == type ? 1 : 0;
            count += map[x, z + 1] == type ? 1 : 0;
            count += map[x, z - 1] == type ? 1 : 0;
            return count;
        }

        private int CountDiagonalNeighboursOfType(int x, int z, byte type = (byte)MapLocationType.Corridor) {
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

        protected bool IsInsideMaze(MapLocation mapLocation) {
            return IsInsideMaze(mapLocation.x, mapLocation.z);
        }

        protected bool IsOutsideMaze(MapLocation mapLocation) {
            return IsOutsideMaze(mapLocation.x, mapLocation.z);
        }

        protected bool IsInsideMaze(int x, int z) {
            return x > 0 &&
                   x < mazeConfig.width - 1 &&
                   z > 0 &&
                   z < mazeConfig.height - 1;
        }

        private bool IsOutsideMaze(int x, int z) {
            bool result = x <= 0 ||
                          x >= mazeConfig.width - 1 ||
                          z <= 0 ||
                          z >= mazeConfig.height - 1;
            return result;
        }
    }
}