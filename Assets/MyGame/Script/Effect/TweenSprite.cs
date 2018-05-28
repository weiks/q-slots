using System;
using CSFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Elona.Slot {
	public class TweenSprite : MonoBehaviour {
		[Serializable]
		public class TSTween {
			public float duration;
			public Ease easeFade;
			public Ease easeScale;
			public Ease easeRotation;
			public Ease easeMove;
			public float alpha;
			public float scale;
			public Vector3 rotation;
			public Vector2 anchorPos;

			public void Start(TweenSprite sprite) {
				if (easeFade != Ease.Unset) sprite.cg.DOFade(alpha, duration).SetEase(easeFade);
				if (easeScale != Ease.Unset) sprite.transform.DOScale(scale, duration).SetEase(easeScale);
				if (easeRotation != Ease.Unset) sprite.transform.DORotate(rotation, duration).SetEase(easeRotation);
				if (easeMove != Ease.Unset) (sprite.transform as RectTransform).DOAnchorPos(anchorPos, duration).SetEase(easeMove);
			}
		}

		public Transform targetParent;
		public TSTween[] tweens;
		public float delay = 0f;
		public bool zeroScaleOnAwake;
		public bool zeroAlphaOnAwake;

		[Header("References")]
		public AudioClip sound;
		public CanvasGroup cg;
		public Text text;

		private int index;
		private int originalFontSize;

		private void Awake() {
			if (zeroScaleOnAwake) transform.localScale = Vector3.zero;
			if (zeroAlphaOnAwake) cg.alpha = 0;
		}

		public void Play(float? _delay = null) {
			TweenSprite copy = Util.InstantiateAt<TweenSprite>(this, targetParent ?? transform.parent);
			copy.gameObject.SetActive(true);
			if (delay == 0f) copy._Play();
			else Util.Tween(_delay ?? delay, null, copy._Play);
		}

		public void _Play() {
			if (index < tweens.Length) {
				Util.Tween(tweens[index].duration, null, _Play);
				tweens[index].Start(this);
				index++;
			} else {
				Destroy(gameObject);
			}
		}

		public TweenSprite SetText(string s, int size = 0) {
			if (originalFontSize == 0) originalFontSize = text.fontSize;

			text.text = s;
			text.fontSize = size > 0 ? size : originalFontSize;
			return this;
		}
	}
}