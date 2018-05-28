using CSFramework;
using UnityEngine;

namespace Elona.Slot {
	public class ElosBonusGame : MonoBehaviour {
		public CustomSlot bonusSlot;
		public Elos elos;
		private SlotEvent slotEvent;

		private Elos.Assets assets { get { return elos.assets; } }

		private void Awake() { bonusSlot.callbacks.onNewSymbolAppear.AddListener(OnNewSymbolAppear); }

		public void Activate(SlotEvent slotEvent) {
			assets.tweens.tsBonus.Play();
			assets.audioBonus.Play();
			this.slotEvent = slotEvent;
			gameObject.SetActive(true);
			bonusSlot.Activate();
		}

		public void OnActivated() { bonusSlot.AddEvent(bonusSlot.StartRound); }
		public void OnNewSymbolAppear() { assets.audioSpinBonus.Play(); }

		public void OnProcessHit(HitInfo info) {
			assets.tweens.tsWinSpecial.SetText(info.hitSymbol.name + "!", 150).Play();
			elos.slot.gameInfo.AddBalance(info.payout*elos.slot.gameInfo.roundCost);
		}

		public void OnRoundComplete() { bonusSlot.Deactivate(); }

		public void OnDeactivated() {
			gameObject.SetActive(false);
			slotEvent.Deactivate();
		}
	}
}