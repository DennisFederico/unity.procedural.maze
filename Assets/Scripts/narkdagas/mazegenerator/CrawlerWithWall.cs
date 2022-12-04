using UnityEngine;

namespace narkdagas.mazegenerator {
    public class CrawlerWithWall : MazeGenerator {
        protected override void GenerateMap() {
            var minHeight = mazeSize.height / 6;
            SingleHorizontalCrawl(Random.Range(minHeight, mazeSize.height - minHeight));
            SingleHorizontalCrawl(Random.Range(minHeight, mazeSize.height - minHeight));

            var minWidth = mazeSize.width / 6;
            SingleVerticalCrawl(Random.Range(minWidth, mazeSize.width - minWidth));
            SingleVerticalCrawl(Random.Range(minWidth, mazeSize.width - minWidth));
            SingleVerticalCrawl(Random.Range(minWidth, mazeSize.width - minWidth));
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

                done |= (x < 1 || x >= mazeSize.width - 1 || z < 1 || z >= mazeSize.height - 1);
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

                done |= (x < 1 || x >= mazeSize.width - 1 || z < 1 || z >= mazeSize.height - 1);
            }
        }
    }
}