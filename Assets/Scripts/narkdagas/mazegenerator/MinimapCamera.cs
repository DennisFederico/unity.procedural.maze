using UnityEngine;

namespace narkdagas.mazegenerator {
    public class MinimapCamera : MonoBehaviour {
    
        private bool _initialized;
        private Camera _minimapCamera;
        private Transform _player;
        [SerializeField] private float yOffset = 5f;

        public void Initialize(Transform followTarget, RenderTexture texture) {
            _player = followTarget;
            _minimapCamera = GetComponent<Camera>();
            _minimapCamera.targetTexture = texture;
            _initialized = true;
        }
        
        private void LateUpdate() {
            if (GameManager.Instance.gameOver || !_initialized) return;
            var newPos = _player.position;
            newPos.y += yOffset;
            transform.position = newPos;
            transform.rotation = Quaternion.Euler(90f, _player.eulerAngles.y, 0);
        }
    }
}