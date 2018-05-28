using UnityEngine;
using UnityEngine.UI;

namespace CSFramework {
	/// <summary>
	/// A manager class to manage skins/templates for CustomSlot. A skin acts as an "original"(or mold) and is referenced 
	/// when CustomSlot needs to instantiate its type. When there're missing symbols in a reel, they will be
	/// replaced with <see cref="defaultSymbol"/>. 
	/// </summary>
	public class SkinManager : MonoBehaviour {
		[Hide] public CustomSlot slot;
		public Symbol defaultSymbol;
		public Line line;
		public SymbolHolder symbolHolder;
		public Row row;
		public Reel reel;
		public SGLineRenderer lineTrail;
		public Image symbolBorder;
		public PayTableItem paytableItem;

		private void Awake() { gameObject.SetActive(false); }
	}
}