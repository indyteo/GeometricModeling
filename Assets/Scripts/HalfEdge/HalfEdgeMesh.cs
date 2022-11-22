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
				vfVertices[vertex] = vertex;
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
					vfFaces[index++] = edge.sourceVertex;
					edge = edge.nextEdge;
				}
			}
			// We use the topology of the original mesh (triangles, quads, ...)
			faceVertexMesh.SetIndices(vfFaces, this.topology, 0);

			return faceVertexMesh;
		}

		public void SubdivideCatmullClark() {
			Vector3[] facePoints, edgePoints, vertexPoints;
			this.CatmullClarkCreateNewPoints(out facePoints, out edgePoints, out vertexPoints);
			foreach (Vertex vertex in this.vertices)
				vertex.position = vertexPoints[vertex];
			List<HalfEdge> edgesClone = new List<HalfEdge>(this.edges);
			foreach (HalfEdge edge in edgesClone)
				this.SplitEdge(edge, edgePoints[edge]);
			List<Face> facesClone = new List<Face>(this.faces);
			foreach (Face face in facesClone)
				this.SplitFace(face, facePoints[face]);
			this.topology = MeshTopology.Quads;
		}

		public void CatmullClarkCreateNewPoints(out Vector3[] facePoints, out Vector3[] edgePoints, out Vector3[] vertexPoints) {
			facePoints = new Vector3[this.faces.Count];
			foreach (Face face in this.faces) {
				int n = this.cardinality;
				Vector3 facePoint = Vector3.zero;
				HalfEdge edge = face.edge;
				for (int i = 0; i < n; i++) {
					facePoint += edge.sourceVertex;
					edge = edge.nextEdge;
				}
				facePoints[face] = facePoint / n;
			}

			Vector3[] midPoints = new Vector3[this.edges.Count];
			edgePoints = new Vector3[this.edges.Count];
			foreach (HalfEdge edge in this.edges) {
				Vector3 startPlusEnd = edge.sourceVertex.position + edge.nextEdge.sourceVertex;
				Vector3 midPoint = startPlusEnd / 2;
				midPoints[edge] = midPoint;
				edgePoints[edge] = edge.twinEdge ? (startPlusEnd + facePoints[edge.face] + facePoints[edge.twinEdge.face]) / 4 : midPoint;
			}

			vertexPoints = new Vector3[this.vertices.Count];
			foreach (Vertex vertex in this.vertices) {
				int n = 0;
				Vector3 q = Vector3.zero;
				Vector3 r = Vector3.zero;
				HalfEdge edge = vertex.outgoingEdge, previous = null;
				while (edge && (n == 0 || edge != vertex.outgoingEdge)) {
					r += midPoints[edge];
					q += facePoints[edge.face];
					previous = edge.prevEdge;
					edge = previous.twinEdge;
					n++;
				}
				if (edge) {
					vertexPoints[vertex] = (1f / n) * (q / n) + (2f / n) * (r / n) + ((n - 3f) / n) * vertex.position;
				} else {
					HalfEdge firstBorder = previous;
					HalfEdge secondBorder = vertex.outgoingEdge, next = secondBorder.twinEdge;
					while (next) {
						secondBorder = next.nextEdge;
						next = secondBorder.twinEdge;
					}
					vertexPoints[vertex] = (midPoints[firstBorder] + midPoints[secondBorder] + vertex) / 3;
				}
			}
		}

		public void SplitEdge(HalfEdge edge, Vector3 splittingPoint) {
			HalfEdge next = edge.nextEdge;
			Vertex startPoint = edge.sourceVertex;
			Vertex endPoint = next.sourceVertex;
			HalfEdge twin = edge.twinEdge;
			Vertex newPoint = null;
			bool twinAlreadySplit = false;
			if (twin) {
				Vertex twinStart = twin.sourceVertex;
				Vertex twinEnd = twin.nextEdge.sourceVertex;
				twinAlreadySplit = twinStart != endPoint || twinEnd != startPoint;
				if (twinAlreadySplit)
					newPoint = twinEnd;
			}

			if (!newPoint) {
				newPoint = new Vertex(this.vertices.Count, splittingPoint);
				this.vertices.Add(newPoint);
			}

			HalfEdge newEdge = new HalfEdge(this.edges.Count, newPoint, edge.face);
			this.edges.Add(newEdge);
			if (!newPoint.outgoingEdge)
				newPoint.outgoingEdge = newEdge;

			next.prevEdge = newEdge;
			newEdge.nextEdge = next;
			newEdge.prevEdge = edge;
			edge.nextEdge = newEdge;

			if (twinAlreadySplit) {
				HalfEdge secondTwin = twin.nextEdge;
				edge.twinEdge = secondTwin;
				secondTwin.twinEdge = edge;
				newEdge.twinEdge = twin;
				twin.twinEdge = newEdge;
			}
		}

		public void SplitFace(Face face, Vector3 splittingPoint) {
			Vertex newPoint = new Vertex(this.vertices.Count, splittingPoint);
			this.vertices.Add(newPoint);
			Face f = face;
			HalfEdge edge = face.edge.nextEdge, firstCreated = null, lastCreated = null;
			do {
				if (!f) {
					f = new Face(this.faces.Count);
					this.faces.Add(f);
					f.edge = lastCreated;
					lastCreated!.face = f; // Only null in first loop (but we do not enter the upper condition then)
				}
				edge.prevEdge.face = edge.prevEdge.prevEdge.face = f;
				Vertex edgePoint = edge.sourceVertex;
				HalfEdge outgoing = new HalfEdge(this.edges.Count, edgePoint, f);
				this.edges.Add(outgoing);
				outgoing.prevEdge = edge.prevEdge;
				edge.prevEdge.nextEdge = outgoing;
				if (lastCreated) {
					outgoing.nextEdge = lastCreated;
					lastCreated.prevEdge = outgoing;
				}
				HalfEdge incoming = lastCreated = new HalfEdge(this.edges.Count, newPoint, null);
				this.edges.Add(incoming);
				outgoing.twinEdge = incoming;
				incoming.twinEdge = outgoing;
				incoming.nextEdge = edge;
				edge.prevEdge = incoming;
				if (!firstCreated)
					firstCreated = outgoing;
				f = null;
				edge = edge.nextEdge.nextEdge;
			} while (edge.prevEdge != face.edge);
			lastCreated.face = face;
			firstCreated.nextEdge = lastCreated;
			lastCreated.prevEdge = firstCreated;
			newPoint.outgoingEdge = lastCreated;
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
