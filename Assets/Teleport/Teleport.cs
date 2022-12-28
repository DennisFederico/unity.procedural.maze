using UnityEngine;
using UnityEngine.Serialization;

public class Teleport : MonoBehaviour {
    public Vector3 destination;
    public AudioSource teleportSound;

    private void OnTriggerEnter(Collider col) {
        //ensure the player character is tagged in the Inspector as "Player"
        if (col.gameObject.CompareTag("Player")) {
            col.gameObject.transform.position = destination + new Vector3(0, 1, 0);
            teleportSound.Play();
        }
    }
}