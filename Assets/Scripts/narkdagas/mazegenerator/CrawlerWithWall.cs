using UnityEngine;

namespace narkdagas.mazegenerator {
    public class CrawlerWithWall : MazeGenerator {
        protected override void GenerateMap() {
            var minHeight = mazeConfig.height / 6;
            SingleHorizontalCrawl(Random.Range(minHeight, mazeConfig.height - minHeight));
            SingleHorizontalCrawl(Random.Range(minHeight, mazeConfig.height - minHeight));

            var minWidth = mazeConfig.width / 6;
            SingleVerticalCrawl(Random.Range(minWidth, mazeConfig.width - minWidth));
            SingleVerticalCrawl(Random.Range(minWidth, mazeConfig.width - minWidth));
            SingleVerticalCrawl(Random.Range(minWidth, mazeConfig.width - minWidth));
        }

        private void SingleHorizontalCrawl(int startZ) {
            bool done = false;
            int x = 1;
            int z = startZ;

            while (!done) {
                map[x, z] = 0;
                if (Random.Range(0, 100) < 40) {
                    x += Random.Range(0, 2);
                } else {
                    z += Random.Range(-1, 2);
                }

                done |= (x < 1 || x >= mazeConfig.width - 1 || z < 1 || z >= mazeConfig.height - 1);
            }
        }

        private void SingleVerticalCrawl(int startX) {
            bool done = false;
            int x = startX;
            int z = 1;

            while (!done) {
                map[x, z] = 0;
                if (Random.Range(0, 100) < 40) {
                    z += Random.Range(0, 2);
                } else {
                    x += Random.Range(-1, 2);
                }

                done |= (x < 1 || x >= mazeConfig.width - 1 || z < 1 || z >= mazeConfig.height - 1);
            }
        }
    }
}