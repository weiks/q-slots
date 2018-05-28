using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CSFramework {
	/// <summary>
	/// A class that displays a symbols a reel has.
	/// See <see cref="Reel"/> for more information.
	/// SymbolHolder doesn't store references to Symbol but the index of Symbol list on its parent reel.  
	/// </summary>
	public class SymbolHolder : MonoBehaviour {
		private CustomSlot slot { get { return reel.slot; } }
		public float y { get { return _rect.anchoredPosition.y; } set { _rect.anchoredPosition = new Vector2(0, value); } }
		public Symbol symbol { get { return reel.symbols[symbolIndex]; } }

		[Tooltip("Fix the holder's rotation to face front if you modified your slot's rotation.")] public bool faceFront;

		[Header("References")] public Image image;
		public Image backgroundImage;
		public Animator animator;

		[Hide] public Reel reel;
		[Hide] public int symbolIndex;
		[HideInInspector] public RectTransform _rect;
		[NonSerialized] public SymbolHolder link;

		private void Awake() {
			_rect = transform as RectTransform;
			link = this;
		}

		internal SymbolHolder OnRefreshLayout(Reel reel, int index) {
			this.reel = reel;
			_rect = transform as RectTransform;
			y = -slot.layoutRow.cellSize.y*index;
			_rect.SetParent(reel.transform, false);
			symbolIndex = slot.config.totalRows - 1 - index;
			_rect.sizeDelta = reel.slot.layout.sizeSymbol;
			if (faceFront) transform.eulerAngles = new Vector3(0, 0, 0);
			if (!animator) animator = gameObject.AddComponent<Animator>();
			return this;
		}

		public void RefreshImage() {
			if (symbol) {
				if (backgroundImage) backgroundImage.sprite = symbol.backgroundSprite;
				image.sprite = symbol.sprite;
				if (animator) animator.runtimeAnimatorController = symbol.animator ? symbol.animator.runtimeAnimatorController : null;
			}
		}

		internal void SetNextItem() {
			symbolIndex = reel.lastSymbol = reel.nextSymbol;
			RefreshImage();
			if (slot.config.reel.sortSymbolTransform == SlotConfig.HolderSortMode.AppearAsFirstSibling) transform.SetAsFirstSibling();
			else if (slot.config.reel.sortSymbolTransform == SlotConfig.HolderSortMode.AppearAsLastSibling) transform.SetAsLastSibling();
			slot.callbacks.onNewSymbolAppear.Invoke();
		}

		/// <summary>
		/// Highlights the symbol this holder has for the given duration.
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public virtual Tweener HighlightBorder(float duration) {
			Image border = Util.InstantiateAt<Image>(slot.skin.symbolBorder, image.transform);
			Color color = border.color;
			border.color = new Color(0, 0, 0, 0);
			return border.DOColor(color, duration*0.5f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo).OnComplete(() => { Destroy(border.gameObject); });
		}
	}
}