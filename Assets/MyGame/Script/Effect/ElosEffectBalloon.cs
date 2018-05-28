using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Elona.Slot {
	/// <summary>
	/// Balloon text popup for Eloslos.
	/// </summary>
	public class ElosEffectBalloon : MonoBehaviour {
		public Text textMain;
		public Image image;

		public float moveY = 200;
		public float duration = 2;
		public Ease easeMove;
		public AnimationCurve easeScale;

		public ElosEffectBalloon SetPos(float x, float y) {
			transform.localPosition = new Vector3(x, y);
			return this;
		}

		public void Play(string text, float duration = 0) {
			if (duration == 0) duration = this.duration;
			textMain.text = text.Replace("#", System.Environment.NewLine);
			transform.localScale = Vector2.zero;
			transform.DOLocalMoveY(transform.localPosition.y - moveY, duration).OnComplete(() => { Destroy(gameObject); }).SetEase(easeMove);
			transform.DOScale(1, duration - 0.1f).SetEase(easeScale);
		}
	}
}