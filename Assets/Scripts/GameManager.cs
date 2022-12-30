using System;
using narkdagas.mazegenerator;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {
    [SerializeField] private DungeonMazeManager mazeManager;
    [SerializeField] private RectTransform displayMapCanvas;
    private RawImage _displayMapRawImage;
    public bool gameOver;

    private void Start() {
        _displayMapRawImage = displayMapCanvas.gameObject.GetComponentInChildren<RawImage>();
        mazeManager.Init();
        CanvasManager.Instance.StartGame();
        gameOver = false;
    }

    public void EndGame() {
        CanvasManager.Instance.EndGame();
        gameOver = true;
    }

    private void Update() {
        if (!gameOver) return;
        if (Input.GetKeyDown(KeyCode.R)) {
            SceneManager.LoadScene(0);
        }
    }

    public void ToggleMap(int level, Maze.MapLocation playerLocation) {
        displayMapCanvas.gameObject.SetActive(!displayMapCanvas.gameObject.activeInHierarchy);
        if (displayMapCanvas.gameObject.activeInHierarchy) {
            var minimapCopy = new Texture2D(DungeonMazeManager.Instance.mazes[level].minimap.width, DungeonMazeManager.Instance.mazes[level].minimap.height);
            minimapCopy.hideFlags = HideFlags.HideAndDontSave;
            minimapCopy.SetPixels(DungeonMazeManager.Instance.mazes[level].minimap.GetPixels());
            var minimapPixelSize = DungeonMazeManager.Instance.mazes[level].minimapPixelSize;
            var startX = playerLocation.x * minimapPixelSize;
            var startY = playerLocation.z * minimapPixelSize;
            Color[] playerSpot = new Color[minimapPixelSize * minimapPixelSize];
            Array.Fill(playerSpot, Color.blue);
            minimapCopy.SetPixels(startX, startY, minimapPixelSize, minimapPixelSize, playerSpot);
            minimapCopy.Apply();
            _displayMapRawImage.texture = minimapCopy;
            //_displayMapRawImage.texture = DungeonMazeManager.Instance.mazes[level].minimap;            
        } else {
            Destroy(_displayMapRawImage.texture);
        }
    }
}