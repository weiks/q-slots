using UnityEngine;
using UnityEngine.UI;

namespace Elona.Slot {
	public class ElosShopItem : MonoBehaviour {
		public Text textName;
		public Text textHint;
		public Text textCost;
		public Image icon;
		public ElosShop.ShopItemData data;
		public Button button;
		public Image bg;
		public Color colorValid;
		public Color colorInvalid;

		public void ApplyData(ElosShop.ShopItemData data, ElosShop shop) {
			this.data = data;
			data.actor = this;
		}

		public void Refresh(bool canAfford) {
            textName.text = data.name;
            textHint.text = data.hint;
		    textCost.text = "" + data.cost;
			icon.sprite = data.sprite;
			textCost.color = canAfford ? colorValid : colorInvalid;
		}
	}
}