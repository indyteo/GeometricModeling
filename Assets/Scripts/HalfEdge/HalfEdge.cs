namespace HalfEdge {
	public class HalfEdge {
		public int index;
		public Vertex sourceVertex;
		public Face face;
		public HalfEdge prevEdge;
		public HalfEdge nextEdge;
		public HalfEdge twinEdge;
	}
}
