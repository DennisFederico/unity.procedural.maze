using UnityEngine;
using UnityEngine.UI;

namespace narkdagas.mazegenerator {
    public class MapGenerator : MonoBehaviour {

        [SerializeField] private Image imageHolder;
        [SerializeField] private Texture2D mapImage;
        
        public void GenerateMap(Maze maze) {
            Debug.Log($"texture - Width:{mapImage.width}, Height:{mapImage.height}, Format:{mapImage.format.ToString()}");
        } 
    }
}