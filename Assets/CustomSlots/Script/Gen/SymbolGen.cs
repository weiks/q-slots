using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CSFramework {
	/// <summary>
	/// A class that handles generating and simulating symbol setups/loadout.
	/// </summary>
	[Serializable]
	public class SymbolGen {
		public enum SortMode {
			None,
			Profit,
			Hits,
			Count
		}

		[Serializable]
		public class Setting {
			public int spinsPerTry = 10000;
			public SortMode sortMode;
			public bool showSymbolCounts = true;
			public bool confirmGeneration = true;
		}

		public CustomSlot slot;
		public Setting setting;

		[NonSerialized] public SymbolGenLog log;
		internal GameInfo gameInfo;
		private Symbol[] symbolsOnPath;
		private Dictionary<SlotMode, List<Symbol[]>> reelMap = new Dictionary<SlotMode, List<Symbol[]>>();
		private List<Symbol[]> reels = new List<Symbol[]>();
		private List<Symbol[]> scores = new List<Symbol[]>();

		public LineManager lineManager { get { return slot.lineManager; } }
		public SymbolManager symbolManager { get { return slot.symbolManager; } }
		public int reelLength { get { return slot.config.reelLength; } }
		public int rows { get { return slot.config.rows; } }
		public int symbolsPerReel { get { return slot.config.symbolsPerReel; } }
		public Line[] lines { get { return lineManager.lines; } }
		public int CostPerSpin(SlotMode mode) { return mode.costPerLine*lines.Length; }
		public int NewCostPerSpin(SlotMode mode) { return (defaultMode.costPerLine - mode.costPerLine)*lines.Length; }
		public SlotMode defaultMode { get { return slot.modes.defaultMode; } }
		public SlotMode freeSpinMode { get { return slot.modes.freeSpinMode; } }
		public SlotMode bonusMode { get { return slot.modes.bonusMode; } }

		private void Init() {
			//	slot.layouter.Refresh();
			slot.Validate();
			for (int x = 0; x < reelLength; x++) {
				reels.Add(new Symbol[symbolsPerReel]);
				scores.Add(new Symbol[rows]);
			}
			foreach (Line line in lines) line.OnGenInit();
			log = new SymbolGenLog(this);
		}

		/// <summary>
		/// Generates a random set of symbols and apply it to the slot.
		/// </summary>
		public void Generate() {
			Init();
			MakeNewReels();
			ApplyLoadout();
			slot.layout.Refresh();
		}

		/// <summary>
		/// Simulate rounds and shows the result.
		/// </summary>
		public void Simulate() {
			if (Application.isPlaying) return;
			Init();
			gameInfo = new GameInfo(slot);
			gameInfo.OnStartRound();
			symbolsOnPath = new Symbol[slot.reels.Length];
			ParseReels();
			for (int i = 0; i < setting.spinsPerTry; i++) Spin();
			log.ProcessResult();
			slot.Validate();
		}

		private void MakeNewReels() {
			int lastIndex = 0;
			foreach (Symbol symbol in symbolManager.symbols)
				if (symbol.minCountPerReel > 0)
					for (int i = 0; i < symbol.minCountPerReel; i++) {
						if (lastIndex >= symbolsPerReel) break;
						for (int x = 0; x < reelLength; x++) reels[x][lastIndex] = symbol;
						lastIndex++;
					}

			for (int x = 0; x < reelLength; x++) for (int y = lastIndex; y < symbolsPerReel; y++) reels[x][y] = symbolManager.GetRandomSymbol();
		}

		private void ParseReels() {
			reelMap.Clear();
			SlotMode cleanMode = new SlotMode();
			foreach (SlotMode mode in new SlotMode[] {cleanMode, defaultMode, freeSpinMode, bonusMode}) {
				List<Symbol[]> r = new List<Symbol[]>();
				for (int j = 0; j < reelLength; j++) r.Add(new Symbol[symbolsPerReel]);
				for (int x = 0; x < reelLength; x++)
					for (int y = 0; y < symbolsPerReel; y++) {
						Symbol symbol = slot.reels[x].symbols[y];
						if (mode.symbolSwaps != null) for (int i = 0; i < mode.symbolSwaps.Count; i++) if (symbol == mode.symbolSwaps[i].from) symbol = mode.symbolSwaps[i].to;
						r[x][y] = symbol;
						if (mode == cleanMode) r[x][y].log.count++;
					}
				reelMap.Add(mode, r);
			}
		}

		private void Spin() {
			slot.lineManager.allHitHolders.Clear();

			if (gameInfo.bonuses > 0) {
				log.totalCost += CostPerSpin(bonusMode);
				gameInfo.bonuses--;
				reels = reelMap[bonusMode];
			} else if (gameInfo.freeSpins > 0) {
				log.totalCost += CostPerSpin(freeSpinMode);
				gameInfo.freeSpins--;
				reels = reelMap[freeSpinMode];
			} else {
				log.totalCost += CostPerSpin(defaultMode);
				reels = reelMap[defaultMode];
			}

			// Draw symbols
			for (int x = 0; x < reelLength; x++) {
				int index = Random.Range(0, symbolsPerReel);
				for (int y = 0; y < rows; y++) {
					Symbol symbol = reels[x][index];

					// Processing scatter hit info.
					for (int j = 0; j < gameInfo.scatterHitInfos.Count; j++) {
						HitInfo info = gameInfo.scatterHitInfos[j];
						if (symbol == info.hitSymbol) info.hitChains++;
					}
					scores[x][y] = symbol;
					index++;
					if (index >= reels[x].Length) index = 0;
				}
			}

			for (int j = 0; j < gameInfo.scatterHitInfos.Count; j++) ProcessHit(gameInfo.scatterHitInfos[j]);

			// Hit Check
			for (int i = 0; i < lines.Length; i++) {
				Line line = lines[i];
				if (line.paths == null) continue;
				for (int x = 0; x < reelLength; x++) symbolsOnPath[x] = scores[x][line.paths[x]];
				line.hitInfo.ParseChains(symbolsOnPath);
				ProcessHit(line.hitInfo);
			}
		}

		private void ProcessHit(HitInfo info) {
			if (info.isHit) {
				log.LogHit(info);
				info.hitSymbol.log.LogHit(info);
			}
			info.Reset();
		}

		private void ApplyLoadout() { for (int x = 0; x < reelLength; x++) reels[x].CopyTo(slot.reels[x].symbols, 0); }
	}
}