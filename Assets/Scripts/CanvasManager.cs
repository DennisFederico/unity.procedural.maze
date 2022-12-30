using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : Singleton<CanvasManager> {
    [SerializeField] private GameObject[] activeDuringGame;
    [SerializeField] private GameObject[] activeOnGameOver;

    public void StartGame() {
        ProcessIsGameActive(true);
    }

    public void EndGame() {
        ProcessIsGameActive(false);
    }

    private void ProcessIsGameActive(bool isGameActive) {
        foreach (var go in activeDuringGame) {
            go.SetActive(isGameActive);            
        }

        foreach (var go in activeOnGameOver) {
            go.SetActive(!isGameActive);
        }
    }
}