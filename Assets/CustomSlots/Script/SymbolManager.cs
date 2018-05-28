using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CSFramework {
	/// <summary>
	/// A manager class to manage symbols.
	/// All the gameobjects that have Symbol component attached under the symbol manager's transform will be parsed and act as symbols
	/// </summary>
	public class SymbolManager : MonoBehaviour {
		public enum SortMode {
			None,
			ByName,
			ByPay
		}

		[Space, Header("Editor Options")] public SortMode sortMode;

		[Hide] public CustomSlot slot;
		[HideInInspector] public Symbol[] symbols;
		[HideInInspector] public float[] weights;

		private void Awake() { gameObject.SetActive(false); }

		internal void Validate(CustomSlot slot) {
			this.slot = slot;
			symbols = GetComponentsInChildren<Symbol>();
			foreach (Symbol symbol in symbols) symbol.Validate();
			SetWeights();
		}

		public void OnStartRound() { foreach (Symbol symbol in symbols) symbol.ignoreThisRound = false; }

		public Symbol GetSymbol(string symbolName) {
			foreach (Symbol symbol in symbols) if (symbol.name == symbolName) return symbol;
			return null;
		}

		// SymbolMap is a 2-dimensional list of symbols which can be applied to reels to change their symbols.

		// Called by SlotLayouter before it destorys and create new reels so that symbols on the old reel can be
		// carried over to the newly created reel.
		public SymbolMap GetSymbolMap() {
			SymbolMap map = new SymbolMap();
			for (int i = 0; i < slot.reels.Length; i++) {
				map.symbols.Add(new List<Symbol>());
				foreach (Symbol symbol in slot.reels[i].symbols) map.symbols[i].Add(symbol);
			}
			return map;
		}

		public void ApplySymbolMap(SymbolMap map = null, List<SymbolSwapper> swaps = null) {
			for (int x = 0; x < slot.reels.Length; x++) {
				Reel reel = slot.reels[x];
				for (int y = 0; y < reel.symbols.Length; y++) {
					Symbol symbol = map == null ? reel.symbols[y] : (x < map.symbols.Count && y < map.symbols[x].Count) ? map.symbols[x][y] : null;
					if (swaps != null) for (int i = 0; i < swaps.Count; i++) if (symbol == swaps[i].from) symbol = swaps[i].to;
					reel.symbols[y] = symbol ?? slot.skin.defaultSymbol;
				}
				reel.RefreshHolders();
			}

			if (swaps != null) for (int i = 0; i < swaps.Count; i++) if (swaps[i].to) swaps[i].to.ignoreThisRound = true;
		}

		public void ApplySymbolMap(SymbolMap map, SymbolSwapper swap) { ApplySymbolMap(map, new List<SymbolSwapper> {swap}); }

		/// <summary>
		/// Returns a random symbol based on the symbol's relative weight 
		/// </summary>
		public Symbol GetRandomSymbol(bool excludeMRS = false) {
			if (weights.Length > 0) {
				float r = Random.Range(0, weights[weights.Length - 1]);
				for (int i = 0; i < weights.Length; i++) {
					if (excludeMRS && symbols[i].isMRS) continue;
					if (r < weights[i]) return symbols[i];
				}
			}
			return slot.skin.defaultSymbol;
		}

		public void SetWeights() {
			weights = new float[symbols.Length];
			for (int i = 0; i < weights.Length; i++) {
				weights[i] = (i == 0 ? 0 : weights[i - 1]) + symbols[i].frequency;
			}
		}

		public void Sort() {
			Validate(slot);
			if (sortMode == SortMode.None) return;
			if (sortMode == SortMode.ByName) Array.Sort(symbols, (x, y) => String.Compare(x.name, y.name));
			if (sortMode == SortMode.ByPay) Array.Sort(symbols, (x, y) => (y.GetMaxPay() - x.GetMaxPay()));
			foreach (Symbol symbol in symbols) symbol.transform.SetAsLastSibling();
			SetWeights();
		}
	}

	[Serializable]
	public class SymbolMap {
		internal List<List<Symbol>> symbols = new List<List<Symbol>>();
	}

	[Serializable]
	public class SymbolSwapper {
		public Symbol from;
		public Symbol to;
	}
}