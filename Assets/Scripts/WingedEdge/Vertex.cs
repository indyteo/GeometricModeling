using UnityEngine;

namespace WingedEdge {
	public class Vertex {
		public readonly int index;
		public Vector3 position;
		public WingedEdge edge;

		public Vertex(int index) {
			this.index = index;
		}

		public Vertex(int index, Vector3 position) : this(index) {
			this.position = position;
		}

		public override string ToString() => "V" + this.index.ToString();

		protected bool Equals(Vertex other) => !ReferenceEquals(null, other) && this.index == other.index;

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return this.Equals((Vertex) obj);
		}

		public override int GetHashCode() => this.index;

		public static bool operator==(Vertex a, Vertex b) => ReferenceEquals(null, a) ? ReferenceEquals(null, b) : a.Equals(b);

		public static bool operator!=(Vertex a, Vertex b) => !(a == b);

		public static implicit operator bool(Vertex obj) => !ReferenceEquals(null, obj);
	}
}
