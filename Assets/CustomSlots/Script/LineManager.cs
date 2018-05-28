using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CSFramework {
	/// <summary>
	/// A manager class to manage lines.
	/// </summary>
	public class LineManager : MonoBehaviour {
		[Hide] public CustomSlot slot;
		[HideInInspector] public Line[] lines;
		public int currentIndex { get; private set; }
		public int activeLines { get { return currentIndex; } }

		[Tooltip("Gradient colors to color lines. The left-most color represents the color for the first line(first order) and the right-most color for the last line(last order). ")] public Gradient gradientColor;

		[Header("Editor Options")] public bool drawGizmo = true;
		[Tooltip("When turned on, Line's gameobject will be automatically named according to its setup")] public bool autoNameGameobject = true;
		[Tooltip("When turned on and if there's a Text component assigned to a line, the line's order number is set to its text")] public bool autoSetTextNumber = true;
		[Tooltip("When turned on and using autoSetTextNumber, it will increment the number shown by one(Thus 0 will be displayed as 1)")] public bool incrementTextNumber = true;
		public Color gizmoColor = new Color(0, 1, 0.5f, 1);

		public HashSet<List<SymbolHolder>> allHitHolders = new HashSet<List<SymbolHolder>>(); // For alternativeLineCheck to determine whether if a win is already made for a chain

		internal void Validate(CustomSlot slot) {
			this.slot = slot;
			lines = GetComponentsInChildren<Line>();
			Array.Sort(lines);
			foreach (Line line in lines) line.Validate(this);
		}

		public void CreateLinesForWayGame() {
			Util.DestroyChildren(transform);
			int maxPath = (int) Math.Pow(slot.config.rows, slot.config.reelLength - 1);
			int[,] paths = new int[maxPath, slot.config.reelLength - 1];
			StringBuilder sb = new StringBuilder();
			int rows = slot.config.rows;
			for (int i = 0; i < maxPath; i++) for (int col = 0; col < slot.config.reelLength - 1; col++) paths[i, col] = i/(int) Math.Pow(rows, col)%rows;

			for (int row = 0; row < rows; row++) {
				for (int i = 0; i < maxPath; i++) {
					Line line = Util.InstantiateAt(slot.skin.line, transform);
					line.row = row;
					sb.Append(paths[i, 0] - row);
					for (int col = 1; col < slot.config.reelLength - 1; col++) {
						int fix = paths[i, col - 1];
						sb.Append("," + (paths[i, col] - fix));
					}
					line._path = sb.ToString();
					line.order = i + row*maxPath;
					line.image.gameObject.SetActive(false);
					line.textIndex.gameObject.SetActive(false);
					sb.Length = 0;
				}
			}
		}

		internal
			void OnRefreshLayout() {
			foreach (
				Line line in lines) line.SetPaths();
		}

		internal
			void OnStartRound() {
			foreach (
				Line line in lines) line.OnStartRound();
		}

		/// <summary>
		/// Enables next line. Returns false if line's state could not be changed.
		/// </summary>
		public bool EnableNextLine() { return SwitchLine(currentIndex, true); }

		/// <summary>
		/// Disables current line. Returns false if line's state could not be changed.
		/// </summary>
		public bool DisableCurrentLine() { return SwitchLine(currentIndex - 1, false); }

		/// <summary>
		/// Switch the line's state to either enabled or disabled.
		/// Only enabled lines will be checked when CustomSlot performs Hit Check.
		/// </summary>
		/// <param name="index">index of a line</param>
		/// <param name="enable">true to enable, flase to disable</param>
		/// <param name="instant">when set to true, the line will be instantly enabled/disabled without animation</param>
		/// <param name="force">when set to false, Line's State will not change when the State is not Idle or if there's an active event in the CustomSlot's event system</param>
		public bool SwitchLine(int index, bool enable, bool instant = false, bool force = false) {
			if (slot.debug.alwaysMaxLines) enable = true;
			if (index < 0 || index >= lines.Length) return false;
			if (!force && ((slot.state != CustomSlot.State.NotStarted && slot.state != CustomSlot.State.Idle) || slot.isLocked)) return false;
			if (slot.config.firstLineAlwaysActive && index == 0) {
				if (lines[0].isLineEnabled) return false;
				enable = true;
			}

			lines[index].SwitchLine(enable, instant);
			currentIndex = index + (enable ? 1 : 0);
			slot.callbacks.onLineSwitch.Invoke(new LineInfo(lines[index]));
			return true;
		}

		/// <summary>
		/// Set all the line's state to either enabled or disabled.
		/// Only enabled lines will be checked when CustomSlot performs Hit Check.
		/// </summary>
		/// <param name="enable">true to enable, flase to disable</param>
		/// <param name="instant">when set to true, lines will be instantly enabled/disabled without animation</param>
		/// <param name="force">when set to false, lines' State will not change when the State is not Idle or if there's an active event in CustomSlot's event system</param>
		public void SwitchAllLines(bool enable, bool instant = false, bool force = true) {
			if (enable) for (int i = 0; i < lines.Length; i++) SwitchLine(i, enable, instant, force);
			else for (int i = lines.Length - 1; i >= 0; i--) SwitchLine(i, enable, instant, force);
			currentIndex = enable ? lines.Length : (slot.config.firstLineAlwaysActive ? 1 : 0);
		}

		public void EnableAllLines() { SwitchAllLines(true, false, false); }

		public void DisableAllLines() { SwitchAllLines(false, false, false); }

		public void EffectSwitchAllLines(bool? enable, bool instant) { foreach (Line line in lines) line.EffectSwitchLine(enable, instant); }
	}
}