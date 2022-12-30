using System.Collections;
using System.Collections.Generic;
using narkdagas.mazegenerator;
using UnityEngine;

[RequireComponent(typeof(FindAStarPath))]
public class Clairvoyance : MonoBehaviour {
    public GameObject particles;
    public float effectDuration = 10f;
    private FindAStarPath _fastarp;

    private void Start() {
        _fastarp = GetComponent<FindAStarPath>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.C)) {
            if (_fastarp) {
                RaycastHit hit;
                Ray ray = new Ray(this.transform.position, Vector3.down);
                if (Physics.Raycast(ray, out hit)) {
                    var thisMaze = hit.collider.gameObject.GetComponentInParent<Maze>();
                    var startLocation = hit.collider.gameObject.GetComponent<PieceLocation>().location;
                    var endLocation = thisMaze.exitLocations.GetRandom();
                    Debug.Log($"Casting Clairvoyance for level {thisMaze.mazeConfig.level} @start[{startLocation.x},{startLocation.z}]:{hit.collider.gameObject.transform.position}");
                    Debug.Log($"to @end[{endLocation.x},{endLocation.z}]@{thisMaze.pieces[endLocation.x, endLocation.z].pieceModel.transform.position}");
                    var destination = _fastarp.FindPath(thisMaze, startLocation, endLocation);
                    Debug.Log($"Clairvoyance found path for @destination[{destination.mapLocation.x},{destination.mapLocation.z}]:{thisMaze.pieces[destination.mapLocation.x, destination.mapLocation.z].pieceModel.transform.position}");
                    StartCoroutine(DisplayMagicPath(thisMaze, destination));
                }
            }
        }
    }

    private IEnumerator DisplayMagicPath(Maze maze, PathMarker destination) {
        
        List<Maze.MapLocation> magicPath = new();
        while (destination != null) {
            magicPath.Add(destination.mapLocation);
            destination = destination.parent;
        }
        magicPath.Reverse();

        var magic = Instantiate(particles, transform.position, Quaternion.identity);
        foreach (var pathStep in magicPath) {
            magic.transform.LookAt(maze.pieces[pathStep.x, pathStep.z].pieceModel.transform.position + Vector3.up);

            int loopTimeout = 0;
            while (Vector2.Distance(new Vector2(magic.transform.position.x, magic.transform.position.z),
                       new Vector2(maze.pieces[pathStep.x, pathStep.z].pieceModel.transform.position.x,
                           maze.pieces[pathStep.x, pathStep.z].pieceModel.transform.position.z)) > 2
                        && loopTimeout < 100) {
                magic.transform.Translate(0,0, 10f * Time.deltaTime);
                yield return new WaitForSeconds(0.01f);
                loopTimeout++;
            }
        }
        Destroy(magic, effectDuration);
    }
}