using System;
using UnityEngine;

namespace CSFramework {
	/// <summary>
	/// CustomSlot's configuration class.
	/// </summary>
	[Serializable]
	public class SlotConfig {
		[Serializable]
		public enum HolderSortMode {
			DoNotSort,
			AppearAsLastSibling,
			AppearAsFirstSibling
		}

		[Tooltip("When set to false, you will need to manually call <CustomSlot>.Activate() method for your slot to be ready.")] public bool autoActivate = true;

		[Tooltip("When set to false, you will need to manually call <CustomSlot>.StartRound() method for a round to start.")] public bool autoStartRound = true;

		[Space, Tooltip("When turned on, the first line will be always set enabled")] public bool firstLineAlwaysActive = true;

		[Space, Tooltip("Represents the number of rows this slot has"), Range(1, 100)] public int rows = 3;

		[Tooltip("The number of hidden rows that will be generated above the normal rows to prevent graphical glitches. Usually 2 or 1 is enough"), Range(1, 100)] public int hiddenTopRows = 2;

		[Tooltip("The number of hidden rows that will be generated below the normal rows to prevent graphical glitches. Usually 2 or 1 is enough"), Range(1, 100)] public int hiddenBottomRows = 1;

		//	[Tooltip("Extra margin for a spinning symbol to jump to the top of the screen when it reaches the bottom of the screen. It is needed to prevent glitches when using Back Ease for reelStopEase")] public float magrinBottomRow = 60f;

		[Space] public ReelConfig reel;
		[Space] public AdvancedConfig advanced;

		public int totalRows { get { return hiddenTopRows + rows + hiddenBottomRows; } }
		public int symbolsPerReel { get { return reel.symbolsPerReel; } }
		public int reelLength { get { return reel.reelLength; } }
		public bool isRowValid(int index) { return index >= hiddenTopRows && index <= hiddenTopRows + rows - 1; }

		[Serializable]
		public class ReelConfig {
			[Space, Tooltip("Represents the number of symbols each reel has"), Range(1, 100)] public int symbolsPerReel = 20;
			[Tooltip("Represents the number of reels(columns) this slot has"), Range(1, 100)] public int reelLength = 5;
			[Tooltip("Shifts the starting position of reels by the given number of row(s). When specified -1 or below, the positions of reels will be randomized. ")] public int initialReelPosition;
			[Tooltip("Enabling this option doesn't alter actual orders of symbols on a reel but changes how newly appeared symbols picked when spinning. For some people it looks more real.")] public bool invertSymbolDirection;
			[Tooltip("As reels have fixed symbol layouts, depending on your slot setting, players may get same spin result after numbers of game rounds. This option prevents it by randomizing symbol index when reels finish accelerating each time a game is played. Usually, players won't notice the process but if your reels spin at very low speed, it may become noticable.")] public bool randomizeIndexWhenSpinning;
			[Tooltip("When set, either Transform.SetAsFirstSibling or SetAsLastSibling is applied to newly appeared symbols.")] public HolderSortMode sortSymbolTransform;
		}

		[Serializable]
		public class AdvancedConfig {
			[Tooltip("Disable hidden rows' gameobjects at the start. In some layout setup, you might actually want to make hidden rows visible so the option is left here")] public bool disableHiddenRows = true;
			[Tooltip("When enabled, skips validation of lines and symbols at startup. It might save you a few frames but if you forgot to manually hit RefreshLayout button after adding/removing lines and symbols, Unity will give you errors.")] public bool skipStartupValidation = false;
			[Tooltip("An option for 243 ways style slot game. Sholud be turned off for Regular slots which use paylines to evaluate wins. When enabled, there will be only 1 win for each hit chain.")] public bool alternativeLineCheck = false;
		}
	}
}