using System;
using System.Collections;
using System.Collections.Generic;
using narkdagas.mazegenerator;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MazeManager : MonoBehaviour {
    public MazeGenerator[] mazes;
    public byte width;
    public byte depth;

    public GameObject straightManholeUp;
    public GameObject straightManholeDown;
    public GameObject deadEndManholeUp;
    public GameObject deadEndManholeDown;

    private void Start() {
        Debug.Log("Start MazeManager");
        GenerateMazes();
        ConnectLevels();
    }

    void GenerateMazes() {
        
        byte level = 0;
        foreach (MazeGenerator maze in mazes) {
            Debug.Log($"Building maze for level: {level}");
            maze.Build(width, depth, level++);
        }
    }

    void ConnectLevels() {
        Debug.Log("Connecting Levels...");
        for (int level = 0; level < mazes.Length - 1; level++) {
            Debug.Log($"Connecting maze {level} to {level+1}");
            var connectionsFound = GetConnectionCandidates(mazes[level].pieces, mazes[level + 1].pieces);
            BuildConnections(connectionsFound, (mazes[level].mazeConfig, mazes[level+1].mazeConfig), mazes[level].numLaddersRange.x, mazes[level].numLaddersRange.y);    
        }
    }
    
    //TODO CHECK THE MAZE TYPE FOR VALID CONNECTIONS MATCHES
    IList<(PieceData src, PieceData dst)> GetConnectionCandidates(PieceData[,] maze1, PieceData[,] maze2) {
        IList<(PieceData src, PieceData dst)> connections = new List<(PieceData src, PieceData dst)>();
        for (byte z = 0; z < depth; z++) {
            for (byte x = 0; x < width; x++) {
                if (maze1[x, z].pieceType == maze2[x, z].pieceType) {
                    if (maze1[x, z].pieceType is 
                        PieceType.CorridorHorizontal or 
                        PieceType.CorridorVertical or 
                        PieceType.DeadEndBottom or 
                        PieceType.DeadEndTop or 
                        PieceType.DeadEndLeft or 
                        PieceType.DeadEndRight) {
                        connections.Add((maze1[x, z], maze2[x,z]));
                    }
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

            Vector3 srcPos = new Vector3(srcPiece.posX * mazeConfigs.src.scale, mazeConfigs.src.level * mazeConfigs.src.scale * 2, srcPiece.posZ * mazeConfigs.src.scale);
            srcPiece.pieceType = PieceType.LadderUp;
            Vector3 dstPos = new Vector3(dstPiece.posX * mazeConfigs.dst.scale, mazeConfigs.dst.level * mazeConfigs.dst.scale * 2, dstPiece.posZ * mazeConfigs.dst.scale);
            dstPiece.pieceType = PieceType.LadderDown;
            
            GameObject newSrcPieceModel = null;
            GameObject newDstPieceModel = null;
            switch (connections[i].src.pieceType) {
                case PieceType.CorridorHorizontal:
                    newSrcPieceModel = Instantiate(straightManholeUp, srcPos, Quaternion.Euler(0, 90, 0));
                    newDstPieceModel = Instantiate(straightManholeDown, dstPos, Quaternion.Euler(0, 90, 0));
                    break;
                case PieceType.CorridorVertical:
                    newSrcPieceModel = Instantiate(straightManholeUp, srcPos, Quaternion.identity);
                    newDstPieceModel = Instantiate(straightManholeDown, dstPos, Quaternion.identity);
                    break;
                case PieceType.DeadEndTop:
                    newSrcPieceModel = Instantiate(deadEndManholeUp, srcPos, Quaternion.identity);
                    newDstPieceModel = Instantiate(deadEndManholeDown, dstPos, Quaternion.identity);
                    break;
                case PieceType.DeadEndRight:
                    newSrcPieceModel = Instantiate(deadEndManholeUp, srcPos, Quaternion.Euler(0, 270, 0));
                    newDstPieceModel = Instantiate(deadEndManholeDown, dstPos, Quaternion.Euler(0, 270, 0));
                    break;
                case PieceType.DeadEndBottom:
                    newSrcPieceModel = Instantiate(deadEndManholeUp, srcPos, Quaternion.Euler(0, 180, 0));
                    newDstPieceModel = Instantiate(deadEndManholeDown, dstPos, Quaternion.Euler(0, 180, 0));
                    break;
                case PieceType.DeadEndLeft:
                    newSrcPieceModel = Instantiate(deadEndManholeUp, srcPos, Quaternion.Euler(0, 90, 0));
                    newDstPieceModel = Instantiate(deadEndManholeDown, dstPos, Quaternion.Euler(0, 90, 0));
                    break;
            }
            
            srcPiece.pieceModel = newSrcPieceModel;
            dstPiece.pieceModel = newDstPieceModel;
            Debug.Log($"Built connection {srcPos} -> {dstPos}");
        }
        Debug.Log($"{numConnections} Connections Built between levels {mazeConfigs.src.level} -> {mazeConfigs.dst.level}");
    }
}