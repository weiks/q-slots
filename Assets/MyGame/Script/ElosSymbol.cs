using System.Collections.Generic;
using CSFramework;
using UnityEngine;

namespace Elona.Slot {
	/// <summary>
	/// An example custom Symbol class inherited from Symbol 
	/// </summary>
	public class ElosSymbol : Symbol {
		[Header("Elos")]
		public List<string> talksJP;
		public List<string> talksEN;

		public string GetRandomTalk() {
			if (Lang.isJP) return talksJP.Count == 0 ? "Mew mew?" : talksJP[Random.Range(0, talksJP.Count)];
			return talksEN.Count == 0 ? "Mew mew?" : talksEN[Random.Range(0, talksEN.Count)];
		}
	}
}