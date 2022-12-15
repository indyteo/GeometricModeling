using System;
using UnityEngine;

public static class MeshUtils {
	public static Mesh CreateBox(Vector3 halfSize) {
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

	public static Mesh CreateChips(Vector3 halfSize) {
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

	public static Mesh CreateRegularPolygon(Vector3 halfSize, int nSectors) {
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

	public static Mesh CreatePacMan(Vector3 halfSize, int nSectors, float startAngle = Mathf.PI / 3, float endAngle = 5 * Mathf.PI / 3) {
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

	public static Mesh CreateTree(float baseSize = 3, float stemRadius = 1, float stemHeight = 10, float leavesSize = 5) { 
		Mesh mesh = new Mesh();
		mesh.name = "custom";

		Vector3[] vertices = new Vector3[20];
		#region Vertices
		float baseOffset = baseSize + stemRadius;
		float leavesHeight = stemHeight + 2 * leavesSize;
		vertices[0] = new Vector3(-baseOffset, 0, -baseOffset);
		vertices[1] = new Vector3(baseOffset, 0, -baseOffset);
		vertices[2] = new Vector3(baseOffset, 0, baseOffset);
		vertices[3] = new Vector3(-baseOffset, 0, baseOffset);

		vertices[4] = new Vector3(-stemRadius, 0, -stemRadius);
		vertices[5] = new Vector3(stemRadius, 0, -stemRadius);
		vertices[6] = new Vector3(stemRadius, 0, stemRadius);
		vertices[7] = new Vector3(-stemRadius, 0, stemRadius);

		vertices[8] = new Vector3(-stemRadius, stemHeight, -stemRadius);
		vertices[9] = new Vector3(stemRadius, stemHeight, -stemRadius);
		vertices[10] = new Vector3(stemRadius, stemHeight, stemRadius);
		vertices[11] = new Vector3(-stemRadius, stemHeight, stemRadius);

		vertices[12] = new Vector3(-leavesSize, stemHeight, -leavesSize);
		vertices[13] = new Vector3(leavesSize, stemHeight, -leavesSize);
		vertices[14] = new Vector3(leavesSize, stemHeight, leavesSize);
		vertices[15] = new Vector3(-leavesSize, stemHeight, leavesSize);

		vertices[16] = new Vector3(-leavesSize, leavesHeight, -leavesSize);
		vertices[17] = new Vector3(leavesSize, leavesHeight, -leavesSize);
		vertices[18] = new Vector3(leavesSize, leavesHeight, leavesSize);
		vertices[19] = new Vector3(-leavesSize, leavesHeight, leavesSize);
		#endregion
		mesh.vertices = vertices;

		int[] quads = new int[17 * 4];
		#region Quads
		int index = 0;
		// Base
		quads[index++] = 0;
		quads[index++] = 4;
		quads[index++] = 5;
		quads[index++] = 1;

		quads[index++] = 1;
		quads[index++] = 5;
		quads[index++] = 6;
		quads[index++] = 2;

		quads[index++] = 2;
		quads[index++] = 6;
		quads[index++] = 7;
		quads[index++] = 3;

		quads[index++] = 3;
		quads[index++] = 7;
		quads[index++] = 4;
		quads[index++] = 0;

		// Stem
		quads[index++] = 4;
		quads[index++] = 8;
		quads[index++] = 9;
		quads[index++] = 5;

		quads[index++] = 5;
		quads[index++] = 9;
		quads[index++] = 10;
		quads[index++] = 6;

		quads[index++] = 6;
		quads[index++] = 10;
		quads[index++] = 11;
		quads[index++] = 7;

		quads[index++] = 7;
		quads[index++] = 11;
		quads[index++] = 8;
		quads[index++] = 4;

		// Bottom Leaves
		quads[index++] = 8;
		quads[index++] = 12;
		quads[index++] = 13;
		quads[index++] = 9;

		quads[index++] = 9;
		quads[index++] = 13;
		quads[index++] = 14;
		quads[index++] = 10;

		quads[index++] = 10;
		quads[index++] = 14;
		quads[index++] = 15;
		quads[index++] = 11;

		quads[index++] = 11;
		quads[index++] = 15;
		quads[index++] = 12;
		quads[index++] = 8;

		// Side Leaves
		quads[index++] = 12;
		quads[index++] = 16;
		quads[index++] = 17;
		quads[index++] = 13;

		quads[index++] = 13;
		quads[index++] = 17;
		quads[index++] = 18;
		quads[index++] = 14;

		quads[index++] = 14;
		quads[index++] = 18;
		quads[index++] = 19;
		quads[index++] = 15;

		quads[index++] = 15;
		quads[index++] = 19;
		quads[index++] = 16;
		quads[index++] = 12;

		// Top
		quads[index++] = 16;
		quads[index++] = 19;
		quads[index++] = 18;
		quads[index] = 17;
		#endregion 
		mesh.SetIndices(quads, MeshTopology.Quads, 0);

		return mesh;
	}

	public static Mesh CreateHelicalTube(int helicalPrecision = 10, int tubePrecision = 10, float helicalRadius = 5, float tubeRadius = 1, int n = 5, float extension = 1) {
		return CreateNormalizedGrid(helicalPrecision * n, tubePrecision, (x, z) => {
			float theta = n * 2 * Mathf.PI * x;
			float alpha = 2 * Mathf.PI * z;
			Vector3 omega = new Vector3(helicalRadius * Mathf.Cos(theta), 0, helicalRadius * Mathf.Sin(theta));
			Vector3 omegaP = tubeRadius * (Mathf.Cos(alpha) * omega.normalized + Mathf.Sin(alpha) * Vector3.up + Vector3.up * x * 2 * n * extension);
			return omega + omegaP;
		});
	}

	public static Mesh CreateStrip(int n, Vector3 size) {
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

	public static Mesh CreateGrid(int nX, int nZ, Vector3 size) {
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

	public delegate Vector3 ComputePosDelegate(float x, float z);

	public static Mesh CreateNormalizedGrid(int nX, int nZ, ComputePosDelegate computePos = null) {
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
}

public enum ObjectType {
	Box, Chips, RegularPolygon, PacMan, Tree, HelicalTube
}
