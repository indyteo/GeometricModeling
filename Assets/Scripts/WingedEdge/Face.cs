namespace WingedEdge {
	public class Face {
		public readonly int index;
		public WingedEdge edge;

		public Face(int index) {
			this.index = index;
		}

		public override string ToString() => "F" + this.index.ToString();

		protected bool Equals(Face other) => !ReferenceEquals(null, other) && this.index == other.index;

		public override bool Equals(object obj) {
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return this.Equals((Face) obj);
		}

		public override int GetHashCode() => this.index;

		public static bool operator==(Face a, Face b) => ReferenceEquals(null, a) ? ReferenceEquals(null, b) : a.Equals(b);

		public static bool operator!=(Face a, Face b) => !(a == b);

		public static implicit operator bool(Face obj) => !ReferenceEquals(null, obj);
	}
}
