using System;
using UnityEngine;

namespace CSFramework {
	public class PayTableGen : MonoBehaviour {
		[Serializable]
		public class Settting {
			[TextArea]
			public string textForFreeSpin = "Free Spin";
			[TextArea]
			public string textForBonus = "Bonus";
			[TextArea]
			public string textForCustom = "Custom";
		}

		[Hide]
		public CustomSlot slot;
		public Transform targetParent;
		public Settting setting;

		public void Generate() {
			if (!targetParent) {
				Debug.Log("Target parent transform must be specified.");
				return;
			}
			slot.Validate();
			Util.DestroyChildren<PayTableItem>(targetParent);
			Symbol[] symbols = slot.symbolManager.symbols;
			foreach (Symbol symbol in symbols) {
				PayTableItem item = Util.InstantiateAt<PayTableItem>(slot.skin.paytableItem, targetParent);
				item.Init(symbol, slot, this);
			}
		}
	}
}