using System;
using CSFramework;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Elona.Slot {
	public class ElosUI : BaseSlotGameUI {
		[Serializable]
		public class Colors {
			public Gradient freeSpinBG;
			public Gradient freeSpinBGSlot;
		}

		[Header("Elos")] public Elos elos;
		public ElosShop shop;
		public Colors colors;
		public Text textLevel, textExp;

		public Image background, highlightFreeSpin, backgroundSlot;
		public Button buttonPlay;
		public Slider sliderExp;
		public GameObject payTable;
		public GameObject[] BGs;
		public AudioMixer mixer;

		public float volumeMaster { set { mixer.SetFloat("VolumeMaster", Mathf.Lerp(-80, 0, value)); } }
		public float volumeBGM { set { mixer.SetFloat("VolumeBGM", Mathf.Lerp(-80, 0, value)); } }
		public float volumeSE { set { mixer.SetFloat("VolumeSE", Mathf.Lerp(-80, 0, value)); } }

		private int indexBG;

		private Tweener _moneyTween;
		private int lastBalance;

		private Elos.Assets assets { get { return elos.assets; } }
		private Elos.ElonaSlotData data { get { return elos.data; } }
		private void OnEnable() { assets.bgm.Play(); }

		public override void Initialize() {
			elos.Load();
			base.Initialize();
			RefreshExp();
			slot.callbacks.onAddBalance.AddListener(OnAddBalance);
			lastBalance = slot.gameInfo.balance;
			shop.gameObject.SetActive(false);
			elos.bonusGame.gameObject.SetActive(false);
		}

		public void RefreshExp() {
			textLevel.text = "" + data.lv;
			textExp.text = "" + data.exp + " / " + data.expNext;
			sliderExp.value = (float) data.exp/data.expNext;
		}

		public override void OnActivated() {
			base.OnActivated();
			if (!slot.debug.skipIntro) {
				assets.audioDemo.Play();
				assets.tweens.tsIntro1.Play();
				assets.tweens.tsIntro2.Play();
			}
		}

		public override void OnRoundStart() {
			base.OnRoundStart();
			if (slot.currentMode != slot.modes.freeSpinMode) buttonPlay.interactable = true;
		}

		public override void OnReelStart(ReelInfo info) {
			base.OnReelStart(info);
			if (info.isFirstReel) {
				assets.audioSpin.Play();
				assets.audioSpinLoop.Play();
			}
		}

		public override void OnReelStop(ReelInfo info) {
			base.OnReelStop(info);
			assets.audioReelStop.Play();
			if (info.isFirstReel && slot.currentMode.spinMode == SlotMode.SpinMode.ManualStopAll) buttonPlay.interactable = false;
			if (info.isLastReel) {
				buttonPlay.interactable = false;
				assets.audioSpinLoop.Stop();
			}
		}

		public override void OnRoundComplete() {
			base.OnRoundComplete();
			if (slot.gameInfo.roundHits == 0) assets.audioLose.Play();
		}

		public override void EnableNextLine() {
			if (!slot.lineManager.EnableNextLine()) assets.audioBeep.Play();
			else assets.audioBet.Play();
		}

		public override void DisableCurrentLine() {
			if (!slot.lineManager.DisableCurrentLine()) assets.audioBeep.Play();
			else {
				assets.audioBet.pitch = 0.6f;
				assets.audioBet.Play();
			}
		}

		public override bool SetBet(int index) {
			if (!base.SetBet(index)) {
				assets.audioBeep.Play();
				return false;
			}
			assets.audioBet.Play();
			return true;
		}

		public void TogglePayTable() {
			assets.audioClick.Play();
			payTable.SetActive(!payTable.activeSelf);
		}

		public void ToggleShop() {
			if (slot.isIdle) shop.Activate();
			else assets.audioBeep.Play();
		}

		public override void ToggleFreeSpin(bool enable) {
			base.ToggleFreeSpin(enable);
			if (enable) {
				assets.particleFreeSpin.Play();
				backgroundSlot.DOGradientColor(colors.freeSpinBGSlot, 0.6f);
				background.DOGradientColor(colors.freeSpinBG, 0.6f);
			} else {
				assets.particleFreeSpin.Stop();
				backgroundSlot.DOColor(Color.white, 2f);
				background.DOColor(Color.white, 2f);
			}
		}

		public override void OnProcessHit(HitInfo info) {
			base.OnProcessHit(info);
			SymbolHolder randomHolder = info.hitHolders[Random.Range(0, info.hitHolders.Count)];
			ElosSymbol symbol = randomHolder.symbol as ElosSymbol;
			Util.InstantiateAt<ElosEffectBalloon>(assets.effectBalloon, slot.transform.parent, randomHolder.transform).Play(symbol.GetRandomTalk());
			foreach (SymbolHolder holder in info.hitHolders) info.sequence.Join(ShowWinAnimation(info, holder));
		}

		// Winning particle and audio effect when a line is a "hit"
		public Tweener ShowWinAnimation(HitInfo info, SymbolHolder holder) {
			return Util.Tween(() => {
				int coins = (info.hitChains - 2)*(info.hitChains - 2)*(info.hitChains - 2) + 1;

				if (info.hitSymbol.payType == Symbol.PayType.Normal) {
					assets.particlePrize.transform.position = holder.transform.position;
					Util.Emit(assets.particlePrize, coins);
					if (info.hitChains <= 3) assets.audioWinSmall.Play();
					else if (info.hitChains == 4) assets.audioWinMedium.Play();
					else assets.audioWinBig.Play();
					if (info.hitChains >= 4) assets.tweens.tsWin.SetText(info.hitChains + "-IN-A-ROW!", info.hitChains*40).Play();
				} else {
					assets.audioWinSpecial.Play();
					if (info.hitSymbol.payType == Symbol.PayType.FreesSpin) assets.tweens.tsWinSpecial.SetText("Free Spin!").Play();
					else assets.tweens.tsWinSpecial.SetText("BONUS!").Play();
				}
			});
		}

		private int _lastBalance;

		private void Update() {
			if (_lastBalance != lastBalance) {
				textMoney.text = "" + lastBalance;
				_lastBalance = lastBalance;
			}
		}

		public override void RefreshMoney() { }

		public void OnAddBalance(BalanceInfo info) {
			if (info.amount == 0) return;

			float duration = 1f;
			if (info.amount < 0) {
				assets.audioPay.Play();
				Util.Emit(assets.particlePay, 3);
			} else {
				if (info.hitInfo != null) {
					if (info.hitInfo.hitChains <= 3) assets.audioEarnSmall.Play();
					else assets.audioEarnBig.Play();
					duration = slot.effects.GetHitEffect(info.hitInfo).duration*0.8f;
				} else {
					assets.audioEarnSmall.Play();
				}
			}

			Util.InstantiateAt<ElosEffectMoney>(assets.effectMoney, transform).SetText(info.amount, info.hitInfo == null ? "" : info.hitInfo.hitChains + " in a row!").Play(100, 3f);

			if (_moneyTween != null && _moneyTween.IsPlaying()) _moneyTween.Complete();
			_moneyTween = DOTween.To(() => lastBalance, x => lastBalance = x, slot.gameInfo.balance, duration).OnComplete(() => { _moneyTween = null; });
		}

		public void SwitchBG() {
			BGs[indexBG].gameObject.SetActive(false);
			indexBG++;
			if (indexBG >= BGs.Length) indexBG = 0;
			BGs[indexBG].gameObject.SetActive(true);
		}
	}
}