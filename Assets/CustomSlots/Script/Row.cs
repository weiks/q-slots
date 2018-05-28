using UnityEngine;

namespace CSFramework {
	/// <summary>
	/// A class that holds information of a row.
	/// </summary>
	public class Row : MonoBehaviour {
		[Hide]
		public CustomSlot slot;
		[Hide]
		public SymbolHolder[] holders;
		[Hide]
		public int index;
		public bool isHiddenRow { get { return !slot.config.isRowValid(index); } }

		internal void OnRefreshLayout(CustomSlot slot, int index) {
			this.slot = slot;
			this.index = index;
			transform.SetParent(slot.layoutRow.transform, false);
			holders = new SymbolHolder[slot.config.reelLength];
		}
	}
}