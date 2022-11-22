using JetBrains.Annotations;

namespace HalfEdge {
	public class HalfEdge {
		public readonly int index;
		public Vertex sourceVertex;
		public Face face;
		public HalfEdge prevEdge;
		public HalfEdge nextEdge;
		public HalfEdge twinEdge;

		public ulong UID => ComputeUID(this.sourceVertex, this.nextEdge.sourceVertex);

		public HalfEdge(int index) {
			this.index = index;
		}

		public HalfEdge(int index, Vertex vertex, Face face) : this(index) {
			this.sourceVertex = vertex;
			this.face = face;
		}

		public override string ToString() => "E" + this.index.ToString();

		public static ulong ComputeUID(Vertex start, Vertex end) => ComputeUID(start.index, end.index);

		private static ulong ComputeUID(int start, int end) => ((ulong) start) << 32 | (uint) end;

		[ContractAnnotation("null => false; notnull => true")]
		public static implicit operator bool(HalfEdge obj) => !ReferenceEquals(null, obj);

		public static implicit operator int(HalfEdge obj) => obj.index;
	}
}
