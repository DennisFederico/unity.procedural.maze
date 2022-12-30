using UnityEngine;

namespace narkdagas.mazegenerator {
    public class MinimapCamera : MonoBehaviour {
    
        private bool _initialized;
        private Camera _minimapCamera;
        private Transform _player;
        [SerializeField] private float yOffset = 5f;

        public void Initialize(Transform followTarget) {
            _player = followTarget;
            _minimapCamera = GetComponent<Camera>();
            //_minimapCamera.targetTexture = targetTexture;
            _initialized = true;
        }

        private void LateUpdate() {
            if (!_initialized) return;
            var newPos = _player.position;
            newPos.y += yOffset;
            transform.position = newPos;
        
            transform.rotation = Quaternion.Euler(90f, _player.eulerAngles.y, 0);
        }
    }
}