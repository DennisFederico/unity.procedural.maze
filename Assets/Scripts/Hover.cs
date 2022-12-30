using UnityEngine;

public class Hover : MonoBehaviour {

    private Vector3 _startPosition;
    private readonly float _amplitude = .25f;
    private void Start() {
        _startPosition = transform.position;
    }

    private void Update() {
        this.transform.position = _startPosition + Vector3.up * Mathf.Cos(Time.time) * _amplitude;
    }
}