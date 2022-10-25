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
			// Use a dictionary to keep track of created edges: Quick lookup to avoid duplicate edges.
			// The unique identifier (ulong) for each edges is provided by WingedEdge#ComputeUID(...)
			Dictionary<ulong, WingedEdge> createdEdges = new Dictionary<ulong, WingedEdge>();
			int[] vfFaces = mesh.GetIndices(0);
			for (int i = 0; i < vfFaces.Length / n; i++) {
				// Create each face in the same order
				Face face = new Face(i);
				this.faces.Add(face);
				// At this point, we have the face, but not linked to any edge

				// Index of the first vertex of the face
				int offset = i * n;
				Vertex first = this.vertices[vfFaces[offset]], start = first;
				WingedEdge previousEdge = null;
				// Loop through all remaining vertices of the face
				for (int j = 1; j < n; j++) {
					// Each iteration consider a new edge, and its end vertex will become the next one's start
					Vertex end = this.vertices[vfFaces[offset + j]];
					// We process the edge and mark it as the previous edge for our next iteration
					previousEdge = ProcessEdge(face, start, end, previousEdge, createdEdges);
					start = end;
				}
				// Then, finish the face with the last edge, closing the polygon.
				// The "start" variable contains the end of the last edge, and the
				// "first" variable the initial vertex of our face
				WingedEdge edge = ProcessEdge(face, start, first, previousEdge, createdEdges);
				// Finally save the relationship between the latest edge and the first created.
				// See comment in the ProcessEdge method for more information.
				// The previous here is the latest processed edge (the "edge" variable)
				// and supposed next one is the first created edge (saved in the Face#edge field)
				edge.SetEndCCWEdge(first, face.edge);
				face.edge.SetStartCWEdge(first, edge);
			}

			// Save the list of edges
			this.edges = new List<WingedEdge>(createdEdges.Values);
			// At this point, we have all edges created, with their start and end vertices,
			// right and left faces, and all the inner start/end (counter)clockwise edges
			// relationships. Their only missing data are the relationships between the
			// external neighbouring edges.

			// Postprocess every edges to fill the missing fields
			foreach (WingedEdge edge in this.edges) {
				// If needed, save the edge as the edge of its start/end vertex
				if (!edge.startVertex.edge)
					edge.startVertex.edge = edge;
				if (!edge.endVertex.edge)
					edge.endVertex.edge = edge;
				// At this point, our vertices data structures are complete

				// If the edge has no left face, then we need to manually find
				// its start counterclockwise and end clockwise edges
				if (!edge.leftFace) {
					// We check if they are not already defined because when we
					// always register bidirectional relationships, so they may
					// already have been defined earlier when processing another
					// edge in this loop
					if (!edge.startCCWEdge) {
						// We'll try to find the edge that match this one when
						// rotation around start, in counterclockwise motion
						WingedEdge matchingStartCCW = edge;
						// We rotate around the start vertex in clockwise motion
						// until there is no edge, so we reach the end of the
						// mesh using the opposite motion (because all inner
						// relationships are defined)
                        while (matchingStartCCW.GetStartCWEdge(edge.startVertex))
	                        matchingStartCCW = matchingStartCCW.GetStartCWEdge(edge.startVertex);
                        // Finally, we can register the outer relationship between
                        // the current edge and the matching edge found.
                        // We are the start clockwise edge of our start
                        // counterclockwise edge
                        edge.startCCWEdge = matchingStartCCW;
                        matchingStartCCW.SetStartCWEdge(edge.startVertex, edge);
					}
					// The process here is the same as above, but with different edges
					if (!edge.endCWEdge) {
						WingedEdge matchingEndCW = edge;
                        while (matchingEndCW.GetEndCCWEdge(edge.endVertex))
                        	matchingEndCW = matchingEndCW.GetEndCCWEdge(edge.endVertex);
                        edge.endCWEdge = matchingEndCW;
                        matchingEndCW.SetEndCCWEdge(edge.endVertex, edge);
					}
				}

				// If the edge has no right face, then we need to manually find
				// its start clockwise and end counterclockwise edges
				if (!edge.rightFace) {
					// The process here is the same as above, but with different edges
					if (!edge.startCWEdge) {
						WingedEdge matchingStartCW = edge;
                        while (matchingStartCW.GetStartCCWEdge(edge.startVertex))
                        	matchingStartCW = matchingStartCW.GetStartCCWEdge(edge.startVertex);
                        edge.startCWEdge = matchingStartCW;
                        matchingStartCW.SetStartCCWEdge(edge.startVertex, edge);
					}
					// The process here is the same as above, but with different edges
					if (!edge.endCCWEdge) {
						WingedEdge matchingEndCCW = edge;
                        while (matchingEndCCW.GetEndCWEdge(edge.endVertex))
                        	matchingEndCCW = matchingEndCCW.GetEndCWEdge(edge.endVertex);
                        edge.endCCWEdge = matchingEndCCW;
                        matchingEndCCW.SetEndCWEdge(edge.endVertex, edge);
					}
				}
			}
			// At this point, we're done, all our data structures are complete!
		}

		private static WingedEdge ProcessEdge(Face face, Vertex start, Vertex end, WingedEdge previousEdge, Dictionary<ulong, WingedEdge> createdEdges) {
			// Get the unique identifier of the edge to process
			ulong uid = WingedEdge.ComputeUID(start, end);
			// Retrieve or create it
			WingedEdge edge;
			if (!createdEdges.TryGetValue(uid, out edge))
				edge = createdEdges[uid] = new WingedEdge(createdEdges.Count, start, end);

			// The edges are processed in the clockwise motion for each face.
			// So the face we are rotating around is our right face (from
			// our start of the edge)
			edge.SetRightFace(start, face);
			if (!previousEdge)
				// In case we have no previous edge, we save it in the face
				face.edge = edge;
				// At this point, our faces data structures are complete
			else {
				// Otherwise, we record relationship with the previous edge.
				// The relationship is bidirectional and we use the common
				// vertex as our referential (our start is the previous end).
				// Remember: The edges are processed in the clockwise motion.
				// So we are the end counterclockwise edge of the previous,
				// and the previous is our start clockwise edge
				previousEdge.SetEndCCWEdge(start, edge);
				edge.SetStartCWEdge(start, previousEdge);
			}
			// Finally return the edge so it can be used as the next "previous edge"
			return edge;
		}

		public Mesh ConvertToFaceVertexMesh() {
			// TODO Add comments to explain algorithm
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
