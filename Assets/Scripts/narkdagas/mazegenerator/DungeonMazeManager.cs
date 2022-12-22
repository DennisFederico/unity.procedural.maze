using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace narkdagas.mazegenerator {
    public class DungeonMazeManager : MonoBehaviour {
        public MazeGenerator[] mazes;
        public byte width;
        public byte depth;

        public GameObject deadEndStair;

        private void Start() {
            Debug.Log("Start MazeManager");
            GenerateMazes();
            ConnectLevels();
        }

        void GenerateMazes() {
            byte level = 0;
            //TODO REBUILD (NEXT?) MAZE UNTIL THERE ARE ENOUGH CONNECTION CANDIDATES - TIMEOUT?? MAX TRIES??
            foreach (MazeGenerator maze in mazes) {
                Debug.Log($"Building maze for level: {level}");
                maze.Build(width, depth, level++);
            }
        }

        void ConnectLevels() {
            Debug.Log("Connecting Levels...");
            for (int level = 0; level < mazes.Length - 1; level++) {
                Debug.Log($"Connecting maze {level} to {level + 1}");
                var connectionsFound = GetConnectionCandidates(mazes[level].pieces, mazes[level + 1].pieces);
                BuildConnections(connectionsFound, (mazes[level].mazeConfig, mazes[level + 1].mazeConfig), mazes[level].numLaddersRange.x, mazes[level].numLaddersRange.y);
            }
        }

        //TODO CHECK THE MAZE TYPE FOR VALID CONNECTIONS MATCHES
        IList<(PieceData src, PieceData dst)> GetConnectionCandidates(PieceData[,] srcMaze, PieceData[,] dstMaze) {
            IList<(PieceData src, PieceData dst)> connections = new List<(PieceData src, PieceData dst)>();
            for (byte z = 0; z < depth; z++) {
                for (byte x = 0; x < width; x++) {
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

        void BuildConnections(IList<(PieceData src, PieceData dst)> connections, (MazeGenerator.MazeConfig src, MazeGenerator.MazeConfig dst) mazeConfigs, int min, int max) {
            int numConnections = Math.Min(Random.Range(min, max + 1), connections.Count);
            Debug.Log($"Building {numConnections} random connections out of {connections.Count} candidates between levels {mazeConfigs.src.level} -> {mazeConfigs.dst.level}");
            connections.ShuffleCurrent();
            for (var i = 0; i < numConnections; i++) {
                PieceData srcPiece = connections[i].src;
                PieceData dstPiece = connections[i].dst;
                Destroy(srcPiece.pieceModel);
                Destroy(dstPiece.pieceModel);

                Vector3 srcPos = new Vector3(srcPiece.posX * mazeConfigs.src.pieceScale, mazeConfigs.src.level * mazeConfigs.src.pieceScale * mazeConfigs.src.heightScale, srcPiece.posZ * mazeConfigs.src.pieceScale);
                srcPiece.pieceType = PieceType.LadderUp;
                Vector3 dstPos = new Vector3(dstPiece.posX * mazeConfigs.dst.pieceScale, mazeConfigs.dst.level * mazeConfigs.dst.pieceScale * mazeConfigs.src.heightScale, dstPiece.posZ * mazeConfigs.dst.pieceScale);
                dstPiece.pieceType = PieceType.LadderDown;

                GameObject newSrcPieceModel = null;
                switch (connections[i].src.pieceType) {
                    case PieceType.DeadEndRight:
                        newSrcPieceModel = Instantiate(deadEndStair, srcPos, Quaternion.identity);
                        newSrcPieceModel.name = "Stairs_DeadEndRight";
                        break;
                    case PieceType.DeadEndBottom:
                        newSrcPieceModel = Instantiate(deadEndStair, srcPos, Quaternion.Euler(0, 90, 0));
                        newSrcPieceModel.name = "Stairs_DeadEndBottom";
                        break;
                    case PieceType.DeadEndLeft:
                        newSrcPieceModel = Instantiate(deadEndStair, srcPos, Quaternion.Euler(0, 180, 0));
                        newSrcPieceModel.name = "Stairs_DeadEndLeft";
                        break;
                    case PieceType.DeadEndTop:
                        newSrcPieceModel = Instantiate(deadEndStair, srcPos, Quaternion.Euler(0, 270, 0));
                        newSrcPieceModel.name = "Stairs_DeadEndTop";
                        break;
                }
                srcPiece.pieceModel = newSrcPieceModel;
                dstPiece.pieceModel = null;
                Debug.Log($"Built connection {srcPos} -> {dstPos}");
            }

            Debug.Log($"{numConnections} Connections Built between levels {mazeConfigs.src.level} -> {mazeConfigs.dst.level}");
        }
    }
}
