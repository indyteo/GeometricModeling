using System.Collections.Generic;
using UnityEngine;

namespace HalfEdge {
	public class HalfEdgeMesh {
		public List<Vertex> vertices;
		public List<HalfEdge> edges;
		public List<Face> faces;

		private MeshTopology topology;
		private int cardinality => this.topology == MeshTopology.Quads ? 4 : 3;

		// Constructeur prenant un mesh Vertex-Face en paramètre
		public HalfEdgeMesh(Mesh mesh) {
			// Get mesh topology (triangles, quads, ...)
			this.topology = mesh.GetTopology(0);

			// Convert vertices
			this.vertices = new List<Vertex>();
			Vector3[] vfVertices = mesh.vertices;
			for (var i = 0; i < vfVertices.Length; i++)
					// Each vertex will keep its index and will be at the same position in the resulting list
				this.vertices.Add(new Vertex(i, vfVertices[i]));
			// At this point, we have all vertices created, but they are not linked to any edge

			// Number of vertices or edges per faces
			int n = this.cardinality;

			// Convert faces and create edges
			this.faces = new List<Face>();
			// Use a dictionary to keep track of created edges: Quick lookup to find twin edges.
			// The unique identifier (ulong) for each edges is provided by HalfEdge#ComputeUID(...)
			Dictionary<ulong, HalfEdge> createdEdges = new Dictionary<ulong, HalfEdge>();
			int[] vfFaces = mesh.GetIndices(0);
			for (int i = 0; i < vfFaces.Length / n; i++) {
				// Create each face in the same order
				Face face = new Face(i);
				this.faces.Add(face);
				// At this point, we have the face, but not linked to any edge

				// Index of the first vertex of the face
				int offset = i * n;
				Vertex start = this.vertices[vfFaces[offset + n - 1]];
				HalfEdge previousEdge = null;
				// Loop through all vertices of the face
				for (int j = 0; j < n; j++) {
					// Each iteration consider a new edge, and its end vertex will become the next one's start
					Vertex end = this.vertices[vfFaces[offset + j]];
					// We process the edge and mark it as the previous edge for our next iteration
					HalfEdge edge = new HalfEdge(createdEdges.Count, start, face);
					createdEdges[HalfEdge.ComputeUID(start, end)] = edge;

					// Link edge to vertex
					if (!start.outgoingEdge)
						start.outgoingEdge = edge;

					// Link previous
					if (previousEdge) {
						previousEdge.nextEdge = edge;
						edge.prevEdge = previousEdge;
					} else {
						// First edge of face
						face.edge = edge;
					}

					// Find twin
					HalfEdge twin;
					if (createdEdges.TryGetValue(HalfEdge.ComputeUID(end, start), out twin)) {
						edge.twinEdge = twin;
						twin.twinEdge = edge;
					}

					previousEdge = edge;
					start = end;
				}
				if (previousEdge) {
					previousEdge.nextEdge = face.edge;
					face.edge.prevEdge = previousEdge;
				}
			}
			// Save the list of edges
			this.edges = new List<HalfEdge>(createdEdges.Values);
		}

		public Mesh ConvertToFaceVertexMesh() {
			// Standard mesh object is composed of all the vertices
			// and faces (with their vertices in the clockwise order)
			Mesh faceVertexMesh = new Mesh();

			// The vertices only contains their position
			Vector3[] vfVertices = new Vector3[this.vertices.Count];
			foreach (Vertex vertex in this.vertices)
				vfVertices[vertex.index] = vertex.position;
			faceVertexMesh.vertices = vfVertices;

			// Number of vertices or edges per faces
			int n = this.cardinality;

			// The faces are just the list of vertices' index
			int[] vfFaces = new int[this.faces.Count * n];
			int index = 0;
			foreach (Face face in this.faces) {
				// We only know one edge of the face
				HalfEdge edge = face.edge;
				for (int i = 0; i < n; i++) {
					vfFaces[index++] = edge.sourceVertex.index;
					edge = edge.nextEdge;
				}
			}
			// We use the topology of the original mesh (triangles, quads, ...)
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
