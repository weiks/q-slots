using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace CSFramework {
	[Serializable]
	public class SlotEffectManager {
		[Hide] public CustomSlot slot;
		[Tooltip("When enabled, symbols on a win line will be brought to top screen so that the images will not be masked.")]
		public bool changeParentOfHitSymbol = true;

		[Space] public EffectTransition transitionIn;
		public EffectTransition transitionOut;

		[Space] public IntroEffect introAnimation;

		[Space] public LineSwitchEffect lineSwitchEffect;
		public LineHitEffect lineHitEffect;

		[Space] public List<SymbolHitEffect> symbolHitEffects;

		public SymbolHitEffect GetHitEffect(HitInfo info) {
			symbolHitEffects.Sort();
			foreach (SymbolHitEffect effect in symbolHitEffects) if (effect.ifSymbolMatches == info.hitSymbol && info.hitChains >= effect.ifChainsAtLeast) return effect;
			foreach (SymbolHitEffect effect in symbolHitEffects) if (!effect.ifSymbolMatches && info.hitChains >= effect.ifChainsAtLeast) return effect;
			if (symbolHitEffects.Count > 0) return symbolHitEffects[0];
			return null;
		}

		[Serializable]
		public class SymbolHitEffect : IComparable {
			public string id = "HitEffect";
			[Header("Conditions")] public int ifChainsAtLeast;
			public Symbol ifSymbolMatches;

			[Header("Animation")] public float duration;
			public float delay = 0.1f;

			public Vector3 scale = new Vector3(1.5f, 1.5f);
			public Ease scaleEase = Ease.OutBack;
			public Vector3 rotation = new Vector3(0, 360);
			public Ease rotationEase = Ease.InOutBack;

			public int CompareTo(object obj) { return (obj as SymbolHitEffect).ifChainsAtLeast - this.ifChainsAtLeast; }

			public Sequence Play(SymbolHolder holder, int orderInLine) {
				CustomSlot slot = holder.reel.slot;
				SlotEffectManager manager = slot.effects;

				Sequence sequence = Util.Sequence();

				if (manager.changeParentOfHitSymbol) {
					Transform oldParent = holder.transform.parent;

					//	int index = holder.transform.GetSiblingIndex();
					sequence.OnStart(() => { holder.transform.SetParent(holder.reel.slot.transform); }).OnComplete(() => {
						holder.transform.SetParent(oldParent);

						//			holder.transform.SetSiblingIndex(index);
					});
				}

				if (scale != Vector3.one) sequence.Join(holder.image.transform.DOScale(scale, duration*0.35f).SetLoops(2, LoopType.Yoyo).SetEase(scaleEase)).SetDelay(delay*orderInLine);
				if (rotation != Vector3.zero) sequence.Join(holder.image.transform.DORotate(rotation, duration*0.6f, RotateMode.FastBeyond360).SetEase(rotationEase)).SetDelay(delay*orderInLine);

				sequence.Join(holder.HighlightBorder(duration));
				return sequence;
			}
		}

		[Serializable]
		public class LineHitEffect {
			public bool displayAsPlayback;
			public bool drawLine = true;
			public Vector3 iconScale = new Vector3(1.5f, 1.5f);
			public Ease iconScaleEase = Ease.OutBack;
			public Ease iconColorEase = Ease.Flash;
			public Ease pathColorEase = Ease.InFlash;

			public Sequence Play(Line line, float duration) {
				Sequence sequence = Util.Sequence();
				if (iconScale != Vector3.one) sequence.Join(line.transform.DOScale(iconScale, duration*0.5f).SetLoops(2, LoopType.Yoyo).SetEase(iconScaleEase));
				sequence.Join(line.image.DOColor(line.gradientColor, duration*0.5f).SetEase(iconColorEase).SetLoops(2, LoopType.Yoyo));
				if (drawLine) sequence.Join(line.DrawPath(duration, pathColorEase));
				return sequence;
			}
		}

		[Serializable]
		public class LineSwitchEffect {
			[Tooltip("An easing used to display/highliting a line")] public float iconDuration = 1f;
			public Ease iconColorEase = Ease.Flash;
			public float pathDuration = 2f;
			public Ease pathColorEase = Ease.InOutFlash;

			public void Play(Line line) {
				line.HighlightLineIcon(iconDuration, iconColorEase);
				line.DrawPath(pathDuration, pathColorEase);
			}
		}

		[Serializable]
		public class EffectTransition {
			public bool disableTransition = true;
			public Transform target;

			[Space] public float duration = 2f;
			public float wait = 1f;
			[Tooltip("Starting alpha of the transform when In-Transition, ending alpha when Out-Transition.")] public float fade = 0f;
			public Ease fadeEase;
			[Tooltip("Starting scale of the transform when In-Transition, ending scale when Out-Transition.")] public Vector3 scale = Vector3.zero;
			public Ease scaleEase = Ease.OutBack;
			[Tooltip("Rotates the target transform by specified amount, set 0(all 3) to disable rotation.")] public Vector3 rotation = Vector3.zero;
			public Ease rotationEase;

			public Sequence Play(CustomSlot slot, bool isOut, Action onComplete) {
				if (slot.debug.skipIntro || disableTransition) {
					onComplete.Invoke();
					return null;
				}
				if (!target) target = slot.transform;
				CanvasGroup cg = target.gameObject.GetComponent<CanvasGroup>();
				if (!cg) cg = target.gameObject.AddComponent<CanvasGroup>();
				cg.alpha = isOut ? 1 : fade;
				target.localScale = isOut ? Vector3.one : scale;
				Sequence sequence = Util.Sequence(duration + wait, () => {
					cg.DOFade(isOut ? fade : 1f, duration).SetDelay(wait).SetEase(fadeEase);
					target.DOScale(isOut ? scale : Vector3.one, duration).SetDelay(wait).SetEase(scaleEase);
					if (rotation != Vector3.zero) target.DORotate(rotation, duration, RotateMode.FastBeyond360).SetDelay(wait).SetEase(rotationEase);
				}).OnComplete(onComplete.Invoke);
				return sequence;
			}
		}

		[Serializable]
		public class IntroEffect {
			public enum IntroType {
				None,
				Demo
			}

			public bool playOnlyOnce = false;
			public IntroType introType;

			[Space] public float wait = 1f;
			public float duration = 2f;
			public float delay = 0.025f;
			public Vector3 rotation = new Vector3(0, 360, 0);
			public Ease rotationEase = Ease.OutQuart;
			public int vibrato = 2;
			public float elasticity = 5;
			private bool isPlayed = false;

			public Sequence Play(CustomSlot slot) {
				if (introType == IntroType.None) return null;
				if (isPlayed && playOnlyOnce) return null;
				isPlayed = true;

				List<SymbolHolder> holders = slot.GetVisibleHolders();
				Sequence sequence = DOTween.Sequence();

				switch (introType) {
					case IntroType.Demo:
						sequence.Join(slot.effects.IlluminateLines(duration + holders.Count*delay));
						foreach (SymbolHolder holder in holders) {
							sequence.Join(holder.transform.DORotate(rotation, duration*0.5f, RotateMode.FastBeyond360).SetEase(rotationEase).SetLoops(2, LoopType.Yoyo).SetDelay(delay));
							sequence.Join(holder.transform.DOPunchScale(new Vector2(1f, 1f), duration, vibrato, elasticity).SetEase(rotationEase).SetDelay(delay));
						}
						break;
				}

				slot.AddEvent(wait);
				slot.AddEvent(sequence);
				return sequence;
			}
		}

		public Sequence IlluminateLines(float duration = 2f, int count = 10) {
			Sequence sequence = Util.Sequence(duration, () => { slot.lineManager.EffectSwitchAllLines(true, true); }, () => { slot.lineManager.EffectSwitchAllLines(null, true); });
			foreach (Line line in slot.lineManager.lines) sequence.Join(line.image.DOColor(line.gradientColor, duration/count/2).SetLoops(count*2, LoopType.Yoyo));
			return sequence;
		}
	}
}