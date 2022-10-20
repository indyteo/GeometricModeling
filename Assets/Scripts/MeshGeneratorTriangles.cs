using System;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorTriangles : MonoBehaviour {
	private new Transform transform;
	private MeshFilter mf;

	void Awake() {
		this.transform = this.GetComponent<Transform>();
		this.mf = this.GetComponent<MeshFilter>();
		this.mf.mesh = this.CreateGrid(6, 4, new Vector3(3, 0, 2));
	}

	private Mesh CreateTriangle() {
		Mesh mesh = new Mesh();
		mesh.name = "triangle";

		Vector3[] vertices = new Vector3[3];
		vertices[0] = Vector3.right;
		vertices[1] = Vector3.up;
		vertices[2] = Vector3.forward;
		mesh.vertices = vertices;

		int[] triangles = new int[1 * 3];
		triangles[0] = 0;
		triangles[1] = 1;
		triangles[2] = 2;
		mesh.triangles = triangles;

		return mesh;
	}

	private Mesh CreateQuad(Vector3 size) {
		Mesh mesh = new Mesh();
		mesh.name = "quad";

		Vector3[] vertices = new Vector3[4];
		vertices[0] = new Vector3(-size.x, 0, -size.z);
		vertices[1] = new Vector3(-size.x, 0, size.z);
		vertices[2] = new Vector3(size.x, 0, size.z);
		vertices[3] = new Vector3(size.x, 0, -size.z);
		mesh.vertices = vertices;

		int[] triangles = new int[2 * 3];
		triangles[0] = 0;
		triangles[1] = 1;
		triangles[2] = 2;

		triangles[3] = 0;
		triangles[4] = 2;
		triangles[5] = 3;
		mesh.triangles = triangles;

		return mesh;
	}

	private Mesh CreateStrip(int n, Vector3 size) {
		Mesh mesh = new Mesh();
		mesh.name = "strip";

		Vector3[] vertices = new Vector3[(n + 1) * 2];
		float x = -size.x;
		float dx = 2 * size.x / n;
		for (int i = 0; i <= n; i++) {
			vertices[i] = new Vector3(x, 0, size.z);
			vertices[n + 1 + i] = new Vector3(x, 0, -size.z);
			x += dx;
		}
		mesh.vertices = vertices;

		int[] triangles = new int[n * 2 * 3];
		for (int i = 0; i < n; i++) {
			int offset = i * 6;
			triangles[offset] = i;
			triangles[offset + 1] = i + 1;
			triangles[offset + 2] = n + 1 + i;

			triangles[offset + 3] = i + 1;
			triangles[offset + 4] = n + 1 + i + 1;
			triangles[offset + 5] = n + 1 + i;
		}
		mesh.triangles = triangles;

		return mesh;
	}

	private Mesh CreateGrid(int nX, int nZ, Vector3 size) {
		Mesh mesh = new Mesh();
		mesh.name = "grid";

		Func<int,int,int> index = (i, j) => i * (nZ + 1) + j;

		Vector3[] vertices = new Vector3[(nX + 1) * (nZ + 1)];
		float dx = 2 * size.x / nX;
		float dz = 2 * size.z / nZ;
		float x = -size.x;
		for (int i = 0; i <= nX; i++) {
			float z = -size.z;
			for (int j = 0; j <= nZ; j++) {
				vertices[index(i, j)] = new Vector3(x, 0, z);
				z += dz;
			}
			x += dx;
		}
		mesh.vertices = vertices;

		int[] triangles = new int[nX * nZ * 2 * 3];
		for (int i = 0; i < nX; i++) {
			for (int j = 0; j < nZ; j++) {
				int offset = (i * nZ + j) * 6;
				triangles[offset] = index(i, j);
				triangles[offset + 1] = index(i, j + 1);
				triangles[offset + 2] = index(i + 1, j + 1);

				triangles[offset + 3] = index(i, j);
				triangles[offset + 4] = index(i + 1, j + 1);
				triangles[offset + 5] = index(i + 1, j);
			}
		}
		mesh.triangles = triangles;

		return mesh;
	}

	private void OnDrawGizmos() {
		if (!this.mf || !this.mf.mesh)
			return;

		Mesh mesh = this.mf.mesh;
		Vector3[] vertices = mesh.vertices;
		int[] triangles = mesh.GetIndices(0);

		GUIStyle style = new GUIStyle();
		style.fontSize = 16;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.red;

		for (int i = 0; i < vertices.Length; i++)
			Handles.Label(this.transform.TransformPoint(vertices[i]), i.ToString(), style);

		Gizmos.color = Color.black;
		style.normal.textColor = Color.blue;
		for (int i = 0; i < triangles.Length; i += 3) {
			int idx1 = triangles[i], idx2 = triangles[i + 1], idx3 = triangles[i + 2];
			Vector3 pt1 = this.transform.TransformPoint(vertices[idx1]);
			Vector3 pt2 = this.transform.TransformPoint(vertices[idx2]);
			Vector3 pt3 = this.transform.TransformPoint(vertices[idx3]);
			Gizmos.DrawLine(pt1, pt2);
			Gizmos.DrawLine(pt2, pt3);
			Gizmos.DrawLine(pt3, pt1);
			string str = string.Format("{0}: {1},{2},{3}", i / 3, idx1, idx2, idx3);
			Handles.Label((pt1 + pt2 + pt3) / 3, str, style);
		}
	}
}
