using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crawler : MazeGenerator {
    public override void GenerateMap() {
        var minHeight = mazeSize.height / 6;
        SingleHorizontalCrawl(Random.Range(minHeight, mazeSize.height - minHeight));
        var minWidth = mazeSize.width / 6;
        SingleVerticalCrawl(Random.Range(minWidth, mazeSize.width - minWidth));
        
        SingleHorizontalCrawl(Random.Range(minHeight, mazeSize.height - minHeight));
        SingleVerticalCrawl(Random.Range(minWidth, mazeSize.width - minWidth));
        SingleHorizontalCrawl(Random.Range(minHeight, mazeSize.height - minHeight));
        SingleVerticalCrawl(Random.Range(minWidth, mazeSize.width - minWidth));
    }

    private void SingleHorizontalCrawl(int startZ) {
        bool done = false;
        int x = 0;
        int z = startZ;

        while (!done) {
            map[x, z] = 0;
            if (Random.Range(0, 100) < 40) {
                x += Random.Range(0, 2);
            } else {
                z += Random.Range(-1, 2);
            }

            done |= (x < 0 || x >= mazeSize.width || z < 0 || z >= mazeSize.height);
        }
    }

    private void SingleVerticalCrawl(int startX) {
        bool done = false;
        int x = startX;
        int z = 0;

        while (!done) {
            map[x, z] = 0;
            if (Random.Range(0, 100) < 40) {
                z += Random.Range(0, 2);
            } else {
                x += Random.Range(-1, 2);
            }

            done |= (x < 0 || x >= mazeSize.width || z < 0 || z >= mazeSize.height);
        }
    }
}