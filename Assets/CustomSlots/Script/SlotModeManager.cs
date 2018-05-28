using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CSFramework {
	[Serializable]
	public class SlotModeManager {
		[Hide] public CustomSlot slot;
		internal SlotMode current;
		public SlotMode defaultMode;
		public SlotMode freeSpinMode;
		public SlotMode bonusMode;
		internal SymbolMap cleanMap;

		public void Initialize() {
			cleanMap = slot.symbolManager.GetSymbolMap();
			current = defaultMode;
		}

		public void SwitchMode(SlotMode mode = null) {
			if (mode == null) {
				mode = current;
				if (mode == bonusMode) {
					if (slot.gameInfo.bonuses == 0) mode = freeSpinMode;
				}
				if (mode == freeSpinMode) {
					if (slot.gameInfo.bonuses > 0) mode = bonusMode;
					else if (slot.gameInfo.freeSpins == 0) mode = defaultMode;
				}
				if (mode == defaultMode) {
					if (slot.gameInfo.bonuses > 0) mode = bonusMode;
					else if (slot.gameInfo.freeSpins > 0) mode = freeSpinMode;
				}
			}

			if (mode != current) {
				slot.symbolManager.ApplySymbolMap(cleanMap, mode.symbolSwaps);
				SlotModeInfo info = new SlotModeInfo(current);
				current = mode;
				slot.callbacks.onSlotModeChange.Invoke(info);
			}
		}
	}

	[Serializable]
	public class SlotMode {
		[Serializable]
		public enum SpinMode {
			AutoStop,
			ManualStopAll,
			ManualStopOne,
			ManualStartOne
		}

		[Tooltip("AutoStop: Reels will automatically stop after certain time\nManualStopAll:Reels will stop when a button is pressed or told to do so in script\nManualStopOne: Each reel will stop one by one when a button is pressed\nManualStartOne:When a button is pressed, single reel will start spinning and the last reel will stop spinning")] public SpinMode spinMode;

		[Tooltip("When enabled, a round will automatically start without interaction(e.g without pressing play button) in this mode")] public bool forcePlay = false;
		public int costPerLine = 5;

		[Tooltip("Only used in AutoStop mode. Represents the duration before AutoStop commances")] public float autoStopTime = 1f;

		[Space, Tooltip("Represents the delay for each reel to start spinning")] public float spinStartDelay = 0.3f;

		[Tooltip("Represents the delay for each reel to stop spinning")] public float spinStopDelay = 0.6f;

		[Space, Tooltip("Represents the max spinning speed of reels")] public float reelMaxSpeed = 2000f;

		[Tooltip("Represents how long it takes for a reel to accelerate to its max speed")] public float reelAccelerateTime = 0.5f;

		[Tooltip("An easing that is applied when a reel starts to spin")] public Ease reelAccelerateEase = Ease.InSine;

		[Tooltip("Represents how long it takes for a reel to completely stop")] public float reelStopTime = 0.5f;

		[Tooltip("Number of rows symbols move until a reel completely stops. Cannot exceed the number of rows your slot has.")] public int reelStopDistance = 1;

		[Tooltip("An easing that is applied when a reel stops")] public Ease reelStopEase = Ease.OutBack;

		[Tooltip("Specify symbols to swap upon entering this mode. [From] symbol will be replaced with [To] symbol.")] public List<SymbolSwapper> symbolSwaps;
	}
}