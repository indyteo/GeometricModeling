using System;
using System.Collections.Generic;
using UnityEditor;
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
			// Standard mesh object is composed of all the vertices
			// and faces (with their vertices in the clockwise order)
			Mesh faceVertexMesh = new Mesh();

			// The vertices only contains their position
			Vector3[] vfVertices = new Vector3[this.vertices.Count];
			foreach (Vertex vertex in this.vertices)
				vfVertices[vertex.index] = vertex.position;
			faceVertexMesh.vertices = vfVertices;

			// The faces are just the list of vertices' index
			int[] vfFaces = new int[this.faces.Count * this.cardinality];
			int index = 0;
			foreach (Face face in this.faces) {
				// We only know one edge of the face
				WingedEdge edge = face.edge;
				// Remember: The edges are processed in the clockwise motion.
				// So we should be the right face of the edge. If not, this
				// means the edge is flipped, and we sould start at the end
				Vertex first = edge.GetVertex(face == edge.leftFace);
				// We can record the index of the first vertex of the face
				vfFaces[index++] = first.index;

				Vertex start = first, end;
				// We use the other vertex of the edge as our end, and we loop
				// until we reach the first (complete rotation around the face).
				// Notice the "edge" variable is modified inside the loop
				while ((end = edge.GetOtherVertex(start)) != first) {
					// We record the next vertex of our face
					vfFaces[index++] = end.index;
					// And get the next edge by rotating around the end vertex,
					// in counterclockwise motion
					edge = edge.GetEndCCWEdge(end);
					start = end;
				}
			}
			// We use the topology of the original mesh (triangles, quads, ...)
			faceVertexMesh.SetIndices(vfFaces, this.topology, 0);

			// And we're done!
			return faceVertexMesh;
		}

		public string ConvertToCSVFormat(string separator = "\t") {
			List<string> lines = new List<string>();
			// We first dump all the edges
			foreach (WingedEdge edge in this.edges)
				lines.Add(string.Join(separator, edge, edge.startVertex, edge.endVertex, edge.leftFace, edge.rightFace, edge.startCWEdge, edge.startCCWEdge, edge.endCWEdge, edge.endCCWEdge) + separator.Repeat(2));

			// Then all the vertices
			for (int i = 0; i < this.vertices.Count; i++) {
				Vertex vertex = this.vertices[i];
				lines[i] += string.Join(separator, vertex, vertex.position, vertex.edge) + separator.Repeat(2);
			}

			// And finally all the faces
			for (int i = 0; i < this.faces.Count; i++) {
				Face face = this.faces[i];
				lines[i] += string.Join(separator, face, face.edge);
			}

			// And we add a header before the data
			return "Edges" + separator.Repeat(10) + "Vertex" + separator.Repeat(4) + "Faces" + separator.Repeat(2) + "\n"
			       + string.Join(separator, "Index", "Start vertex", "End vertex", "Left face", "Right face", "Start CW", "Start CCW", "End CW", "End CCW", "", "Index", "Position", "Outgoing edge", "", "Index", "Edge") + "\n"
			       + string.Join("\n", lines);
		}

		public void DrawGizmos(Func<Vector3, Vector3> transform, bool drawVertices, bool drawEdgesLines, bool drawEdgesLabels, bool drawFaces) {
			GUIStyle style = new GUIStyle();
			style.fontSize = 16;
			style.alignment = TextAnchor.MiddleCenter;
			style.normal.textColor = Color.red;

			// Each vertex is represented by its index (in red)
			if (drawVertices)
				foreach (Vertex vertex in this.vertices)
					Handles.Label(transform(vertex.position), vertex.index.ToString(), style);

			Gizmos.color = Color.black;
			style.normal.textColor = Color.blue;

			// The edges are composed of lines and labels (their index, in blue)
			if (drawEdgesLines || drawEdgesLabels) {
				foreach (WingedEdge edge in this.edges) {
					if (drawEdgesLines)
						Gizmos.DrawLine(transform(edge.startVertex), transform(edge.endVertex));
					if (drawEdgesLabels)
						Handles.Label(Vector3.Lerp(transform(edge.startVertex), transform(edge.endVertex), 0.5f), edge.index.ToString(), style);
				}
			}

			style.normal.textColor = Color.green;

			// And each face has its index and the list of indexes of its vertices (in green)
			if (drawFaces) {
				foreach (Face face in this.faces) {
					WingedEdge edge = face.edge;
					Vertex first = edge.GetVertex(face == edge.leftFace);
					string indexes = first.index.ToString();
                    Vector3 positions = transform(first);
                    int n = 1;
                    Vertex start = first, end;
					while ((end = edge.GetOtherVertex(start)) != first) {
						indexes += "," + end.index;
						positions += transform(end);
						n++;
						edge = edge.GetEndCCWEdge(end);
						start = end;
					}
					Handles.Label(positions / n, face.index.ToString() + ": " + indexes, style);
				}
			}
		}
	}
}
