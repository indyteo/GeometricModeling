using JetBrains.Annotations;

namespace WingedEdge {
	public class WingedEdge {
		public readonly int index;
		public Vertex startVertex;
		public Vertex endVertex;
		public Face leftFace;
		public Face rightFace;
		public WingedEdge startCWEdge;
		public WingedEdge startCCWEdge;
		public WingedEdge endCWEdge;
		public WingedEdge endCCWEdge;

		public WingedEdge(int index) {
			this.index = index;
		}

		public WingedEdge(int index, Vertex startVertex, Vertex endVertex) : this(index) {
			this.startVertex = startVertex;
			this.endVertex = endVertex;
		}

		public ulong UID => ComputeUID(this.startVertex, this.endVertex);

		public Vertex GetOtherVertex(Vertex vertex) => vertex == this.startVertex ? this.endVertex : this.startVertex;

		public Vertex GetVertex(bool end = false) => end ? this.endVertex : this.startVertex;

		public Face GetOtherFace(Face face) => face == this.leftFace ? this.rightFace : this.leftFace;

		public WingedEdge GetStartCWEdge(Vertex expectedStart) => expectedStart == this.startVertex ? this.startCWEdge : this.endCWEdge;

		public WingedEdge GetStartCCWEdge(Vertex expectedStart) => expectedStart == this.startVertex ? this.startCCWEdge : this.endCCWEdge;

		public WingedEdge GetEndCWEdge(Vertex expectedEnd) => expectedEnd == this.endVertex ? this.endCWEdge : this.startCWEdge;

		public WingedEdge GetEndCCWEdge(Vertex expectedEnd) => expectedEnd == this.endVertex ? this.endCCWEdge : this.startCCWEdge;

		public void SetStartCWEdge(Vertex expectedStart, WingedEdge edge) {
			if (expectedStart == this.startVertex)
				this.startCWEdge = edge;
			else
				this.endCWEdge = edge;
		}

		public void SetStartCCWEdge(Vertex expectedStart, WingedEdge edge) {
			if (expectedStart == this.startVertex)
				this.startCCWEdge = edge;
			else
				this.endCCWEdge = edge;
		}

		public void SetEndCWEdge(Vertex expectedEnd, WingedEdge edge) {
			if (expectedEnd == this.endVertex)
				this.endCWEdge = edge;
			else
				this.startCWEdge = edge;
		}

		public void SetEndCCWEdge(Vertex expectedEnd, WingedEdge edge) {
			if (expectedEnd == this.endVertex)
				this.endCCWEdge = edge;
			else
				this.startCCWEdge = edge;
		}

		public Face GetLeftFace(Vertex expectedStart) => this.GetFace(expectedStart);

		public Face GetRightFace(Vertex expectedStart) => this.GetFace(expectedStart, true);

		public Face GetFace(Vertex expectedStart, bool right = false) => expectedStart == this.startVertex ^ right ? this.leftFace : this.rightFace;

		public void SetRightFace(Vertex expectedStart, Face face) {
			if (expectedStart == this.startVertex)
				this.rightFace = face;
			else
				this.leftFace = face;
		}

		public override string ToString() => "E" + this.index.ToString();

		public static ulong ComputeUID(Vertex a, Vertex b) => ComputeUID(a.index, b.index);

		public static ulong ComputeUID(int a, int b) => a > b ? _ComputeUID(a, b) : _ComputeUID(b, a);

		private static ulong _ComputeUID(int max, int min) => ((ulong) max) << 32 | (uint) min;

		[ContractAnnotation("null => false; notnull => true")]
		public static implicit operator bool(WingedEdge obj) => !ReferenceEquals(null, obj);

		public static implicit operator int(WingedEdge obj) => obj.index;
	}
}
