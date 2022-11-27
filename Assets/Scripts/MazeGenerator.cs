using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour {
    public GameObject cube;
    public IntVector2 mazeSize;
    public byte[,] map;
    public int scale = 6;

    [Serializable]
    public struct IntVector2 {
        public int width;
        public int height;
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

    private void DrawMap() {
        for (int z = 0; z < mazeSize.height; z++) {
            for (int x = 0; x < mazeSize.width; x++) {
                if (map[x, z] == 1) {
                    Vector3 pos = new Vector3(x * scale, 0, z * scale);
                    GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wall.transform.localScale = new Vector3(scale, scale, scale);
                    wall.transform.position = pos;
                    //Instantiate(cube, pos, Quaternion.identity);
                }
            }
        }
    }

    // Update is called once per frame
    void Update() { }
}