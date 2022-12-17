using UnityEditor;
using UnityEngine;

public class SelectOnStart : MonoBehaviour {
	private void Start() {
		Selection.activeGameObject = this.gameObject;
	}
}
