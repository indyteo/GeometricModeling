using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
		this.mf.mesh = this.CreateNormalizedGrid(6 * 100 / 5, 50 / 5, (x, z) => {
			float R = 5, r = 1;
			float theta = 6 * 2 * Mathf.PI * x;
			float alpha = 2 * Mathf.PI * z;
			Vector3 omega = new Vector3(R * Mathf.Cos(theta), 0, R * Mathf.Sin(theta));
			Vector3 omegaP = r * Mathf.Cos(alpha) * omega.normalized + r * Mathf.Sin(alpha) * Vector3.up + Vector3.up * x * 2 * r * 6;
			return omega + omegaP;
		});
		//this.mf.mesh = this.CreatePacman(new Vector3(2, 0, 2), 20);
	}

	private Mesh CreateBox(Vector3 halfSize) {
		Mesh mesh = new Mesh();
		mesh.name = "box";

		Vector3[] vertices = new Vector3[8];
		#region Vertices
		vertices[0] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
        vertices[1] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
        vertices[2] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
        vertices[3] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
        vertices[4] = new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
        vertices[5] = new Vector3(halfSize.x, halfSize.y, -halfSize.z);
        vertices[6] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
        vertices[7] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
        #endregion
		mesh.vertices = vertices;

		int[] quads = new int[6 * 4];
		#region Quads
		int index = 0;
		// Bottom
		quads[index++] = 0;
		quads[index++] = 1;
		quads[index++] = 2;
		quads[index++] = 3;
		// Top
		quads[index++] = 4;
		quads[index++] = 7;
		quads[index++] = 6;
		quads[index++] = 5;
		// Front
		quads[index++] = 0;
		quads[index++] = 4;
		quads[index++] = 5;
		quads[index++] = 1;
		// Back
		quads[index++] = 2;
		quads[index++] = 6;
		quads[index++] = 7;
		quads[index++] = 3;
		// Left
		quads[index++] = 1;
		quads[index++] = 5;
		quads[index++] = 6;
		quads[index++] = 2;
		// Right
		quads[index++] = 3;
		quads[index++] = 7;
		quads[index++] = 4;
		quads[index] = 0;
		#endregion 
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
	}

	private Mesh CreateChips(Vector3 halfSize) {
		Mesh mesh = new Mesh();
		mesh.name = "chips";

		Vector3[] vertices = new Vector3[8];
		#region Vertices
		vertices[0] = new Vector3(-halfSize.x, -halfSize.y, -halfSize.z);
		vertices[1] = new Vector3(halfSize.x, -halfSize.y, -halfSize.z);
		vertices[2] = new Vector3(halfSize.x, -halfSize.y, halfSize.z);
		vertices[3] = new Vector3(-halfSize.x, -halfSize.y, halfSize.z);
		vertices[4] = new Vector3(-halfSize.x, halfSize.y, -halfSize.z);
		vertices[5] = new Vector3(halfSize.x, halfSize.y, -halfSize.z);
		vertices[6] = new Vector3(halfSize.x, halfSize.y, halfSize.z);
		vertices[7] = new Vector3(-halfSize.x, halfSize.y, halfSize.z);
		#endregion
		mesh.vertices = vertices;

		int[] quads = new int[3 * 4];
		#region Quads
		int index = 0;
		// Top
		quads[index++] = 4;
		quads[index++] = 7;
		quads[index++] = 6;
		quads[index++] = 5;
		// Front
		quads[index++] = 0;
		quads[index++] = 4;
		quads[index++] = 5;
		quads[index++] = 1;
		// Back
		quads[index++] = 2;
		quads[index++] = 6;
		quads[index++] = 7;
		quads[index] = 3;
		#endregion 
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
	}

	private Mesh CreateRegularPolygon(Vector3 halfSize, int nSectors) {
		Mesh mesh = new Mesh();
		mesh.name = "regularPolygon";

		Vector3[] vertices = new Vector3[2 * nSectors + 1];
		float theta = 0;
		float deltaTheta = (2 * Mathf.PI) / nSectors;
		Vector3 nextPos = new Vector3(halfSize.x, 0, 0);
		for (int i = 0; i < nSectors; i++) {
			Vector3 pos = vertices[2 * i] = nextPos;
			theta += deltaTheta;
			nextPos = new Vector3(halfSize.x * Mathf.Cos(theta), 0, halfSize.z * Mathf.Sin(theta));
			vertices[2 * i + 1] = Vector3.Lerp(pos, nextPos, 0.5f);
		}
		vertices[2 * nSectors] = Vector3.zero;
		mesh.vertices = vertices;

		int[] quads = new int[nSectors * 4];
		int center = 2 * nSectors;
		for (int i = 0; i < nSectors; i++) {
			int offset = 4 * i;
			quads[offset] = center;
			quads[offset + 1] = 2 * i + 1;
			quads[offset + 2] = 2 * i;
			quads[offset + 3] = i == 0 ? 2 * nSectors - 1 : 2 * i - 1;
		}
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
	}

	private Mesh CreatePacman(Vector3 halfSize, int nSectors, float startAngle = Mathf.PI / 3, float endAngle = 5 * Mathf.PI / 3) {
		Mesh mesh = new Mesh();
		mesh.name = "pacman";

		Vector3[] vertices = new Vector3[2 * (nSectors + 1)];
		float theta = startAngle;
		float deltaTheta = (endAngle - startAngle) / nSectors;
		Vector3 nextPos = new Vector3(halfSize.x * Mathf.Cos(theta), 0, halfSize.z * Mathf.Sin(theta));
		for (int i = 0; i < nSectors; i++) {
			Vector3 pos = vertices[2 * i] = nextPos;
			theta += deltaTheta;
			nextPos = new Vector3(halfSize.x * Mathf.Cos(theta), 0, halfSize.z * Mathf.Sin(theta));
			vertices[2 * i + 1] = Vector3.Lerp(pos, nextPos, 0.5f);
		}
		vertices[2 * nSectors] = nextPos;
		vertices[2 * nSectors + 1] = Vector3.zero;
		mesh.vertices = vertices;

		int[] quads = new int[nSectors * 4];
		int center = 2 * nSectors + 1;
		for (int i = 0; i < nSectors; i++) {
			int offset = 4 * i;
			quads[offset] = center;
			quads[offset + 1] = 2 * i + 2;
			quads[offset + 2] = 2 * i + 1;
			quads[offset + 3] = 2 * i;
		}
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
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
