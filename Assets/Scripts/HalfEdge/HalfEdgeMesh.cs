using System.Collections.Generic;
using UnityEngine;

namespace HalfEdge {
	public class HalfEdgeMesh {
		public List<Vertex> vertices;
		public List<HalfEdge> edges;
		public List<Face> faces;

		// Constructeur prenant un mesh Vertex-Face en paramètre
		public HalfEdgeMesh(Mesh mesh) {
			// TODO Magic happens
		}

		public Mesh ConvertToFaceVertexMesh() {
			Mesh faceVertexMesh = new Mesh();
			// TODO Magic happens
			return faceVertexMesh;
		}

		public string ConvertToCSVFormat(string separator = "\t") {
			string str = "";
			// TODO Magic happens
			return str;
		}

		public void DrawGizmos(bool drawVertices, bool drawEdges, bool drawFaces) {
			// TODO Magic happens
		}
	}
}
