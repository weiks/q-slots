using System;
using CSFramework;
using DG.Tweening;
using UnityEngine;
using QuartersSDK;
using PlayFab;
using PlayFab.ClientModels;

namespace Elona.Slot {
	/// <summary>
	/// A main class for Elona Slot(Demo) derived from BaseSlotGame.
	/// For the most part, it's overriding the base methods to add visual/audio effects.
	/// </summary>
	public class Elos : MonoBehaviour {
		public CustomSlot slot;

        [Serializable]
		public class Assets {
			[Serializable]
			public class Tweens {
				public TweenSprite tsBonus, tsIntro1, tsIntro2, tsWin, tsWinSpecial;
			}

			public Tweens tweens;
			public AudioSource bgm, audioDemo, audioEarnSmall, audioEarnBig, audioPay, audioSpin, audioSpinLoop, audioReelStop, audioClick;
			public AudioSource audioWinSmall, audioWinMedium, audioWinBig, audioLose, audioBet, audioImpact, audioBeep;
			public AudioSource audioBonus, audioWinSpecial, audioSpinBonus;
			public ParticleSystem particlePay, particlePrize, particleFreeSpin;
			public ElosEffectMoney effectMoney;
			public ElosEffectBalloon effectBalloon;
		}

		[Serializable]
		public class ElonaSlotData {
			public int expNext = 100;
			public int lv = 1;
			public int exp;
		}

		[Serializable]
		public class ElonaSlotSetting {
			public bool allowDebt = true;
		}

		public Assets assets;
		public ElonaSlotData data;
		public ElonaSlotSetting setting;
		public ElosUI ui;
		public ElosBonusGame bonusGame;
		public float transitionTime = 3f;
		public CanvasGroup cg;
		public GameObject mold;

		protected void Awake() {
            slot.callbacks.onRoundComplete.AddListener(CheckLevelUp);
			slot.callbacks.onRoundComplete.AddListener(Save);
			slot.callbacks.onReelStart.AddListener(OnReelStart);
			slot.callbacks.onProcessHit.AddListener(OnProcessHit);
			Initialize();
		}

        public void Initialize() {
			mold.gameObject.SetActive(false);
			if (!slot.debug.skipIntro) {
				cg.alpha = 0;
				cg.DOFade(1f, transitionTime*0.5f).SetDelay(transitionTime*0.5f);
			}
		}

		private void Update() {
			if (slot.debug.useDebugKeys) {
				if (Input.GetKeyDown(KeyCode.Alpha1)) assets.tweens.tsBonus.Play(0);
				if (Input.GetKeyDown(KeyCode.Alpha2)) assets.tweens.tsWinSpecial.Play(0);
				if (Input.GetKeyDown(KeyCode.Alpha3)) assets.tweens.tsIntro1.Play(0);
				if (Input.GetKeyDown(KeyCode.Alpha4)) assets.tweens.tsIntro2.Play(0);
				if (Input.GetKeyDown(KeyCode.F10)) slot.AddEvent(new SlotEvent(bonusGame.Activate));
			}
			if (Application.platform == RuntimePlatform.Android) {
				if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
			}
		}

		public void Play() {
			if (slot.state == CustomSlot.State.Idle && !setting.allowDebt && slot.gameInfo.balance < slot.gameInfo.roundCost) {
				assets.audioBeep.Play();
                Debug.Log("You're out of Quarters. [purchase]");
                return;
			} else {
                if (Application.isEditor) {
                    slot.Play();
                    PlayerPrefs.SetInt("quartersBalance", slot.gameInfo.balance);
                } else {
                    SpendQuarters(slot.gameInfo.roundCost, "Pay " + slot.gameInfo.roundCost + " Quarters for round cost.");
                }
            }
        }

        private void SpendQuarters(int amount, string description) {
            TransferAPIRequest request = new TransferAPIRequest(amount, description, delegate (string transactionHash) {
                Debug.Log("Quarters transferred: " + transactionHash);

                slot.Play();
                PlayerPrefs.SetInt("quartersBalance", slot.gameInfo.balance);

            }, delegate (string error) {
                Debug.LogError(error);
            });

            Quarters.Instance.CreateTransfer(request);
        }

        public void OnReelStart(ReelInfo info) { if (info.isFirstReel) AddExp(slot.lineManager.activeLines); }

		public void OnProcessHit(HitInfo info) {
            AddExp(info.hitChains);

            if (info.hitSymbol.payType == Symbol.PayType.Normal) {
                int amount = info.payout * slot.gameInfo.bet;
                Debug.Log("payout: " + amount);

                Quarters.Instance.AwardQuarters(amount, delegate (string transactionHash) {
                    Debug.Log("Quarters awarded: " + transactionHash);
                }, delegate (string error) {
                    Debug.LogError(error);
                });
            }
            
            if (info.hitSymbol.payType == Symbol.PayType.Custom) slot.AddEvent(new SlotEvent(bonusGame.Activate));
		}

        public void AddExp(int amount = 0) {
			data.exp += amount;
			ui.RefreshExp();
		}

		public void CheckLevelUp() {
			if (data.exp >= data.expNext) {
				data.lv++;
				data.exp = 0;
				data.expNext *= 2;
				ui.RefreshExp();

				slot.AddEvent(3, () => {
					assets.tweens.tsWinSpecial.SetText("Level Up!").Play();
					//slot.gameInfo.AddBalance(1000);
				});
			}
		}

		public void Save() { Save("game1"); }

		public void Save(string id) {
            Debug.Log("Quarters balance: " + slot.gameInfo.balance);

            PlayerPrefs.SetInt(id + "_balance", slot.gameInfo.balance);
			PlayerPrefs.SetInt(id + "_lv", data.lv);
			PlayerPrefs.SetInt(id + "_exp", data.exp);
			PlayerPrefs.SetInt(id + "_expNext", data.expNext);
			PlayerPrefs.Save();
		}

		public void Load() { Load("game1"); }

		public void Load(string id) {
			//slot.gameInfo.balance = PlayerPrefs.GetInt(id + "_balance", slot.gameInfo.balance);
            slot.gameInfo.balance = PlayerPrefs.GetInt("quartersBalance");
            Debug.Log("Quarters balance: " + slot.gameInfo.balance);

            data.lv = PlayerPrefs.GetInt(id + "_lv", data.lv);
			data.exp = PlayerPrefs.GetInt(id + "_exp", data.exp);
			data.expNext = PlayerPrefs.GetInt(id + "_expNext", data.expNext);
		}

		public void DeleteSave(string id) {
			PlayerPrefs.DeleteKey(id + "_balance");
			PlayerPrefs.DeleteKey(id + "_lv");
			PlayerPrefs.DeleteKey(id + "_exp");
			PlayerPrefs.DeleteKey(id + "_expNext");
		}
	}
}
