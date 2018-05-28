using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Elona.Slot {
	/// <summary>
	/// A fading text effect for Eloss
	/// </summary>
	public class ElosEffectMoney : MonoBehaviour {
		public Text textAmount;
		public Text textHint;
		public CanvasGroup cg;
		public Ease ease;

		public ElosEffectMoney SetText(int amount, string hint) {
			textAmount.text = "" + amount;
			textHint.text = hint;
			Vector2 pos = transform.localPosition;
			pos.x = pos.x + Random.Range(-50f, 50f);
			pos.y = pos.y + Random.Range(-50f, 50f);
			transform.localPosition = pos;
			int size = Mathf.Clamp(60 + amount/5, 60, 150);
			textAmount.fontSize = size;
			textHint.fontSize = (int) ((float) size*0.7f);
			return this;
		}

		public void Play(float y, float duration) {
			cg.alpha = 0;
			cg.DOFade(1, duration*0.5f).SetLoops(2, LoopType.Yoyo).SetEase(ease);
			transform.DOLocalMoveY(transform.localPosition.y + y, duration + 0.1f).OnComplete(
				() => { Destroy(gameObject); }
				);
		}
	}
}