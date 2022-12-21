using System;
using System.Collections;
using System.Collections.Generic;
using narkdagas.mazegenerator;
using UnityEngine;
using UnityEngine.Serialization;

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
        byte level = 0;
        foreach (MazeGenerator maze in mazes) {
            Debug.Log($"Building maze for level: {level}");
            maze.Build(width, depth, level++);
        }

        Debug.Log("Connecting Levels");
        int connectionsFound = 0;
        for (byte z = 0; z < depth; z++) {
            for (byte x = 0; x < width; x++) {
                if (mazes[0].pieces[x, z].pieceType == mazes[1].pieces[x, z].pieceType) {
                    if (mazes[0].pieces[x, z].pieceType == PieceType.None) continue;
                    
                    //Debug.Log($"Found same piece Type {mazes[0].pieces[x, z].pieceType}");
                    if (mazes[0].pieces[x, z].pieceType != PieceType.CorridorVertical) continue;
                    Debug.Log($"Found same piece Type {mazes[0].pieces[x, z].pieceType} @[{x},{z}]");
                    
                    Destroy(mazes[0].pieces[x, z].pieceModel);
                    Destroy(mazes[1].pieces[x, z].pieceModel);
                    Vector3 upManholePos = new Vector3(
                        x * mazes[0].scale,
                        mazes[0].mazeConfig.level * mazes[0].scale * 2,
                        z * mazes[0].scale
                    );
                    mazes[0].pieces[x, z].pieceType = PieceType.ManholeUp;
                    mazes[0].pieces[x, z].pieceModel = Instantiate(straightManholeUp, upManholePos, Quaternion.identity);
                        
                    Vector3 downManholePos = new Vector3(
                        x * mazes[1].scale,
                        mazes[1].mazeConfig.level * mazes[1].scale * 2,
                        z * mazes[1].scale
                    );
                    mazes[1].pieces[x, z].pieceType = PieceType.ManholeDown;
                    mazes[1].pieces[x, z].pieceModel = Instantiate(straightManholeDown, downManholePos, Quaternion.identity);

                    connectionsFound++;
                    Debug.Log($"Found connection #{connectionsFound} at Pos:{upManholePos} -> {downManholePos}");
                }
            }
        }
    }
}