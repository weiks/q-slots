using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CSFramework {
	/// <summary>
	/// A base UI class you can use for your slot game.
	/// It is not necessary to inherit this class
	/// </summary>
	public class BaseSlotGameUI : MonoBehaviour {
		public CustomSlot slot;
		public Text textMoney, textRoundCost, textRound, textIncome, textBet, textFreeSpin, textBonus, debugText;
		public GameObject goFreeSpin, goBonus;
		public List<int> betList = new List<int>() {1, 10, 100};
		public int targetFrameRate = 70;
		private int betIndex = 0;

		protected virtual void Awake() {
			slot.callbacks.onActivated.AddListener(OnActivated);
			slot.callbacks.onDeactivated.AddListener(OnDeactivated);
			slot.callbacks.onRoundStart.AddListener(OnRoundStart);
			slot.callbacks.onReelStart.AddListener(OnReelStart);
			slot.callbacks.onReelStop.AddListener(OnReelStop);
			slot.callbacks.onProcessHit.AddListener(OnProcessHit);
			slot.callbacks.onRoundComplete.AddListener(OnRoundComplete);
			slot.callbacks.onSlotStateChange.AddListener(OnSlotStateChange);
			slot.callbacks.onSlotModeChange.AddListener(OnSlotModeChange);
			slot.callbacks.onLineSwitch.AddListener(OnLineSwitch);
			slot.Activate();
			Initialize();
		}

		public virtual void Initialize() {
			Application.targetFrameRate = targetFrameRate;
			RefreshMoney();
			RefreshBet();
			RefreshRoundCost();
			goFreeSpin.SetActive(false);
			goBonus.SetActive(false);
			slot.SetBet(betList[betIndex]);
		}

		/// <summary>
		/// A callback method subscribed to CS's onActivated event.
		/// It is invoked when CS is activated, precisely after an intro animation event(parallel progression).
		/// </summary>
		public virtual void OnActivated() { ShowDebugText("onActivated"); }

		/// <summary>
		/// A callback method subscribed to CS's onDeactivated event.
		/// It is invoked when CS is deactivated, after an Out-Transition is completed.
		/// </summary>
		public virtual void OnDeactivated() { ShowDebugText("onDeactivated"); }

		/// <summary>
		/// A callback method subscribed to CS's onRoundStart event.
		/// It is invoked at the start of every round.
		/// </summary>
		public virtual void OnRoundStart() {
			ShowDebugText("onRoundStart");
			RefreshRoundInfo();
		}

		/// <summary>
		/// A callback method subscribed to CS's onStartSpin event.
		/// It is invoked each time a reel starts.
		/// </summary>
		public virtual void OnReelStart(ReelInfo info) {
			ShowDebugText("onStartSpin");
			if (info.isFirstReel) {
				RefreshRoundInfo();
				RefreshMoney();
			}
		}

		/// <summary>
		/// A callback method subscribed to CS's onStopReel event.
		/// It is invoked each time a reel stops.
		/// ReelInfo contains information like which reel was stopped.
		/// </summary>
		public virtual void OnReelStop(ReelInfo info) { ShowDebugText("onStopReel"); }

		/*
		/// <summary>
		/// A callback method subscribed to CS's onRoundInterval event.
		/// It is invoked when all the reels stop spinning and before Hit Check starts.
		/// </summary>
		public virtual void OnRoundInterval() { ShowDebugText("onRoundInterval"); }
		*/

		/// <summary>
		/// A callback method subscribed to CS's onProcessHit event.
		/// It is invoked when a Hit Check is performed on a line or a scatter and if it is successful.
		/// Information like which symbol holders were the subjects of the hit, how many chains the hit made and etc etc
		/// are passed with HitInfo.
		/// 
		/// HitInfo also has a reference to a DOTween sequence(<see cref="DG.Tweening.DOTween.Sequence"/> that plays highlighting effects for the hit symbols.
		/// 
		/// </summary>
		public virtual void OnProcessHit(HitInfo info) {
			ShowDebugText("onProcessHit");
			if (info.hitSymbol.payType == Symbol.PayType.Normal) RefreshMoney();
			RefreshRoundInfo();
		}

		/// <summary>
		/// An UnityAction subscribed to CS's onRoundComplete event.
		/// It is invoked at the end of every round.
		/// </summary>
		public virtual void OnRoundComplete() { ShowDebugText("onRoundComplete"); }

		/// <summary>
		/// A callback method subscribed to CS's onSlotStateChange event.
		/// It is invoked when CS's state changes.
		/// </summary>
		public virtual void OnSlotStateChange() { ShowDebugText("onSlotStateChange"); }

		/// <summary>
		/// A callback method subscribed to CS's onSlotModeChange event.
		/// It is invoked when CS's mode changes(e.g going to free spin mode).
		/// </summary>
		public virtual void OnSlotModeChange(SlotModeInfo info) {
			ShowDebugText("onSlotModeChange");
			if (info.lastMode == slot.modes.freeSpinMode) ToggleFreeSpin(false);
			if (info.lastMode == slot.modes.bonusMode) ToggleBonus(false);
			if (slot.currentMode == slot.modes.freeSpinMode) ToggleFreeSpin(true);
			if (slot.currentMode == slot.modes.bonusMode) ToggleBonus(true);
			RefreshFreeSpin();
			RefreshBonus();
		}

		/// <summary>
		/// An UnityAction subscribed to CS's onLineSwitch event.
		/// It is invoked every time a line is turned on/off.
		/// The current state of the line can be checked from the passed LineInfo. 
		/// </summary>
		public virtual void OnLineSwitch(LineInfo info) {
			ShowDebugText("onLineSwitch");
			RefreshRoundCost();
			RefreshRoundInfo();
		}

		public virtual void ShowDebugText(string detail) {
			if (!debugText) return;
			debugText.text = "Last Callback: " + detail;
		}

		public virtual void RefreshRoundCost() { textRoundCost.text = "" + slot.gameInfo.roundCost; }
		public virtual void RefreshMoney() { textMoney.text = "" + slot.gameInfo.balance; }
		public virtual void RefreshBet() { textBet.text = "" + slot.gameInfo.bet; }

		public virtual void RefreshRoundInfo() {
			textRound.text = "Round. " + (slot.gameInfo.roundsCompleted + 1);
			textIncome.text = "( " + slot.gameInfo.roundBalance + " )";
			RefreshFreeSpin();
			RefreshBonus();
		}

		public virtual void ToggleFreeSpin(bool enable) { goFreeSpin.SetActive(enable); }
		public virtual void RefreshFreeSpin() { textFreeSpin.text = "" + slot.gameInfo.freeSpins; }
		public virtual void ToggleBonus(bool enable) { goBonus.SetActive(enable); }
		public virtual void RefreshBonus() { textBonus.text = "" + slot.gameInfo.bonuses; }

		public virtual bool SetBet(int index) {
			if (!slot.isIdle || index < 0 || index >= betList.Count) return false;
			betIndex = index;
			slot.SetBet(betList[betIndex]);
			RefreshBet();
			RefreshRoundCost();
			return true;
		}

		public virtual void RaiseBet() { SetBet(betIndex + 1); }
		public virtual void LowerBet() { SetBet(betIndex - 1); }

		public virtual void EnableNextLine() { slot.lineManager.EnableNextLine(); }
		public virtual void DisableCurrentLine() { slot.lineManager.DisableCurrentLine(); }
	}
}