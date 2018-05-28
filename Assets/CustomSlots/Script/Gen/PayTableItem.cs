using UnityEngine;
using UnityEngine.UI;

namespace CSFramework {
	/// <summary>
	/// 
	/// </summary>
	public class PayTableItem : MonoBehaviour {
		public CustomSlot slot;
		public Image imageMain;
		public Text textChain;
		public Text textPayout;
		public LayoutGroup layout;

		public void Init(Symbol symbol, CustomSlot slot, PayTableGen gen) {
			this.slot = slot;
			imageMain.sprite = symbol.sprite;
			int[] pays = symbol.pays;
			if (symbol.payType == Symbol.PayType.Normal) {
				for (int i = 0; i < slot.reels.Length; i++) {
					if (i >= pays.Length) break;
					if (pays[i] == 0) continue;
					Text chain = Util.InstantiateAt<Text>(textChain, layout.transform);
					Text payout = Util.InstantiateAt<Text>(textPayout, layout.transform);
					chain.text = "" + (i + 1);
					payout.text = "" + symbol.GetPayAmount(i + 1);
				}
			} else {
				Text chain = Util.InstantiateAt<Text>(textChain, layout.transform);
				if (symbol.payType == Symbol.PayType.FreesSpin) chain.text = gen.setting.textForFreeSpin;
				if (symbol.payType == Symbol.PayType.Bonus) chain.text = gen.setting.textForBonus;
				if (symbol.payType == Symbol.PayType.Custom) chain.text = gen.setting.textForCustom;
			}
			textChain.gameObject.SetActive(false);
			textPayout.gameObject.SetActive(false);
		}
	}
}