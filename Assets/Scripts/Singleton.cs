using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component {
    private static T _instance;

    public static T Instance {
        get {
            if (_instance == null) {
                var objs = FindObjectsOfType(typeof(T)) as T[];
                if (objs is { Length: 1 })
                    _instance = objs[0];
                if (objs is { Length: > 1 }) {
                    Debug.LogError("There is more than one " + typeof(T).Name + " in the scene.");
                }

                if (_instance == null) {
                    GameObject obj = new GameObject {
                        name = $"_{typeof(T).Name}"
                    };
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }
}