using System.Collections.Generic;
using UnityEngine;

namespace narkdagas.mazegenerator {
    
    public class FindAStarPath : MonoBehaviour {
        private readonly List<PathMarker> _openMarkers = new();
        private readonly List<PathMarker> _closedMarkers = new();
        private (PathMarker startNode, PathMarker goalNode) _pathEnds;
        private PathMarker _lastMarker;
        private bool _done;

        void BeginSearch(Maze maze, Maze.MapLocation startLocation, Maze.MapLocation endLocation) {
            _done = false;
            float g = 0;
            float h = Vector2.Distance(startLocation.ToVector(), endLocation.ToVector());
            float f = g + h;

            var startMarker = new PathMarker(startLocation, g, h, f, null);
            var goalMarker = new PathMarker(endLocation, 0, 0, 0, null);

            _openMarkers.Clear();
            _closedMarkers.Clear();
            _openMarkers.Add(startMarker);
            _lastMarker = startMarker;

            _pathEnds = (startMarker, goalMarker);
        }

        void Search(Maze maze, PathMarker thisNode) {
            if (thisNode.Equals(_pathEnds.goalNode)) {
                _done = true;
                return;
            }

            foreach (var direction in maze.directions) {
                Maze.MapLocation neighbour = direction + thisNode.mapLocation;
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

        public PathMarker FindPath(Maze maze, Maze.MapLocation start, Maze.MapLocation end) {
            BeginSearch(maze, start, end);
            while (!_done) {
                Search(maze, _lastMarker);
            }

            return _lastMarker;
        }
    }
}