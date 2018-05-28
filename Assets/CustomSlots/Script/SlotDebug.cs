using System;
using DG.Tweening;
using UnityEngine;

namespace CSFramework {
	[Serializable]
	public class SlotDebug {
		[Hide]
		public CustomSlot slot;
		[Tooltip("Ignores spin delays")]
		public bool fastSpin;
		[Tooltip("Makes all the lines always enabled")]
		public bool alwaysMaxLines;
		[Tooltip("Skips transitions and intro animations")]
		public bool skipIntro;
		public bool useDebugKeys;

		public void OnUpdate() {
			if (useDebugKeys) {
				if (Input.GetKeyDown(KeyCode.F1)) DebugHitEffect(1);
				if (Input.GetKeyDown(KeyCode.F2)) DebugHitEffect(2);
				if (Input.GetKeyDown(KeyCode.F3)) DebugHitEffect(3);
				if (Input.GetKeyDown(KeyCode.F4)) DebugHitEffect(4);
				if (Input.GetKeyDown(KeyCode.F5)) DebugHitEffect(5);
				if (Input.GetKeyDown(KeyCode.F6)) slot.AddEvent(slot.effects.IlluminateLines(2));
				if (Input.GetKeyDown(KeyCode.F11)) {
					slot.AddFreeSpin(3);
					slot.SwitchMode();
				}
				if (Input.GetKeyDown(KeyCode.F12)) {
					slot.AddBonus(3);
					slot.SwitchMode();
				}
			}
		}

		public void DebugHitEffect(int chain = 1) {
			HitInfo info = new HitInfo();
			Row row = slot.rows[slot.config.hiddenTopRows];
			info.hitSymbol = row.holders[0].symbol;
			info.hitChains = chain;
			Sequence sequence = DOTween.Sequence();
			for (int i = 0; i < Mathf.Clamp(chain, 1, slot.reels.Length); i++) sequence.Join(slot.effects.GetHitEffect(info).Play(row.holders[i], i));
			slot.AddEvent(sequence);
		}
	}
}