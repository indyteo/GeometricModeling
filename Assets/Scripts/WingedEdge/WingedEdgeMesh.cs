using System.Collections.Generic;
using UnityEngine;

namespace WingedEdge {
	public class WingedEdgeMesh {
		public List<Vertex> vertices;
		public List<WingedEdge> edges;
		public List<Face> faces;

		private MeshTopology topology;
		private int cardinality => this.topology == MeshTopology.Quads ? 4 : 3;

		// Constructeur prenant un mesh Vertex-Face en paramètre
		public WingedEdgeMesh(Mesh mesh) {
			this.vertices = new List<Vertex>();
			this.topology = mesh.GetTopology(0);
			Vector3[] vfVertices = mesh.vertices;
			for (var i = 0; i < vfVertices.Length; i++)
				this.vertices.Add(new Vertex(i, vfVertices[i]));
			int n = this.cardinality;
			this.faces = new List<Face>();
			Dictionary<ulong, WingedEdge> createdEdges = new Dictionary<ulong, WingedEdge>();
			int[] vfFaces = mesh.GetIndices(0);
			for (int i = 0; i < vfFaces.Length / n; i++) {
				Face face = new Face(i);
				this.faces.Add(face);
				int offset = i * n;
				Vertex start = this.vertices[vfFaces[offset]];
				WingedEdge previousEdge = null;
				for (int j = 1; j < n; j++) {
					Vertex end = this.vertices[vfFaces[offset + j]];
					ulong uid = WingedEdge.ComputeUID(start, end);
					WingedEdge edge;
					if (!createdEdges.TryGetValue(uid, out edge))
						edge = createdEdges[uid] = new WingedEdge(createdEdges.Count, start, end);
					edge.SetRightFace(start, face);
					if (previousEdge == null)
						face.edge = edge;
					else {
						previousEdge.SetEndCCWEdge(start, edge);
						edge.SetStartCWEdge(start, previousEdge);
					}
					previousEdge = edge;
					start = end;
				}
				Vertex end2 = this.vertices[vfFaces[offset]];
				ulong uid2 = WingedEdge.ComputeUID(start, end2);
				WingedEdge edge2;
				if (!createdEdges.TryGetValue(uid2, out edge2))
					edge2 = createdEdges[uid2] = new WingedEdge(createdEdges.Count, start, end2);
				edge2.SetRightFace(start, face);
				if (previousEdge != null) {
					previousEdge.SetEndCCWEdge(start, edge2);
					edge2.SetStartCWEdge(start, previousEdge);
				}
				edge2.SetEndCCWEdge(end2, face.edge);
				face.edge.SetStartCWEdge(end2, edge2);
			}
			this.edges = new List<WingedEdge>(createdEdges.Values);
			foreach (WingedEdge edge in this.edges) {
				if (edge.startVertex.edge == null)
					edge.startVertex.edge = edge;
				if (null == edge.leftFace) {
					if (edge.startCCWEdge == null) {
						WingedEdge matchingStart = edge;
                        while (matchingStart.GetStartCWEdge(edge.startVertex) != null)
                        	matchingStart = matchingStart.GetStartCWEdge(edge.startVertex);
                        edge.startCCWEdge = matchingStart;
                        matchingStart.SetStartCWEdge(edge.startVertex, edge);
					}
					if (edge.endCWEdge == null) {
						WingedEdge matchingEnd = edge;
                        while (matchingEnd.GetEndCCWEdge(edge.endVertex) != null)
                        	matchingEnd = matchingEnd.GetEndCCWEdge(edge.endVertex);
                        edge.endCWEdge = matchingEnd;
                        matchingEnd.SetEndCCWEdge(edge.endVertex, edge);
					}
				} else if (null == edge.rightFace) {
					if (edge.startCWEdge == null) {
						WingedEdge matchingStart = edge;
                        while (matchingStart.GetStartCCWEdge(edge.startVertex) != null)
                        	matchingStart = matchingStart.GetStartCCWEdge(edge.startVertex);
                        edge.startCWEdge = matchingStart;
                        matchingStart.SetStartCCWEdge(edge.startVertex, edge);
					}
					if (edge.endCCWEdge == null) {
						WingedEdge matchingEnd = edge;
                        while (matchingEnd.GetEndCWEdge(edge.endVertex) != null)
                        	matchingEnd = matchingEnd.GetEndCWEdge(edge.endVertex);
                        edge.endCCWEdge = matchingEnd;
                        matchingEnd.SetEndCWEdge(edge.endVertex, edge);
					}
				}
			}
		}

		public Mesh ConvertToFaceVertexMesh() {
			Mesh faceVertexMesh = new Mesh();
			Vector3[] vfVertices = new Vector3[this.vertices.Count];
			foreach (Vertex vertex in this.vertices)
				vfVertices[vertex.index] = vertex.position;
			faceVertexMesh.vertices = vfVertices;

			int[] vfFaces = new int[this.faces.Count * this.cardinality];
			int index = 0;
			foreach (Face face in this.faces) {
				WingedEdge edge = face.edge;
				Vertex start = edge.GetVertex(face == edge.leftFace);
				vfFaces[index++] = start.index;
				Vertex vertex = start, other;
				while ((other = edge.GetOtherVertex(vertex)) != start) {
					vfFaces[index++] = other.index;
					edge = edge.GetEndEdge(other, face == edge.GetRightFace(vertex));
					vertex = other;
				}
			}
			faceVertexMesh.SetIndices(vfFaces, this.topology, 0);
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
