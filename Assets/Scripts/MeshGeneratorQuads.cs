using System;
using System.Collections.Generic;
using HalfEdge;
using UnityEditor;
using UnityEngine;
using WingedEdge;
using Unity.Mathematics;
using UnityEngine.Rendering;
using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using int3 = Unity.Mathematics.int3;

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneratorQuads : MonoBehaviour {
	[SerializeField] private bool drawGizmos;
	[SerializeField] private bool drawEdges;
	[SerializeField] private bool drawVertices;
	[SerializeField] private bool drawFaces;

	private new Transform transform;
	private MeshFilter mf;

	//[SerializeField] private AnimationCurve profile;

	void Awake() {
		this.transform = this.GetComponent<Transform>();
		this.mf = this.GetComponent<MeshFilter>();
		//this.mf.mesh = this.CreateNormalizedGrid(20, 50, (x, z) => {
		//	float theta = x * 2 * Mathf.PI;
		//	float y = z * 5;
		//	float rho = this.profile.Evaluate(z) * 2;
		//	return new Vector3(rho * Mathf.Cos(theta), y, rho * Mathf.Sin(theta));
		//});
		//this.mf.mesh = this.CreateNormalizedGrid(100, 50, (x, z) => {
		//	float theta = (1 - x) * 2 * Mathf.PI;
		//	float phi = z * Mathf.PI;
		//	float rho = 5 + 0.25f * Mathf.Cos(x * 2 * Mathf.PI * 8) * Mathf.Sin(z * 2 * Mathf.PI * 4);
		//	return new Vector3(rho * Mathf.Cos(theta) * Mathf.Sin(phi), rho * Mathf.Cos(phi), rho * Mathf.Sin(theta) * Mathf.Sin(phi));
		//});
		//Mesh mesh = this.CreateNormalizedGrid(4 * 10, 4, (x, z) => {
		//	float R = 5, r = 1;
		//	float theta = 4 * 2 * Mathf.PI * x;
		//	float alpha = 2 * Mathf.PI * z;
		//	Vector3 omega = new Vector3(R * Mathf.Cos(theta), 0, R * Mathf.Sin(theta));
		//	Vector3 omegaP = r * Mathf.Cos(alpha) * omega.normalized + r * Mathf.Sin(alpha) * Vector3.up + Vector3.up * x * 2 * r * 5;
		//	return omega + omegaP;
		//});
		// Mesh mesh = MeshUtils.CreateBox(new Vector3(3, 3, 3));
		// Mesh mesh = MeshUtils.CreateChips(new Vector3(3, 3, 3));
		// Mesh mesh = MeshUtils.CreateRegularPolygon(new Vector3(2, 0, 2), 20);
		// Mesh mesh = MeshUtils.CreatePacMan(new Vector3(2, 0, 2), 20);
		Mesh mesh = MeshUtils.CreateTree();
		// this.mf.mesh = mesh;
		// this.mf.mesh = new WingedEdgeMesh(mesh).ConvertToFaceVertexMesh();
		HalfEdgeMesh halfEdgeMesh = new HalfEdgeMesh(mesh);
		halfEdgeMesh.SubdivideCatmullClark();
		halfEdgeMesh.SubdivideCatmullClark();
		halfEdgeMesh.SubdivideCatmullClark();
		this.mf.mesh = halfEdgeMesh.ConvertToFaceVertexMesh();

		//int3 nCells = int3(2, 3, 1);
		//int3 nSegmentsPerCell = int3(50, 50, 1);
		//float3 cellSize = float3(2, .5f, 1);
		//this.mf.mesh = this.CreateNormalizedGridSIMD(nCells * nSegmentsPerCell, (k) => {
		//	float3 cellOriginPos = floor(k * nCells).xzy * cellSize;
		//	k = frac(k * nCells);
		//	return cellOriginPos + cellSize * float3(k.x, smoothstep(0.2f - 0.05f, 0.2f + 0.05f, k.x * k.y), k.y);
		//});
	}

	#region Unused

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

		int[] quads = new int[n * 4];
		for (int i = 0; i < n; i++) {
			int offset = i * 4;
			quads[offset] = i;
			quads[offset + 1] = i + 1;
			quads[offset + 2] = n + 1 + i + 1;
			quads[offset + 3] = n + 1 + i;
		}
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
	}

	private Mesh CreateGrid(int nX, int nZ, Vector3 size) {
		Mesh mesh = new Mesh();
		mesh.name = "grid";

		Func<int, int, int> index = (i, j) => i * (nZ + 1) + j;

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

		int[] quads = new int[nX * nZ * 4];
		for (int i = 0; i < nX; i++) {
			for (int j = 0; j < nZ; j++) {
				int offset = (i * nZ + j) * 4;
				quads[offset] = index(i, j);
				quads[offset + 1] = index(i, j + 1);
				quads[offset + 2] = index(i + 1, j + 1);
				quads[offset + 3] = index(i + 1, j);
			}
		}
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
	}

	private delegate Vector3 ComputePosDelegate(float x, float z);

	private Mesh CreateNormalizedGrid(int nX, int nZ, ComputePosDelegate computePos = null) {
		Mesh mesh = new Mesh();
		mesh.name = "normalizedGrid";

		Func<int, int, int> index = (i, j) => i * (nZ + 1) + j;

		Vector3[] vertices = new Vector3[(nX + 1) * (nZ + 1)];
		for (int i = 0; i <= nX; i++) {
			float x = i / (float) nX;
			for (int j = 0; j <= nZ; j++) {
				float z = j / (float) nZ;
				vertices[index(i, j)] = computePos == null ? new Vector3(x, 0, z) : computePos(x, z);
			}
		}
		mesh.vertices = vertices;

		int[] quads = new int[nX * nZ * 4];
		for (int i = 0; i < nX; i++) {
			for (int j = 0; j < nZ; j++) {
				int offset = (i * nZ + j) * 4;
				quads[offset] = index(i, j);
				quads[offset + 1] = index(i, j + 1);
				quads[offset + 2] = index(i + 1, j + 1);
				quads[offset + 3] = index(i + 1, j);
			}
		}
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
	}

	private delegate float3 ComputePosDelegateSIMD(float3 k);

	private Mesh CreateNormalizedGridSIMD(int3 n, ComputePosDelegateSIMD computePos = null) {
		Mesh mesh = new Mesh();
		mesh.name = "normalizedGrid";
		mesh.indexFormat = IndexFormat.UInt32;

		Func<int, int, int> index = (i, j) => i * (n.y + 1) + j;

		Vector3[] vertices = new Vector3[(n.x + 1) * (n.y + 1)];
		for (int i = 0; i <= n.x; i++) {
			for (int j = 0; j <= n.y; j++) {
				float3 k = float3(i, j, 0) / n;
				vertices[index(i, j)] = computePos == null ? k : computePos(k);
			}
		}
		mesh.vertices = vertices;

		int[] quads = new int[n.x * n.y * 4];
		// TODO Optimize
		for (int i = 0; i < n.x; i++) {
			for (int j = 0; j < n.y; j++) {
				int offset = (i * n.y + j) * 4;
				quads[offset] = index(i, j);
				quads[offset + 1] = index(i, j + 1);
				quads[offset + 2] = index(i + 1, j + 1);
				quads[offset + 3] = index(i + 1, j);
			}
		}
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
	}

	#endregion

	#region Tools

	private string ConvertToCSV(string separator = "\t") {
		if (!this.mf || !this.mf.mesh)
			return "";

		Mesh mesh = this.mf.mesh;
		Vector3[] vertices = mesh.vertices;
		int[] quads = mesh.GetIndices(0);
		
		List<string> lines = new List<string>();
		for (int i = 0; i < vertices.Length; i++) {
			Vector3 pos = vertices[i];
			lines.Add(i.ToString() + separator + pos.x.ToString("N03") + " " + pos.y.ToString("N03") + " " + pos.z.ToString("N03") + separator + separator);
		}

		for (int i = vertices.Length; i < quads.Length / 4; i++)
			lines.Add(separator + separator + separator);

		for (int i = 0; i < quads.Length / 4; i++)
			lines[i] += i.ToString() + separator + quads[4 * i].ToString() + "," + quads[4 * i + 1].ToString() + "," + quads[4 * i + 2].ToString() + "," + quads[4 * i + 3].ToString();

		return "Vertices" + separator + separator + separator + "Faces\nIndex" + separator + "Position" + separator + separator + "Index" + separator + "Indices des vertices\n" + string.Join("\n", lines);
	}

	private void OnDrawGizmos() {
		if (!this.drawGizmos || !this.mf || !this.mf.mesh)
			return;

		Mesh mesh = this.mf.mesh;
		Vector3[] vertices = mesh.vertices;
		int[] quads = mesh.GetIndices(0);

		GUIStyle style = new GUIStyle();
		style.fontSize = 16;
		style.alignment = TextAnchor.MiddleCenter;
		style.normal.textColor = Color.red;

		if (this.drawVertices)
			for (int i = 0; i < vertices.Length; i++)
				Handles.Label(this.transform.TransformPoint(vertices[i]), i.ToString(), style);

		Gizmos.color = Color.black;
		style.normal.textColor = Color.blue;
		if (this.drawEdges || this.drawFaces) {
			for (int i = 0; i < quads.Length; i += 4) {
				int idx1 = quads[i], idx2 = quads[i + 1], idx3 = quads[i + 2], idx4 = quads[i + 3];
				Vector3 pt1 = this.transform.TransformPoint(vertices[idx1]);
				Vector3 pt2 = this.transform.TransformPoint(vertices[idx2]);
				Vector3 pt3 = this.transform.TransformPoint(vertices[idx3]);
				Vector3 pt4 = this.transform.TransformPoint(vertices[idx4]);
				if (this.drawEdges) {
					Gizmos.DrawLine(pt1, pt2);
					Gizmos.DrawLine(pt2, pt3);
					Gizmos.DrawLine(pt3, pt4);
					Gizmos.DrawLine(pt4, pt1);
				}
				if (this.drawFaces) {
					string str = string.Format("{0}: {1},{2},{3},{4}", i / 4, idx1, idx2, idx3, idx4);
					Handles.Label((pt1 + pt2 + pt3 + pt4) / 4, str, style);
				}
			}
		}
	}

	private void OnGUI() {
		if (GUI.Button(new Rect(10, 10, 75, 25), "Copy CSV"))
			GUIUtility.systemCopyBuffer = this.ConvertToCSV();
	}

	#endregion
}
