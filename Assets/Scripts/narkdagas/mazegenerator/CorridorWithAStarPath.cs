using System.Collections.Generic;
using UnityEngine;

namespace narkdagas.mazegenerator {
    public class PathMarker {
        public readonly MazeGenerator.CellLocation mapLocation;
        public float g;
        public float h;
        public float f;
        public PathMarker parent;

        public PathMarker(MazeGenerator.CellLocation mapLocation, float g, float h, float f, PathMarker parent) {
            this.mapLocation = mapLocation;
            this.g = g;
            this.h = h;
            this.f = f;
            this.parent = parent;
        }

        protected bool Equals(PathMarker other) {
            return Equals(mapLocation, other.mapLocation);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PathMarker)obj);
        }

        public override int GetHashCode() {
            return mapLocation.GetHashCode();
        }
    }

    public class CorridorWithAStarPath : MonoBehaviour {
        [SerializeField] private MazeGenerator maze;
        private readonly List<PathMarker> _openMarkers = new();
        private readonly List<PathMarker> _closedMarkers = new();
        private (PathMarker startNode, PathMarker goalNode) _pathEnds;
        private PathMarker _lastMarker;
        private bool _done;

        (PathMarker start, PathMarker goal) BeginSearch() {
            _done = false;

            // NOT WALLS
            List<MazeGenerator.CellLocation> notWalls = new List<MazeGenerator.CellLocation>();
            for (int x = 1; x < maze.mazeConfig.width - 1; x++) {
                for (int z = 1; z < maze.mazeConfig.height - 1; z++) {
                    if (maze.map[x, z] != 1) {
                        notWalls.Add(new MazeGenerator.CellLocation(x, z));
                    }
                }
            }

            notWalls.ShuffleCurrent();

            var startLocation = new MazeGenerator.CellLocation(notWalls[0].x, notWalls[0].z);
            var goalLocation = new MazeGenerator.CellLocation(notWalls[1].x, notWalls[1].z);

            float g = 0;
            float h = Vector2.Distance(startLocation.ToVector(), goalLocation.ToVector());
            float f = g + h;

            var startMarker = new PathMarker(startLocation, g, h, f, null);
            var goalMarker = new PathMarker(goalLocation, 0, 0, 0, null);

            _openMarkers.Clear();
            _closedMarkers.Clear();
            _openMarkers.Add(startMarker);
            _lastMarker = startMarker;

            return (startMarker, goalMarker);
        }

        void Search(PathMarker thisNode) {
            if (thisNode.Equals(_pathEnds.goalNode)) {
                _done = true;
                return;
            }

            foreach (var direction in maze.directions) {
                MazeGenerator.CellLocation neighbour = direction + thisNode.mapLocation;
                if (neighbour.x <= 0 || neighbour.x >= maze.mazeConfig.width) continue;
                if (neighbour.z <= 0 || neighbour.z >= maze.mazeConfig.height) continue;
                if (maze.map[neighbour.x, neighbour.z] == 1) continue;
                if (_closedMarkers.ContainsMapLocation(neighbour)) continue;

                float g = thisNode.g + Vector2.Distance(thisNode.mapLocation.ToVector(), neighbour.ToVector());
                float h = Vector2.Distance(neighbour.ToVector(), _pathEnds.goalNode.mapLocation.ToVector());
                float f = g + h;

                _openMarkers.UpdateOrAdd(neighbour, g, h, f, thisNode);
            }

            //Order the list by F
            _openMarkers.Sort((marker, marker1) => (int)(marker.f - marker1.f));
            var marker = _openMarkers[0];
            _closedMarkers.Add(marker);
            _openMarkers.RemoveAt(0);
            _lastMarker = marker;
        }

        void MarkPath() {
            PathMarker current = _lastMarker;

            int steps = 0;
            while (current != null) {
                maze.map[current.mapLocation.x, current.mapLocation.z] = 0;
                current = current.parent;
                steps++;
            }

            Debug.Log($"Path with {steps} steps found!");
        }

        public (PathMarker startNode, PathMarker goalNode) Build() {
            maze = GetComponent<MazeGenerator>();
            _pathEnds = BeginSearch();
            while (!_done) {
                Search(_lastMarker);
            }
            maze.InitializeMap();
            MarkPath();

            return _pathEnds;
        }
    }
}