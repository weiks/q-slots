using System;
using UnityEngine;

namespace CSFramework {
	/// <summary>
	/// Symbol is a class that mainly holds symbol data/information.
	/// SymbolHolder is responsible for actually displaying symbols a reel has. 
	/// </summary>
	public class Symbol : MonoBehaviour {
		public enum MatchType {
			Normal,
			Wild,
			Scatter,
			Custom
		}

		public enum PayType {
			Normal,
			FreesSpin,
			Bonus,
			Custom
		}

		public Animator animator;
		public Sprite sprite;
		public Sprite backgroundSprite;
		[Tooltip("When the value is greater than 1, the symbol is considered MRS(multi-row-symbol) and takes up multiple rows.")] public int rowSize;
		[Tooltip("Specified in a list of numbers separated with comma. (e.g 0,0,100,500). \n\nThe first number represents a pay when there's only 1 symbol in a line. The 2nd  number represents a pay when there're 2  matching symbols in a line(2-in-a-row, in other word). And so on.\n\nWhile the number is 0, the symbol is not considered as a hit(or win).\n\nIn the above example, the symbol's pay would be 100 when 3-in-a-row and 500 when 4-in-a-row.")] public string _pays = "0,0,0,0,0";

		[Tooltip("Represents how a symbol rewards a player. Normal will pay a player normally and FreeSpin will give a player free spins both according to the numbers specified in pays parameter. Custom does nothing and it is there so you can use your own code.")] public PayType payType;
		public MatchType matchType;
		[Header("Generator Setting"), Range(0, 100), Tooltip("The parameter is used by SymbolGen and represents how often the symbol is randomly chosen to appears on a reel. The value is seen as relative weight in all the other symbols.  ")] public float frequency = 50;

		[Tooltip("The parameter is also used by Symbol gen and represents a minimum number of symbols guaranteed to appear on a reel. ")] public int minCountPerReel = 0;
		[HideInInspector] public int[] pays;
		[HideInInspector] public int minChains = 0;
		[NonSerialized] internal SymbolLog log;
		[NonSerialized] public bool ignoreThisRound;
		public bool isMRS { get { return rowSize > 1; } }

		/// <summary>
		/// A method to check whether the given symbol can match this symbol or not.
		/// A symbol with SymboleType.Wild set can match any symbol.
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns></returns>
		public virtual bool CanMatch(Symbol symbol) {
			if (ignoreThisRound || this.matchType == MatchType.Scatter || symbol.matchType == MatchType.Scatter) return false;
			return symbol == this || symbol.matchType == MatchType.Wild || this.matchType == MatchType.Wild;
		}

		/// <summary>
		/// A method to get the amount of payment based on the number of chains.
		/// </summary>
		/// <param name="chains"></param>
		/// <returns></returns>
		public virtual int GetPayAmount(int chains) {
			chains--;
			return chains >= pays.Length ? pays[pays.Length - 1] : pays[chains];
		}

		internal int GetMaxPay() { return pays.Length > 0 ? pays[pays.Length - 1] : 0; }

		internal void Validate() {
			pays = Util.StringToInts(_pays);
			minChains = -1;
			for (int i = 0; i < pays.Length; i++)
				if (pays[i] != 0) {
					minChains = i + 1;
					break;
				}
		}
	}
}