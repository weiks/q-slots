using System;
using System.Collections.Generic;
using CSFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Elona.Slot {
	[ExecuteInEditMode]
	public class ElosShop : MonoBehaviour {
		[Serializable]
		public class ShopItemData {
			public string id;
			public string name_EN;
			public string name_JP;
			public string hint_EN;
			public string hint_JP;
			public int cost;
			public Sprite sprite;
			internal ElosShopItem actor;
		}

		[Serializable]
		public class JureTalk {
			public string welcome_EN;
			public string welcome_JP;
			public string bought_EN;
			public string bought_JP;
		}

		public Elos elos;
		public GridLayoutGroup layoutItems;
		public Transform window;
		public Image background;
		public Transform mascot;

		public ElosShopItem itemMold;
		public int minCheatBalance;
		public JureTalk talks;
		public List<ShopItemData> items;
		private int balance { get { return elos.slot.gameInfo.balance; } }
		private Elos.Assets assets { get { return elos.assets; } }

		private void Awake() {
			if (!Application.isPlaying) {
				items.Sort((x, y) => { return y.cost - x.cost; });
				return;
			}
		}

		public void Activate() {
			Refresh();
			gameObject.SetActive(true);
			assets.audioClick.Play();
			background.color = new Color(0, 0, 0, 0);
			background.DOFade(0.5f, 1f);
			mascot.transform.localPosition = window.transform.localPosition = new Vector3(0, 1200, 0);
			window.DOLocalMoveY(0, 1f).SetEase(Ease.OutBounce);
			Util.Tween(0.35f, null, () => {
				assets.audioImpact.Play();
				Camera.main.DOShakePosition(1.2f, 6, 12);
			});
			mascot.DOLocalMoveY(0, 2f).SetEase(Ease.OutBounce);
			Talk(talks.welcome_EN, talks.welcome_JP, 1f);
		}

		public void Talk(string en, string jp, float delay = 0.1f) { Util.Tween(delay, null, () => { Util.InstantiateAt<ElosEffectBalloon>(elos.assets.effectBalloon, mascot).SetPos(0, 100).Play(Lang.Get(en, jp), 4f); }); }

		public void Deactivate() {
			if (DOTween.IsTweening(background)) return;
			assets.audioClick.Play();
			background.DOFade(0f, 0.8f).OnComplete(_Deactivate);
			window.DOLocalMoveY(-1200, 0.8f).SetEase(Ease.InBack);
		}

		public void _Deactivate() { gameObject.SetActive(false); }

		public void Buy(ShopItemData item) {
			if (balance >= item.cost) {
				Talk(talks.bought_EN, talks.bought_JP);
				elos.slot.gameInfo.AddBalance(-item.cost);
				Refresh();
			} else {
				assets.audioBeep.Play();
			}
		}

		public void Refresh() {
			if (itemMold.gameObject.activeSelf) {
				foreach (ShopItemData item in items) Util.InstantiateAt<ElosShopItem>(itemMold, layoutItems.transform).ApplyData(item, this);
				itemMold.gameObject.SetActive(false);
			}
			foreach (ShopItemData item in items) {
				if (item.cost < 0) item.actor.gameObject.SetActive(balance < minCheatBalance);
				item.actor.Refresh(balance >= item.cost);
			}
		}
	}
}