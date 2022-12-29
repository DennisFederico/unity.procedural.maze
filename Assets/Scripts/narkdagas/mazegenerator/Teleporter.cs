using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace narkdagas.mazegenerator {
    [RequireComponent(typeof(DungeonMazeManager))]
    public class Teleporter : MonoBehaviour {
        [SerializeField] private GameObject teleportPrefab;
        public int startMaze;
        public int endMaze;
        public PieceType[] validPlacing = new[] { PieceType.DeadEndTop, PieceType.DeadEndBottom, PieceType.DeadEndLeft, PieceType.DeadEndRight };

        public void Place(Maze fromMaze, Maze toMaze) {
            List<PieceData> startLocations = new();
            List<PieceData> endLocations = new();
            
            foreach (var piece in fromMaze.pieces) {
                if (piece == null) continue;
                if (!validPlacing.Contains(piece.pieceType)) continue;
                startLocations.Add(piece);
            }
            
            foreach (var piece in toMaze.pieces) {
                if (piece == null) continue;
                if (!validPlacing.Contains(piece.pieceType)) continue;
                endLocations.Add(piece);
            }

            if (startLocations.Count > 0 && endLocations.Count > 0) {
                var startParent = startLocations[Random.Range(0, startLocations.Count)].pieceModel.transform;
                var startPosition = startParent.position;
                var endPosition = endLocations[Random.Range(0, endLocations.Count)].pieceModel.transform.position;
                
                // Place Teleporter
                var destination = Instantiate(teleportPrefab, startPosition, Quaternion.identity, startParent);
                destination.GetComponent<Teleport>().destination = endPosition;
                destination.name = $"Teleport_{startMaze}_{endMaze}";
                Debug.Log($"Teleport build from lvl:{startMaze}@{startPosition} to lvl:{endMaze}@{endPosition}");
            } else {
                Debug.Log($"Cannot build teleport from {startMaze} to {endMaze}");                                
            }
        }
    }
}