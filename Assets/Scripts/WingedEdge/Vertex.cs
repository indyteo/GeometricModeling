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

		public override string ToString() {
			return "V" + this.index.ToString();
		}

		protected bool Equals(Vertex other) {
			return !ReferenceEquals(null, other) && this.index == other.index;
		}

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return this.Equals((Vertex) obj);
		}

		public override int GetHashCode() {
			return this.index;
		}

		public static bool operator==(Vertex a, Vertex b) {
			return ReferenceEquals(null, a) ? ReferenceEquals(null, b) : a.Equals(b);
		}

		public static bool operator!=(Vertex a, Vertex b) {
			return !(a == b);
		}

		public static implicit operator bool(Vertex obj) {
			return !ReferenceEquals(null, obj);
		}
	}
}
