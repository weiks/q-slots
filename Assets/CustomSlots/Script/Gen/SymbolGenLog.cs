using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CSFramework {
	[Serializable]
	public class SymbolGenLog : SymbolLog {
		[HideInInspector] public string summary = "";
		[HideInInspector] public string warning = "";
		[HideInInspector] public List<SymbolLog> symbolLogs = new List<SymbolLog>();

		public string newLine { get { return Environment.NewLine; } }
		public int balance { get { return income - totalCost; } }

		public SymbolGenLog(SymbolGen gen) : base(null, gen) {
			foreach (Symbol symbol in gen.slot.symbolManager.symbols) {
				symbol.log = new SymbolLog(symbol, gen);
				symbolLogs.Add(symbol.log);
			}
		}

		public void Warn(string s) { warning += (warning == "" ? "" : newLine) + s; }

		public void ProcessResult() {
			summary += "Win rate:  " + Ratio(income, totalCost) + "    ( Final balance: " + balance + "  Profit: " + income + "  Cost: " + totalCost + " )" + newLine;
			summary += "Hit rate:  " + Ratio(hits, gen.setting.spinsPerTry) + "   ( Total Hits: " + hits + " )" + newLine;
			summary += "FreeSpins:  " + Percentage(freeSpins, gen.setting.spinsPerTry) + "   ( Total FreeSpins: " + freeSpins + "  Cost saved: " + freeSpins*gen.NewCostPerSpin(gen.slot.modes.freeSpinMode) + " )" + newLine;
			summary += "Bonuses:  " + Percentage(bonuses, gen.setting.spinsPerTry) + "   ( Total Bonuses: " + bonuses + "  Cost saved: " + bonuses*gen.NewCostPerSpin(gen.slot.modes.bonusMode) + " )" + newLine + newLine;

			if (gen.setting.sortMode == SymbolGen.SortMode.Profit) symbolLogs.Sort((x, y) => y.income - x.income);
			if (gen.setting.sortMode == SymbolGen.SortMode.Hits) symbolLogs.Sort((x, y) => y.hits - x.hits);
			if (gen.setting.sortMode == SymbolGen.SortMode.Count) symbolLogs.Sort((x, y) => y.count - x.count);

			ProcessChainMap();

			name = "Summary   ( Result of " + gen.setting.spinsPerTry + " spins )";
		}

		public void ProcessChainMap() {
			for (int i = chainMap.Length - 1; i >= 0; i--) {
				if (chainMap[i] != 0) summary += "[x" + i + "] " + Percentage(chainMap[i], hits, 1) + "   ( Total Hits: " + chainMap[i] + " )" + newLine;
			}

			summary += newLine;

			StringBuilder builder = new StringBuilder();

			//	builder.AppendFormat("{0,-20} {1,-30} {2,-40}", "[Symbol]", "[Profit]", "[Hits]");
			builder.Append("[Symbol]    -    [Profit]    -    [Hits]" + newLine + newLine);

			foreach (SymbolLog log in symbolLogs) {
				string chain = "";
				for (int i = log.chainMap.Length - 1; i >= 0; i--) {
					if (log.chainMap[i] != 0) chain += "[x" + i + "] " + Percentage(log.chainMap[i], hits, 1) + "   ";
				}
				int maxChain = Mathf.Min(chainMap.Length, gen.reelLength);
				if (log.chainMap[maxChain] == 0 && log.symbol.minCountPerReel == 0) Warn(log.symbol.name + " never scored max chains[x" + maxChain + "]");
				builder.AppendFormat("{0,-20} {1,-12} {2,-50}", "" + log.symbol.name + (gen.setting.showSymbolCounts ? "[" + log.count + "]" : ""), Percentage(log.income, income, 1), "" + Percentage(log.hits, hits, 1) + "  =   " + chain);
				builder.AppendLine();
			}
			summary += builder.ToString() + newLine + warning;
		}
	}

	[Serializable]
	public class SymbolLog {
		private const int maxLoggedChains = 10;
		protected SymbolGen gen;
		public string name;
		public int hits;
		public int income;
		public int count;
		public Symbol symbol;
		public int[] chainMap;
		public int freeSpins;
		public int bonuses;
		public int totalCost;

		public SymbolLog(Symbol symbol, SymbolGen gen) {
			this.gen = gen;
			chainMap = new int[maxLoggedChains];
			this.symbol = symbol;
		}

		public string Ratio(int a, int b) { return "" + Math.Round((float) a/b, 2); }

		public string Percentage(int a, int b, int round = 0) {
			if (round > 0) return "" + Math.Round(100f*a/b, round) + "%";
			return "" + a*100/b + "%";
		}

		public void LogHit(HitInfo info) {
			int pay = info.hitSymbol.GetPayAmount(info.hitChains);

			if (info.hitSymbol.payType == Symbol.PayType.Normal) income += pay;
			else if (info.hitSymbol.payType == Symbol.PayType.FreesSpin) {
				if (!symbol) {
					freeSpins += pay;
					gen.gameInfo.freeSpins += pay;
				}
			} else if (info.hitSymbol.payType == Symbol.PayType.Bonus) {
				if (!symbol) {
					bonuses += pay;
					gen.gameInfo.bonuses += pay;
				}
			}

			hits++;
			chainMap[info.hitChains < maxLoggedChains ? info.hitChains : maxLoggedChains - 1]++;
		}
	}
}