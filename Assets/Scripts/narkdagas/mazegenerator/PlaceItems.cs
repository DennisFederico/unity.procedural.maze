using System.Linq;
using UnityEngine;

namespace narkdagas.mazegenerator {
    public class PlaceItems : MonoBehaviour {
        public GameObject[] itemPrefabs;
        public PieceType[] validPlacing;
        public float itemChance;
        public bool iterateItems;
        private Maze _maze;

        private readonly PieceType[] _rotateTypes = {
            PieceType.CorridorVertical,
            PieceType.DeadEndLeft,
            PieceType.DeadEndRight,
            PieceType.JunctionLeft,
            PieceType.JunctionRight
        };

        public void PlaceItemsForMaze() {
            _maze = GetComponent<Maze>();
            if (_maze) {
                int itemCount = 0;
                //var heightPlacement = maze.mazeConfig.level * maze.mazeConfig.heightScale;
                foreach (PieceData piece in _maze.pieces) {
                    if (piece == null) continue;
                    if (!validPlacing.Contains(piece.pieceType)) continue;
                    if (Random.Range(0, 100) > itemChance) continue;
                    var prefab = iterateItems ? itemPrefabs[itemPrefabs.Length % itemCount] : itemPrefabs.GetRandomPiece();
                    Quaternion rotation = _rotateTypes.Contains(piece.pieceType) ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
                    Instantiate(prefab, piece.pieceModel.transform.position, rotation, piece.pieceModel.transform);
                    itemCount++;
                }
            }
        }
    }
}