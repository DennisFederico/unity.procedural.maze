using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace narkdagas.mazegenerator {
    public class DungeonMazeManager : Singleton<DungeonMazeManager> {
        public Maze[] mazes;
        public GameObject grimoirePrefab;
        public GameObject deadEndStair;
        public GameObject playerPrefab;
        public MinimapCamera minimapCamera;
        public RawImage minimapCameraDisplay;
        private RenderTexture _minimapCameraTexture;
        private GameObject _playerInstance;

        public void Init() {
            Debug.Log("Start MazeManager");
            GenerateMazes();
            ConnectLevels();
            PlaceTeleporters();
            GenerateMinimaps();
            PlaceGrimoire();

            var playerPlacement = PlacePlayer(mazes[0]);
            _playerInstance = playerPlacement.player;
            mazes[0].startLocations.Add(playerPlacement.location);

            StartMinimapRender(playerPlacement.player.transform);
        }

        void GenerateMazes() {
            byte level = 0;
            //TODO REBUILD (NEXT?) MAZE UNTIL THERE ARE ENOUGH CONNECTION CANDIDATES - TIMEOUT?? MAX TRIES??
            foreach (Maze maze in mazes) {
                Debug.Log($"Building maze for level: {level}");
                maze.Build(level++);
            }
        }

        void ConnectLevels() {
            Debug.Log("Connecting Levels...");
            for (int level = 0; level < mazes.Length - 1; level++) {
                Debug.Log($"Connecting maze {level} to {level + 1}");
                var connectionsFound = GetConnectionPairs(mazes[level].pieces, mazes[level + 1].pieces);
                if (connectionsFound.Count > 0) {
                    int numStairs = Math.Min(Random.Range(mazes[level].numLaddersRange.x, mazes[level].numLaddersRange.y + 1), connectionsFound.Count);
                    Debug.Log($"Building {numStairs} random stairs out of {connectionsFound.Count} candidates between levels {level} -> {level + 1}");
                    connectionsFound.ShuffleCurrent();
                    for (var i = 0; i < numStairs; i++) {
                        var stairLocation = connectionsFound[i];
                        BuildStair(stairLocation.src, stairLocation.dst, (mazes[level].mazeConfig, mazes[level + 1].mazeConfig));
                        //ADD EXIT TO THE SOURCE MAZE
                        mazes[level].exitLocations.Add(stairLocation.src.pieceLocation);
                        mazes[level + 1].startLocations.Add(stairLocation.dst.pieceLocation);
                    }
                } else {
                    //FIND MATCH PIECES
                    Debug.Log("Building stair by offset translation");
                    var connectionCandidates = GetConnectionCandidates(mazes[level].pieces, mazes[level + 1].pieces);

                    if (connectionCandidates.src.Count == 0 || connectionCandidates.dst.Count == 0) {
                        Debug.Log("No connection candidate pairs found!");
                        continue;
                    }

                    //SELECT A RANDOM PAIR
                    PieceData srcConnection = connectionCandidates.src[Random.Range(0, connectionCandidates.src.Count)];
                    PieceData dstConnection = connectionCandidates.dst[Random.Range(0, connectionCandidates.dst.Count)];

                    //BUILD STAIR
                    BuildStair(srcConnection, dstConnection, (mazes[level].mazeConfig, mazes[level + 1].mazeConfig));
                    mazes[level].exitLocations.Add(srcConnection.pieceLocation);
                    mazes[level + 1].startLocations.Add(dstConnection.pieceLocation);

                    //GET OFFSET TO MOVE UPPER LEVEL
                    mazes[level + 1].mazeConfig.xOffset = srcConnection.pieceLocation.x - dstConnection.pieceLocation.x;
                    mazes[level + 1].mazeConfig.zOffset = srcConnection.pieceLocation.z - dstConnection.pieceLocation.z;
                    Debug.Log($"Offset level {level + 1} by [{mazes[level + 1].mazeConfig.xOffset}, {mazes[level + 1].mazeConfig.zOffset}]");
                }
            }

            float carryXOffset = 0;
            float carryZOffset = 0;
            //MOVE THE MAZES BY THE OFFSET
            for (int level = 0; level < mazes.Length - 1; level++) {
                carryXOffset += mazes[level].mazeConfig.xOffset;
                carryZOffset += mazes[level].mazeConfig.zOffset;

                mazes[level + 1].gameObject.transform.Translate(
                    (mazes[level + 1].mazeConfig.xOffset + carryXOffset) * mazes[level + 1].mazeConfig.pieceScale,
                    0,
                    (mazes[level + 1].mazeConfig.zOffset + carryZOffset) * mazes[level + 1].mazeConfig.pieceScale
                );
            }

            Debug.Log("Connecting Levels... Done.");
        }

        IList<(PieceData src, PieceData dst)> GetConnectionPairs(PieceData[,] srcMaze, PieceData[,] dstMaze) {
            //THESE CAN ONLY EXIST WITHIN THE SMALLEST "COMMON" SECTION OF THE TWO MAZES
            int innerWidth = Math.Min(srcMaze.GetLength(0), dstMaze.GetLength(0));
            int innerDepth = Math.Min(srcMaze.GetLength(1), dstMaze.GetLength(1));
            IList<(PieceData src, PieceData dst)> connections = new List<(PieceData src, PieceData dst)>();
            for (byte z = 0; z < innerDepth; z++) {
                for (byte x = 0; x < innerWidth; x++) {
                    if (srcMaze[x, z] == null || dstMaze[x, z] == null) continue;
                    if ((srcMaze[x, z].pieceType == PieceType.DeadEndLeft && dstMaze[x, z].pieceType == PieceType.DeadEndRight) ||
                        (srcMaze[x, z].pieceType == PieceType.DeadEndRight && dstMaze[x, z].pieceType == PieceType.DeadEndLeft) ||
                        (srcMaze[x, z].pieceType == PieceType.DeadEndTop && dstMaze[x, z].pieceType == PieceType.DeadEndBottom) ||
                        (srcMaze[x, z].pieceType == PieceType.DeadEndBottom && dstMaze[x, z].pieceType == PieceType.DeadEndTop)) {
                        connections.Add((srcMaze[x, z], dstMaze[x, z]));
                    }
                }
            }

            Debug.Log($"Found {connections.Count} connection candidates");
            return connections;
        }

        (IList<PieceData> src, IList<PieceData> dst) GetConnectionCandidates(PieceData[,] srcMaze, PieceData[,] dstMaze) {
            var deadEndTypes = new[] { PieceType.DeadEndLeft, PieceType.DeadEndRight, PieceType.DeadEndTop, PieceType.DeadEndBottom };
            var srcDeadEnds = srcMaze.OfTypes(deadEndTypes);
            var dstDeadEnds = dstMaze.OfTypes(deadEndTypes);

            //LEFT-RIGHT PAIRS
            var srcResults = srcDeadEnds.ToArray().OfType(PieceType.DeadEndLeft);
            var dstResults = dstDeadEnds.ToArray().OfType(PieceType.DeadEndRight);
            if (srcResults.Count > 0 && dstResults.Count > 0) {
                return (srcResults, dstResults);
            }

            //RIGHT-LEFT PAIRS
            srcResults = srcDeadEnds.ToArray().OfType(PieceType.DeadEndRight);
            dstResults = dstDeadEnds.ToArray().OfType(PieceType.DeadEndLeft);
            if (srcResults.Count > 0 && dstResults.Count > 0) {
                return (srcResults, dstResults);
            }

            //TOP-BOTTOM PAIRS
            srcResults = srcDeadEnds.ToArray().OfType(PieceType.DeadEndTop);
            dstResults = dstDeadEnds.ToArray().OfType(PieceType.DeadEndBottom);
            if (srcResults.Count > 0 && dstResults.Count > 0) {
                return (srcResults, dstResults);
            }

            //BOTTOM-TOP PAIRS
            srcResults = srcDeadEnds.ToArray().OfType(PieceType.DeadEndBottom);
            dstResults = dstDeadEnds.ToArray().OfType(PieceType.DeadEndTop);
            if (srcResults.Count > 0 && dstResults.Count > 0) {
                return (srcResults, dstResults);
            }

            Debug.Log($"NO CONNECTION CANDIDATES FOUND!!!");
            return (new List<PieceData>(), new List<PieceData>());
        }

        void BuildStair(PieceData srcPiece, PieceData dstPiece, (Maze.MazeConfig src, Maze.MazeConfig dst) mazeConfigs) {
            Debug.Log($"Building stair between levels {mazeConfigs.src.level} -> {mazeConfigs.dst.level}");
            var transformParent = srcPiece.pieceModel.transform.parent;
            Destroy(srcPiece.pieceModel);
            Destroy(dstPiece.pieceModel);

            Vector3 srcPos = new Vector3(srcPiece.pieceLocation.x * mazeConfigs.src.pieceScale, mazeConfigs.src.level * mazeConfigs.src.pieceScale * mazeConfigs.src.heightScale,
                srcPiece.pieceLocation.z * mazeConfigs.src.pieceScale);
            Vector3 dstPos = new Vector3(dstPiece.pieceLocation.x * mazeConfigs.dst.pieceScale, mazeConfigs.dst.level * mazeConfigs.dst.pieceScale * mazeConfigs.src.heightScale,
                dstPiece.pieceLocation.z * mazeConfigs.dst.pieceScale);

            GameObject newSrcPieceModel = null;
            switch (srcPiece.pieceType) {
                case PieceType.DeadEndRight:
                    newSrcPieceModel = Instantiate(deadEndStair, srcPos, Quaternion.identity);
                    newSrcPieceModel.transform.SetParent(transformParent);
                    newSrcPieceModel.name = $"Stairs_DeadEndRight_{mazeConfigs.src.level}_to_{mazeConfigs.dst.level}";
                    break;
                case PieceType.DeadEndBottom:
                    newSrcPieceModel = Instantiate(deadEndStair, srcPos, Quaternion.Euler(0, 90, 0));
                    newSrcPieceModel.transform.SetParent(transformParent);
                    newSrcPieceModel.name = $"Stairs_DeadEndBottom_{mazeConfigs.src.level}_to_{mazeConfigs.dst.level}";
                    break;
                case PieceType.DeadEndLeft:
                    newSrcPieceModel = Instantiate(deadEndStair, srcPos, Quaternion.Euler(0, 180, 0));
                    newSrcPieceModel.transform.SetParent(transformParent);
                    newSrcPieceModel.name = $"Stairs_DeadEndLeft_{mazeConfigs.src.level}_to_{mazeConfigs.dst.level}";
                    break;
                case PieceType.DeadEndTop:
                    newSrcPieceModel = Instantiate(deadEndStair, srcPos, Quaternion.Euler(0, 270, 0));
                    newSrcPieceModel.transform.SetParent(transformParent);
                    newSrcPieceModel.name = $"Stairs_DeadEndTop_{mazeConfigs.src.level}_to_{mazeConfigs.dst.level}";
                    break;
            }

            srcPiece.pieceType = PieceType.LadderUp;
            srcPiece.pieceModel = newSrcPieceModel;
            if (srcPiece.pieceModel) {
                srcPiece.pieceModel.AddComponent<PieceLocation>().location = srcPiece.pieceLocation;
            }

            dstPiece.pieceType = PieceType.LadderDown;
            dstPiece.pieceModel = null;
            Debug.Log($"Built connection levels {mazeConfigs.src.level} -> {mazeConfigs.dst.level} at {srcPos} -> {dstPos}");
        }

        void PlaceTeleporters() {
            var teleporters = GetComponents<Teleporter>();
            foreach (var teleporter in teleporters) {
                teleporter.Place(mazes[teleporter.startMaze], mazes[teleporter.endMaze]);
            }
        }

        private void PlaceGrimoire() {
            //Will be placed at the last maze on a DeadEnd
            var deadEnds = mazes[^1].pieces.OfTypes(new[] { PieceType.DeadEndTop, PieceType.DeadEndRight, PieceType.DeadEndLeft, PieceType.DeadEndBottom, });
            var deadEnd = deadEnds.GetRandom();
            var grimoire = Instantiate(grimoirePrefab, deadEnd.pieceModel.transform.position + Vector3.up * 2, deadEnd.pieceModel.transform.rotation);
            grimoire.transform.Rotate(Vector3.right, -45);
            grimoire.transform.SetParent(deadEnd.pieceModel.transform);
            mazes[^1].exitLocations.Add(deadEnd.pieceLocation);
        }

        void GenerateMinimaps() {
            foreach (var maze in mazes) {
                maze.GenerateMiniMap();
            }
        }

        (Maze.MapLocation location, GameObject player) PlacePlayer(Maze maze) {
            for (int x = 1; x < maze.mazeConfig.width - 1; x++) {
                for (int z = 1; z < maze.mazeConfig.height - 1; z++) {
                    if (maze.map[x, z] == (int)Maze.MapLocationType.Corridor) {
                        var mapLocation = new Maze.MapLocation(x, z);
                        var playerInstance = Instantiate(playerPrefab, new Vector3(x * maze.mazeConfig.pieceScale, 0, z * maze.mazeConfig.pieceScale), Quaternion.identity);
                        return (mapLocation, playerInstance);
                    }
                }
            }

            return (new Maze.MapLocation(0, 0), null);
        }

        private void StartMinimapRender(Transform player) {
            _minimapCameraTexture = new RenderTexture(150, 150, 8);
            minimapCamera.Initialize(player.transform, _minimapCameraTexture);
            minimapCameraDisplay.texture = _minimapCameraTexture;
        }
    }
}