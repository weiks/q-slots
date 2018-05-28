namespace Elona {
	public class Lang {
		public enum ID {
			EN,
			JP
		}

		public static bool isJP { get { return current == ID.JP; } }
		public static ID current = ID.EN;

		public static string Get(string en, string jp) { return current == ID.EN ? en : jp; }
		public static void ToggleLanguage() { current = current == ID.EN ? ID.JP : ID.EN; }
	}
}