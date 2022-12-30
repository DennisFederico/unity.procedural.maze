using System;
using UnityEngine;

public class Grimoire : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            GameManager.Instance.EndGame();
        }
    }
}