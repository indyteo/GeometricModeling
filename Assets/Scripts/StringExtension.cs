public static class StringExtension {
	public static string Repeat(this string str, int n) {
		string s = str;
		for (int i = 1; i < n; i++)
			s += str;
		return s;
	}
}
