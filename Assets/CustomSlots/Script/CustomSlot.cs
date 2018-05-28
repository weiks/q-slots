using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CSFramework {
	/// <summary>
	/// The main class of the CustomSlots Framework.
	/// </summary>
	public class CustomSlot : MonoBehaviour {
		public enum State {
			NotStarted,
			Idle,
			SpinStarting,
			Spinning,
			SpinStopping,
			Result,
		}

		[Serializable]
		public class Callbacks {
			public UnityEvent onActivated;
			public UnityEvent onDeactivated;
			public UnityEvent onRoundStart;
			public ReelInfo onReelStart;
			[Hide] public UnityEvent onNewSymbolAppear;
			public ReelInfo onReelStop;
			[Hide] public UnityEvent onRoundInterval;
			public HitInfo onProcessHit;
			public UnityEvent onRoundComplete;
			[Hide] public UnityEvent onSlotStateChange;
			public SlotModeInfo onSlotModeChange;
			[Hide] public BalanceInfo onAddBalance;
			public LineInfo onLineSwitch;
		}

		public SkinManager skin;
		public LineManager lineManager;
		public SymbolManager symbolManager;

		[Hide] public SymbolGen symbolGen;
		[Hide] public GridLayoutGroup layoutReel;
		[Hide] public GridLayoutGroup layoutRow;
		[Hide] public RectTransform mainScreen;

		[Space] public SlotConfig config;
		[Space] public SlotModeManager modes;
		[Space] public SlotEffectManager effects;
		[Space] public Callbacks callbacks;
		[Space] public SlotLayouter layout;
		[Space] public SlotDebug debug;

		[HideInInspector] public Reel[] reels;
		[HideInInspector] public Row[] rows;

		private bool isInitialized;
		private int currentReelIndex = 0;
		private Sequence sequenceResult;
		private Queue<SlotEvent> events = new Queue<SlotEvent>();

		public GameInfo gameInfo { get; private set; }
		public State state { get; private set; }
		public SlotEvent currentEvent { get; private set; }
		public SlotMode currentMode { get { return modes.current; } }
		public bool isIdle { get { return !isLocked && state == State.Idle; } }
		public bool isLocked { get { return currentEvent != null || events.Count > 0; } }
		private void Awake() { if (config.autoActivate) Activate(); }

		public void Initialize() {
			if (isInitialized) return;
			isInitialized = true;
			gameInfo = new GameInfo(this);
			if (!config.advanced.skipStartupValidation) Validate();
			modes.Initialize();
			layout.SetActiveLayout(false);
			lineManager.SwitchAllLines(false, true);
		}

		public void Activate() {
			Initialize();
			Time.timeScale = 1f;
			gameObject.SetActive(true);
			effects.transitionIn.Play(this, false, _Activate);
		}

		private void _Activate() {
			if (!debug.skipIntro) effects.introAnimation.Play(this);
			if (state == State.NotStarted && config.autoStartRound) AddEvent(StartRound);
			callbacks.onActivated.Invoke();
		}

		public void Deactivate(bool destroy = false) { effects.transitionOut.Play(this, true, () => { _Deactivate(destroy); }); }

		private void _Deactivate(bool destroy) {
			callbacks.onDeactivated.Invoke();
			if (destroy) Destroy(gameObject);
			else gameObject.SetActive(false);
		}

		/// <summary>
		/// Validate() should be called when a slot needs to refresh its data and layout.
		/// (Number of reels and rows, adding/removing symbols and lines etc)
		/// </summary>
		public void Validate() {
			symbolManager.Validate(this);
			lineManager.Validate(this);
			reels = layoutReel.transform.GetComponentsInChildren<Reel>();
			rows = layoutRow.transform.GetComponentsInChildren<Row>();
			foreach (Reel reel in reels) reel.Validate(this);
		}

		/// <summary>
		/// Queue an event to CustomSlot's event system.
		/// </summary>
		public SlotEvent AddEvent(SlotEvent e) {
			events.Enqueue(e);
			return e;
		}

		public SlotEvent AddEvent(Sequence sequence) { return AddEvent(new EventTweenSequence(sequence)); }
		public SlotEvent AddEvent(float duration, TweenCallback onStart = null, TweenCallback onComplete = null) { return AddEvent(Util.Sequence(duration, onStart, onComplete)); }
		public SlotEvent AddEvent(TweenCallback action) { return AddEvent(Util.Sequence(0, action)); }
		public SlotEvent AddEvent(Tweener tween) { return AddEvent(DOTween.Sequence().Join(tween)); }

		private void SwitchState(State newState) {
			state = newState;
			callbacks.onSlotStateChange.Invoke();
		}

		public void SwitchMode() { if (state == State.Idle) modes.SwitchMode(); }

		private void Update() {
			if (Input.GetKeyDown(KeyCode.A)) {
				layout.sizeSymbol = new Vector2(100, 100);
				layout.Refresh(false);
			}
			if (Input.GetKeyDown(KeyCode.S)) {
				layout.sizeSymbol = new Vector2(150, 150);

				layout.Refresh(false);
			}

			// If there's an active event, skip all other updates until the event is finished.
			if (currentEvent != null) {
				currentEvent.Update();
				if (currentEvent.isDeactivated) currentEvent = null;
				else return;
			}

			if (events.Count > 0) {
				currentEvent = events.Dequeue();
				currentEvent.Activate();
				return;
			}

			switch (state) {
				case State.Idle:
					if (currentMode.forcePlay) StartSpin();
					debug.OnUpdate();
					break;

				case State.SpinStarting:

					break;

				case State.Spinning:

					break;

				case State.SpinStopping:
					for (int i = 0; i < reels.Length; i++) if (reels[i].isSpinning) return;
					callbacks.onRoundInterval.Invoke();
					SwitchState(State.Result);
					break;

				case State.Result:
					foreach (Line line in lineManager.lines) if (line.hitInfo.ProcessHitCheck()) return;
					foreach (HitInfo hitInfo in gameInfo.scatterHitInfos) if (!hitInfo.hitSymbol.ignoreThisRound && hitInfo.ProcessHitCheck()) return;
					gameInfo.OnRoundComplete();
					ClearManipulation();
					SwitchState(State.NotStarted);
					callbacks.onRoundComplete.Invoke();
					if (effects.lineHitEffect.displayAsPlayback) StartPlaybackResult();
					if (config.autoStartRound) StartRound();
					break;
			}
		}

		public void StartRound() {
			gameInfo.OnStartRound();
			SwitchState(State.Idle);
			SwitchMode();
			symbolManager.OnStartRound();
			lineManager.OnStartRound();
			callbacks.onRoundStart.Invoke();
			if (currentMode.forcePlay) Play();
		}

		/// <summary>
		/// Starts spinning reels if State is idle, and stops them if they are spinning.
		/// </summary>
		public void Play() {
			if (isLocked) return;
			switch (state) {
				case State.Idle:
					StartSpin();
					break;

				case State.Spinning:
					if (currentMode.spinMode == SlotMode.SpinMode.ManualStopAll) StopSpin();
					if (currentMode.spinMode == SlotMode.SpinMode.ManualStopOne) StopReel();
					if (currentMode.spinMode == SlotMode.SpinMode.ManualStartOne) {
						StopReel();
						StartReel();
					}
					break;
			}
		}

		/// <summary>
		/// Starts spinning all the reels in a sequence and changes the State to "Intro".
		/// </summary>
		public void StartSpin(float duration = 0) {
			if (!isIdle) return;
			StopPlaybackResult();

			SwitchState(State.SpinStarting);
			if (duration == 0) duration = currentMode.spinMode == SlotMode.SpinMode.AutoStop ? currentMode.autoStopTime + 0.1f : 0;

			float _delay = debug.fastSpin ? 0 : currentMode.spinStartDelay;
			if (debug.alwaysMaxLines) lineManager.SwitchAllLines(true, true);
			currentReelIndex = 0;
			StopCoroutine("StopSpinCoroutine");

			if (currentMode.spinMode == SlotMode.SpinMode.ManualStartOne) {
				StartReel();
			} else {
				Sequence sequence = DOTween.Sequence();
				foreach (Reel reel in reels) sequence.Append(Util.Tween(_delay, reel.Spin));
				sequence.Append(currentMode.reelAccelerateTime, OnIntroComplete);
				AddEvent(sequence);
			}
			gameInfo.OnStartSpin();

			if (duration > 0) AddEvent(duration, null, StopSpin);
		}

		public void StartReel() {
			if (currentReelIndex >= reels.Length) return;
			Sequence sequence = DOTween.Sequence();
			sequence.Append(Util.Tween(currentMode.spinStartDelay, reels[currentReelIndex].Spin));
			sequence.Append(currentMode.reelAccelerateTime, OnIntroComplete);
			AddEvent(sequence);
		}

		/// <summary>
		/// Stops a spinning reel . If the reel is the last spinning reel, CustomSlot will start performing Hit Check.
		/// </summary>
		public void StopReel() {
			reels[currentReelIndex].Stop();
			currentReelIndex++;
			if (currentReelIndex >= reels.Length) SwitchState(State.SpinStopping);
		}

		/// <summary>
		/// Stops all the spinning reels and changes the State to "Outro". When all the reels stop animating, Hit Check will be performed on each line.
		/// </summary>
		public void StopSpin() {
			SwitchState(State.SpinStopping);
			StartCoroutine("StopSpinCoroutine");
		}

		private IEnumerator StopSpinCoroutine() {
			float delay = debug.fastSpin ? 0 : currentMode.spinStopDelay;
			while (currentReelIndex < reels.Length) {
				StopReel();
				while (reels[currentReelIndex - 1].isSearchingForSymbol) { yield return null; }
				yield return new WaitForSeconds(delay);
			}
		}

		private void OnIntroComplete() { SwitchState(State.Spinning); }

		internal void ProcessHit(HitInfo info) {
			info.payout = info.hitSymbol.GetPayAmount(info.hitChains);

			gameInfo.AddHit();

			if (info.hitSymbol.payType == Symbol.PayType.Normal) {
				gameInfo.AddBalance(info.payout*gameInfo.bet, info);
			} else if (info.hitSymbol.payType == Symbol.PayType.FreesSpin) {
				AddFreeSpin(info.payout);
			} else if (info.hitSymbol.payType == Symbol.PayType.Bonus) {
				AddBonus(info.payout);
			}

			if (!effects.lineHitEffect.displayAsPlayback) AddEvent(DisplayHitEffect(info));
		}

		public Sequence DisplayHitEffect(HitInfo info) {
			Sequence sequence = info.sequence = DOTween.Sequence();
			sequence.OnStart(() => {
				if (!info.isSequencePlayed) {
					callbacks.onProcessHit.Invoke(info);
					info.isSequencePlayed = true;
				}
			});
			SlotEffectManager.SymbolHitEffect hitEffect = effects.GetHitEffect(info);
			if (hitEffect == null) {
				sequence.Append(Util.Tween(1f));
			} else {
				for (int i = 0; i < info.holders.Length; i++) if (i < info.hitChains) sequence.Join(hitEffect.Play(info.holders[i].link, i));
				if (info.line) sequence.Join(effects.lineHitEffect.Play(info.line, hitEffect.duration));
			}
			return sequence;
		}

		private void StartPlaybackResult() {
			HitInfo firstHit = (from line in lineManager.lines where line.hitInfo.isHit select line.hitInfo).FirstOrDefault();
			if (firstHit == null) foreach (HitInfo hitInfo in gameInfo.scatterHitInfos) if (!hitInfo.hitSymbol.ignoreThisRound && hitInfo.isHit) firstHit = hitInfo;
			if (firstHit == null) return;
			sequenceResult = DisplayHitEffect(firstHit);
			foreach (Line line in lineManager.lines) if (line.hitInfo.isHit && line.hitInfo != firstHit) sequenceResult.Append(DisplayHitEffect(line.hitInfo));
			foreach (HitInfo hitInfo in gameInfo.scatterHitInfos) if (!hitInfo.hitSymbol.ignoreThisRound && hitInfo.isHit && hitInfo != firstHit) sequenceResult.Append(DisplayHitEffect(hitInfo));
		}

		public void StopPlaybackResult() {
			if (sequenceResult != null) {
				sequenceResult.Complete(true);
				sequenceResult = null;
			}
		}

		/// <summary>
		/// Returns a list of all visible SymbolHolder.  
		/// </summary>
		public List<SymbolHolder> GetVisibleHolders() {
			List<SymbolHolder> list = new List<SymbolHolder>();
			foreach (Row row in rows) {
				if (row.isHiddenRow) continue;
				foreach (SymbolHolder holder in row.holders) list.Add(holder);
			}
			return list;
		}

		public List<SymbolHolder> GetAllHolders() {
			List<SymbolHolder> list = new List<SymbolHolder>();
			foreach (Reel reel in reels) foreach (SymbolHolder holder in reel.holders) list.Add(holder);
			return list;
		}

		public Reel GetReelAt(int index) { return index < 0 || index >= reels.Length ? null : reels[index]; }

		public void AddFreeSpin(int amount) {
			gameInfo.freeSpins += amount;
			if (gameInfo.freeSpins < 0) gameInfo.freeSpins = 0;
		}

		public void AddBonus(int amount) {
			gameInfo.bonuses += amount;
			if (gameInfo.bonuses < 0) gameInfo.bonuses = 0;
		}

		public void SetBet(int amount) { gameInfo.bet = amount; }

		/// <summary>
		/// Set the result for the next spin. 
		/// The first parameter(rowOffset) is an offset from the top visible row. When specified 0 the symbols will land on the top visible row, 1 for the 2nd row and etc....
		/// Specify symbol(s) you want to land in the result in the second parameter.
		/// </summary>
		public void SetManipulation(int rowOffset, string symbolId) {
			Symbol symbol = symbolManager.GetSymbol(symbolId);
			if (symbol) foreach (Reel reel in reels) reel.SetManipulation(symbol, 0, rowOffset);
		}

		public void SetManipulation(int rowOffset, Symbol symbol) { foreach (Reel reel in reels) reel.SetManipulation(symbol, 0, rowOffset); }
		public void SetManipulation(int rowOffset, params Symbol[] symbols) { for (int i = 0; i < symbols.Length; i++) if (i < reels.Length) reels[i].SetManipulation(symbols[i], 0, rowOffset); }
		public void SetManipulation(int rowOffset, params int[] symbolIndexes) { for (int i = 0; i < symbolIndexes.Length; i++) if (i < reels.Length) reels[i].SetManipulation(null, symbolIndexes[i], rowOffset); }
		private void ClearManipulation() { foreach (Reel reel in reels) reel.ClearManipulation(); }
	}
}