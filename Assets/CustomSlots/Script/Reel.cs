using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CSFramework {
	/// <summary>
	/// A class that controlls a reel and its symbol holders.
	/// A reel actually never spins but the symbol holders on the reel do.
	/// When Spin() is called, all the holders on the reel start moving toward the bottom of the screen
	/// then jump to the top once they reach the bottom which makes it look like the reel is spinning.
	/// When a symbol holder reaches the bottom of the screen, it draw a new symbol from the list of symbols its parent has.
	/// </summary>
	public class Reel : MonoBehaviour {
		public class Manipulation {
			public int rowOffset, index;
			public Symbol symbol;
		}

		internal float spacing { get { return slot.layoutRow.cellSize.y; } }
		internal float maxY { get { return -spacing*holders.Count; } }

		[Hide] public int index;
		[Hide] public CustomSlot slot;
		[NonSerialized] public int lastSymbol = -1;
		[NonSerialized] public Manipulation manipulation;
		[NonSerialized] public bool isSpinning, spinHolders;
		[NonSerialized] public bool isSearchingForSymbol;
		[NonSerialized] public float y, nextY, speed;
		public Symbol[] symbols;
		[HideInInspector] public List<SymbolHolder> holders;
		internal Tween tweenAccel;
		internal int nextSymbol {
			get {
				if (slot.config.reel.invertSymbolDirection) return (lastSymbol - 1 < 0 ? slot.config.symbolsPerReel : lastSymbol) - 1;
				return (lastSymbol + 1 >= symbols.Length) ? 0 : lastSymbol + 1;
			}
		}

		internal void OnRefreshLayout(CustomSlot slot, int index) {
			this.slot = slot;
			this.index = index;
			transform.SetParent(slot.layoutReel.transform, false);
			symbols = new Symbol[slot.config.symbolsPerReel];
			holders.Clear();
			Util.DestroyChildren<SymbolHolder>(this);
			for (int i = 0; i < slot.config.totalRows; i++) holders.Add(Instantiate(slot.skin.symbolHolder).OnRefreshLayout(this, i));
		}

		public void RefreshHolders() { foreach (SymbolHolder holder in holders) holder.RefreshImage(); }

		internal void Validate(CustomSlot slot) {
			this.slot = slot;
			for (int i = 0; i < symbols.Length; i++) if (symbols[i] == null) symbols[i] = slot.skin.defaultSymbol;

			if (slot.config.reel.initialReelPosition != 0) {
				if (slot.config.reel.initialReelPosition < 0) {
					lastSymbol = Random.Range(0, symbols.Length);
				} else {
					lastSymbol = slot.config.reel.initialReelPosition%symbols.Length - 1;
				}
			}

			ValidateMultiRow();

			for (int i = holders.Count - 1; i >= 0; i--) holders[i].SetNextItem();
			lastSymbol = holders[0].symbolIndex;
			RefreshRows();
			for (int i = holders.Count - 1; i >= 0; i--) RefreshMultiRowImage(i);
		}

		internal void Update() {
			if (!isSpinning) return;
			if (spinHolders) y += speed*Time.deltaTime;
			while (y >= nextY + spacing) AdvanceReel();
			RefreshHolderPositions();
		}

		internal void AdvanceReel() {
			nextY += spacing;
			var endHolder = holders[holders.Count - 1];
			holders.RemoveAt(holders.Count - 1);
			holders.Insert(0, endHolder);
			endHolder.SetNextItem();
			RefreshMultiRowImage(0);
		}

		public void ValidateMultiRow() {
			for (int i = 0; i < symbols.Length; i++) {
				var symbol = symbols[i];
				if (symbol.isMRS) {
					if (i + symbol.rowSize >= symbols.Length || i == 0) symbols[i] = slot.symbolManager.GetRandomSymbol(true);
					else {
						for (int j = 0; j < symbol.rowSize - 1; j++) {
							i++;
							symbols[i] = symbol;
						}
						i++;
						if (symbols[i].isMRS) symbols[i] = slot.symbolManager.GetRandomSymbol(true);
					}
				}
			}
		}

		public void RefreshMultiRowImage(int i) {
			var h = holders[i];
			var s = h.symbol;
			h.image.enabled = true;
			h.link = h;
			h._rect.sizeDelta = slot.layout.sizeSymbol;
			h._rect.pivot = new Vector2(0.5f, 0.5f);
			h.image.preserveAspect = false;
			if (s.isMRS && i < holders.Count - 1) {
				if (holders[i + 1].symbol == h.symbol) {
					h.image.enabled = false;
					h.link = holders[1].link;
				} else {
					h._rect.sizeDelta = new Vector2(slot.layout.sizeSymbol.x, slot.layout.sizeSymbol.y*s.rowSize + (slot.layout.spacingSymbol.y*s.rowSize));
					h._rect.pivot = new Vector2(0.5f, 0.5f/s.rowSize);
				}
			}
		}

		/// <summary>
		/// A method to start accelerating the reel.
		/// </summary>
		public void Spin() {
			slot.callbacks.onReelStart.Invoke(new ReelInfo(this));
			speed = 0;
			y = nextY = 0;
			if (slot.config.reel.randomizeIndexWhenSpinning) {
				tweenAccel = DOTween.To(() => speed, x => speed = x, slot.currentMode.reelMaxSpeed, slot.currentMode.reelAccelerateTime).SetEase(slot.currentMode.reelAccelerateEase).OnComplete(() => {
					lastSymbol = Random.Range(0, symbols.Length);
					for (int i = holders.Count - 1; i >= 0; i--) holders[i].SetNextItem();
				});
			} else {
				tweenAccel = DOTween.To(() => speed, x => speed = x, slot.currentMode.reelMaxSpeed, slot.currentMode.reelAccelerateTime).SetEase(slot.currentMode.reelAccelerateEase);
			}
			isSpinning = spinHolders = true;
		}

		/// <summary>
		/// A method to stop the reel
		/// Once a reel stops and the symbols snap to rows, the rows will parse symbols each reel has
		/// and stores them in a list. 
		/// </summary>
		public void Stop() {
			/*
			//an alternative way to manipulate result (by user's request) to make reels stop more natural.
			//note:set Reel Stop Distance to 4
			
			if (manipulation != null && !manipulation.symbol) {
				var sortedHolder = holders.ToArray();
				Array.Sort(sortedHolder, (a, b) => (int) (b.y - a.y));
				lastSymbol = GetNormalizedIndex(manipulation.index - 1 - manipulation.rowOffset);
				sortedHolder[0].SetNextItem();
				_Stop(false);
				return;
			}
			*/

			if (manipulation != null) StartCoroutine("StopCoroutine");
			else _Stop(true);
		}

		/// <summary>
		/// A method to stop the reel and make the specified symbol land on the specified row.
		/// When rowOffset is not specified, the symbol will land on the top visible row of your slot.
		/// </summary>
		public void StopAt(Symbol desiredSymbol, int rowOffset = 0) {
			SetManipulation(desiredSymbol, 0, rowOffset);
			Stop();
		}

		/// <summary>
		/// A method to stop the reel at specified symbol index.
		/// When rowOffset is not specified, the symbol will land on the top visible row of your slot.
		/// </summary>
		public void StopAt(int desiredSymbolIndex, int rowOffset = 0) {
			SetManipulation(null, desiredSymbolIndex, rowOffset);
			Stop();
		}

		internal IEnumerator StopCoroutine() {
			isSearchingForSymbol = true;
			while (true) {
				SymbolHolder holder = holders[Mathf.Clamp(slot.config.hiddenTopRows - GetStopDistance() + manipulation.rowOffset, 0, holders.Count - 1)];
				if ((manipulation.symbol && holder.symbol == manipulation.symbol) || (!manipulation.symbol && holder.symbolIndex == manipulation.index)) {
					_Stop(false);
					break;
				}
				yield return null;
			}
		}

		internal void _Stop(bool smoothenDistance = true) {
			isSearchingForSymbol = false;
			if (tweenAccel != null) tweenAccel.Kill();
			speed = 0;
			int distance = GetStopDistance();
			float diff = y%spacing;
			if (smoothenDistance && diff > spacing/2) distance++;
			float targetY = y - diff + spacing*distance;
			DOTween.To(() => y, x => y = x, targetY, slot.currentMode.reelStopTime).SetEase(slot.currentMode.reelStopEase).OnComplete(OnAllHoldersStopped);
			spinHolders = false;
			slot.callbacks.onReelStop.Invoke(new ReelInfo(this));
		}

		public int GetStopDistance() {
			if (manipulation != null) return Mathf.Clamp(slot.currentMode.reelStopDistance + 1, 0, slot.config.hiddenTopRows);
			return Mathf.Clamp(slot.currentMode.reelStopDistance + 1, 0, slot.rows.Length);
		}

		internal void OnAllHoldersStopped() {
			Update();
			isSpinning = false;
			RefreshRows();
		}

		public void RefreshRows() { for (int i = 0; i < holders.Count; i++) slot.rows[i].holders[index] = holders[i]; }
		public void RefreshHolderPositions() { for (int i = 0; i < holders.Count; i++) holders[i].y = i*-spacing - y%spacing; }
		public int GetNormalizedIndex(int index) { return index < 0 ? symbols.Length + index%symbols.Length : index >= symbols.Length ? index%symbols.Length : index; }

		/// <summary>
		/// Swaps a symbol at the given index. An index of current visible symbol can be retrieved by GetTopSymbolIndex() or GetSymbolIndexAt() Method.
		/// As the swapping is done instantly, if the method is called during slot's OnProcessHit callback it will affect the remaining results.  
		/// </summary>
		/// <param name="index"></param>
		/// <param name="symbol"></param>
		public void SwapSymbolAt(int index, Symbol symbol) {
			symbols[GetNormalizedIndex(index)] = symbol;
			RefreshHolders();
		}

		/// <summary>
		/// Returns an index of the first(top) visible symbol on this reel. 
		/// </summary>
		/// <returns></returns>
		public int GetTopSymbolIndex() {
			foreach (Row row in slot.rows) if (!row.isHiddenRow) return row.holders[index].symbolIndex;
			return -1;
		}

		/// <summary>
		/// Returns an index of a symbol on this reel specified by the parameter.
		/// IE. 0 for the first visible symbol, 1 for the second visible symbol and so on.
		/// </summary>
		/// <param name="indexOfRow"></param>
		/// <returns></returns>
		public int GetVisibleSymbolIndexAt(int indexOfRow) { return GetTopSymbolIndex() - indexOfRow; }

		public void SetManipulation(Symbol desiredSymbol, int desiredIndex = 0, int rowOffset = 0) { manipulation = new Manipulation {symbol = desiredSymbol, index = desiredIndex, rowOffset = rowOffset}; }
		public void ClearManipulation() { manipulation = null; }
	}
}