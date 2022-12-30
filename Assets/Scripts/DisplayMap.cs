using narkdagas.mazegenerator;
using UnityEngine;

public class DisplayMap : MonoBehaviour {
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            RaycastHit hit;
            Ray ray = new Ray(this.transform.position, Vector3.down);
            if (Physics.Raycast(ray, out hit)) {
                var thisMaze = hit.collider.gameObject.GetComponentInParent<Maze>();
                var currentLocation = hit.collider.gameObject.GetComponent<PieceLocation>().location;
                Debug.Log($"Show Map for level {thisMaze.mazeConfig.level} @[{currentLocation.x},{currentLocation.z}]:{hit.collider.gameObject.transform.position}");
                GameManager.Instance.ToggleMap(thisMaze.mazeConfig.level, currentLocation);
            }
        }
    }
}