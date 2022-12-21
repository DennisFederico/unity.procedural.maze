using UnityEngine;

namespace narkdagas.mazegenerator {
    public class Crawler : MazeGenerator {
        protected override void GenerateMap() {
            var minHeight = mazeConfig.height / 6;
            SingleHorizontalCrawl(Random.Range(minHeight, mazeConfig.height - minHeight));
            var minWidth = mazeConfig.width / 6;
            SingleVerticalCrawl(Random.Range(minWidth, mazeConfig.width - minWidth));

            SingleHorizontalCrawl(Random.Range(minHeight, mazeConfig.height - minHeight));
            SingleVerticalCrawl(Random.Range(minWidth, mazeConfig.width - minWidth));
            SingleHorizontalCrawl(Random.Range(minHeight, mazeConfig.height - minHeight));
            SingleVerticalCrawl(Random.Range(minWidth, mazeConfig.width - minWidth));
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

                done |= (x < 0 || x >= mazeConfig.width || z < 0 || z >= mazeConfig.height);
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

                done |= (x < 0 || x >= mazeConfig.width || z < 0 || z >= mazeConfig.height);
            }
        }
    }
}