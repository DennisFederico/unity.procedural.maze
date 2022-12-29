using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace narkdagas.mazegenerator {
    
    public enum PieceType {
        None,
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
        OpenRoom,
        LadderUp,
        LadderDown,
        Custom
    }

    public class PieceData {
        public Maze.MapLocation pieceLocation;
        public PieceType pieceType;
        public GameObject pieceModel;

        public PieceData(Maze.MapLocation location, PieceType type, GameObject model) {
            this.pieceLocation = location;
            pieceType = type;
            pieceModel = model;
        }
    }
    
    static class MazePieceExtensions {
        
        // MATCHING CLOCKWISE FROM TOP-LEFT
        //          |---|---|---|
        //          | 0 | 1 | 2 |
        //          |---|---|---|
        //          | 7 | X | 3 |
        //          |---|---|---|
        //          | 6 | 5 | 4 |
        //          |---|---|---|
        public static PieceType MatchMazePiece(this byte[] neighbours) {

            if (neighbours.SequenceEqual(new [] {
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.OpenRoom;
            
            if (neighbours.SequenceEqual(new [] {
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.Intersection;
            
            // T-Junctions
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.JunctionTop;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Wall
                })) return PieceType.JunctionRight;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.JunctionBottom;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.JunctionLeft;
            
            //CORNERS
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Wall
                })) return PieceType.CornerTopRight;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.CornerTopLeft;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor, 
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Wall
                })) return PieceType.CornerBottomRight;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Any, 
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Wall, 
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.CornerBottomLeft;

            //Corridors
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.CorridorHorizontal;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall
                })) return PieceType.CorridorVertical;
            
            //Dead Ends
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall
                })) return PieceType.DeadEndTop;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall
                })) return PieceType.DeadEndRight;
            if (neighbours.MatchPattern(new [] {
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall
                })) return PieceType.DeadEndBottom;
            if (neighbours.MatchPattern( new [] {
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Wall,
                    (byte) Maze.MapLocationType.Any,
                    (byte) Maze.MapLocationType.Corridor
                })) return PieceType.DeadEndLeft;
            
            
            return PieceType.Custom;
        }

        // CROSS NEIGHBOURS FROM TOP
        //              |---|
        //              | 0 |
        //          |---|---|---|
        //          | 3 | X | 1 |
        //          |---|---|---|
        //              | 2 | 
        //              |---|
        public static byte[] GetCrossNeighboursForMazePiece(this byte[,] map, int x, int z) {
            return new[] {
                map[x, z + 1], //UP
                map[x + 1, z], //RIGHT
                map[x, z - 1], //DOWN
                map[x - 1, z]  //LEFT
            };
        }

        public static T CircularIndexValue<T>(this T[] array, int index) {
            //Using modulo we can index past the array length
            //For negative indexes we "inverse" the indexing subtracting from length of the array the modulo of the abs index 
            // Assume and array of Length = 4
            // Requested Index  |-9|-8|-7|-6|-5|-4|-3|-2|-1|(0)|+1|+2|+3|+4|+5|+6|+7|+8|+9|
            //      Real Index  | 3| 0| 1| 2| 3| 0| 1| 2| 3| 0 | 1| 2| 3| 0| 1| 2| 3| 0| 1|
            // 4-(abs(-9)%4)=3 | 4-(abs(-7)%4)=1 | 4-(abs(-3)%4)=1 | 4-(abs(-1)%4)=3 ...
            var circularIndex = index < 0 ? array.Length-((index * -1) % array.Length) : index % array.Length;
            return array[circularIndex];
        }
        
        // NEIGHBOURS CLOCKWISE FROM TOP-LEFT
        //          |---|---|---|
        //          | 0 | 1 | 2 |
        //          |---|---|---|
        //          | 7 | X | 3 |
        //          |---|---|---|
        //          | 6 | 5 | 4 |
        //          |---|---|---|
        public static byte[] GetAllNeighboursForMazePiece(this byte[,] map, int x, int z) {
            return new[] {
                map[x - 1, z + 1], //UP/LEFT
                map[x, z + 1], //UP
                map[x + 1, z + 1], //UP/RIGHT
                map[x + 1, z],  //RIGHT
                map[x + 1, z - 1], //DOWN/RIGHT
                map[x, z - 1], //DOWN
                map[x - 1, z - 1], //DOWN/LEFT
                map[x - 1, z] //LEFT
            };
        }

        private static bool MatchPattern(this byte[] compare, byte[] pattern) {
            if (compare.Length != pattern.Length) return false;
            for (int i = 0; i < compare.Length; i++) {
                if (pattern[i] != (int)Maze.MapLocationType.Any && compare[i] != pattern[i]) {
                    return false;
                }
            }
            return true;
        }

        public static IList<PieceData> OfTypes(this PieceData[,] pieces, PieceType[] types) {
            IList<PieceData> found = new List<PieceData>();
            foreach (var pieceData in pieces) {
                if (pieceData == null) continue;
                if (types.Contains(pieceData.pieceType)) {
                    found.Add(pieceData);
                }
            }
            return found;
        }
        
        public static IList<PieceData> OfType(this PieceData[] pieces, PieceType type) {
            IList<PieceData> found = new List<PieceData>();
            foreach (var pieceData in pieces) {
                if (pieceData?.pieceType == type) {
                    found.Add(pieceData);
                }
            }
            return found;
        }
    }
}