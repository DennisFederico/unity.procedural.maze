using System.Linq;

namespace narkdagas.mazegenerator {
    
    public enum MazePiece {
        CorridorHorizontal, 
        CorridorVertical, 
        CornerTopRight, 
        CornerTopLeft, 
        CornerBottomRight, 
        CornerBottomLeft, 
        DeadEndTop, 
        DeadEndRight,
        DeadEndLeft,
        DeadEndBottom,
        JunctionTop,
        JunctionRight,
        JunctionBottom,
        JunctionLeft,
        Intersection,
        UnknownPiece
    }
        
    static class MazePieceExtensions {
        public static MazePiece MatchMazePiece(this byte[] pattern) {
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR
                })) return MazePiece.CorridorHorizontal;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL
                })) return MazePiece.CorridorVertical;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL
                })) return MazePiece.CornerTopRight;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR
                })) return MazePiece.CornerTopLeft;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL
                })) return MazePiece.CornerBottomRight;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR
                })) return MazePiece.CornerBottomLeft;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL
                })) return MazePiece.DeadEndTop;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL
                })) return MazePiece.DeadEndRight;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL
                })) return MazePiece.DeadEndBottom;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR
                })) return MazePiece.DeadEndLeft;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR
                })) return MazePiece.JunctionTop;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL
                })) return MazePiece.JunctionRight;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR
                })) return MazePiece.JunctionBottom;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.WALL, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR
                })) return MazePiece.JunctionLeft;
            if (Enumerable.SequenceEqual(pattern, new [] {
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR, 
                    MazeGenerator.MazeCellInfo.CORRIDOR
                })) return MazePiece.Intersection;
            return MazePiece.UnknownPiece;
        }

        public static byte[] GetCrossNeighboursForMazePiece(this byte[,] map, int x, int z) {
            return new[] {
                map[x, z + 1], //UP
                map[x + 1, z], //RIGHT
                map[x, z - 1], //DOWN
                map[x - 1, z]  //LEFT
            };
        }
        // public static byte[] GetAllNeighboursForMazePiece(this byte[,] map, int x, int z) {
        //     return new[] {
        //         map[x - 1, z + 1], //UP/LEFT
        //         map[x, z + 1], //UP
        //         map[x + 1, z + 1], //UP/RIGHT
        //         map[x + 1, z],  //RIGHT
        //         map[x + 1, z - 1], //DOWN/RIGHT
        //         map[x, z - 1], //DOWN
        //         map[x - 1, z - 1] //DOWN/LEFT
        //     };
        // }
    }

}