using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CSFramework {
	/// <summary>
	/// A class that controlls a line(Payline).
	/// Each Line has a set of data and a path which Hit Check will follow its trail.
	/// </summary>
	[ExecuteInEditMode]
	public class Line : MonoBehaviour, IComparable {
		public enum LoopMode {
			Stay,
			Continue,
			Loop,
			PingPong
		}

		[Tooltip("Specify an order which CS will use for various operations like determining which line a  hit check will be performed first. 0 will be the first like to be checked, following 1,2,3 and so on.")]
		public int order;
		[Tooltip("Specify the index of a row the line's path starts. ( 0 being the index of the top-most row )")]
		public int row;
		[Tooltip("Specify the path of a line ( like 0,0,1,0 )\n\n Each number represents the amount of row(s) moved from the last reel. Gizmo will draw the path on your scene view.")]
		public string _path = "0";
		[Tooltip("Specify how Hit Check continues when it reaches the end of the path\n\n[Stay] Hit Check will stay at the last row\n[Continue] Hit Check will refer the last path\n[Loop] The path loops from the start\n[PingPong] The path is reversed")]
		public LoopMode pathLoopMode;

		[Header("References")]
		public Image image;
		public CanvasGroup cg;
		public Text textIndex;

		[HideInInspector]
		public LineManager lineManager;
		[HideInInspector]
		public int[] paths;

		public CustomSlot slot { get { return lineManager.slot; } }
		public Color gradientColor { get { return lineManager.gradientColor.Evaluate(gradientPosition); } }
		public float gradientPosition { get { return (float) order/(lineManager.lines.Length + 1); } }

		public HitInfo hitInfo { get; protected set; }
		public bool isLineEnabled { get; protected set; }

		internal void Validate(LineManager lineManager) {
			this.lineManager = lineManager;
			SetPaths();
		}

		internal void OnGenInit() {
			SetPaths(0);
			isLineEnabled = true;
			hitInfo = new HitInfo(slot, this);
		}

		internal virtual void OnStartRound() { hitInfo = new HitInfo(slot, this); }

		internal void SwitchLine(bool enable, bool instant = false) {
			EffectSwitchLine(enable, instant);
			isLineEnabled = enable;
		}

		public virtual void EffectSwitchLine(bool? enable, bool instant) {
			DOTween.Complete(image);
			image.color = Color.white;
			if (enable ?? isLineEnabled) {
				cg.alpha = 1f;
				if (!instant) {
					slot.effects.lineSwitchEffect.Play(this);
				}
			} else {
				if (instant) cg.alpha = 0.5f;
				else cg.DOFade(0.5f, 0.3f);
			}
		}

		/// <summary>
		/// Draws a trail line for the given duration.
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public virtual Tweener DrawPath(float duration, Ease ease = Ease.InFlash) {
			SGLineRenderer sgl = Util.InstantiateAt<SGLineRenderer>(slot.skin.lineTrail, slot.mainScreen.transform, slot.layoutRow.transform.localPosition);
			List<Vector2> points = new List<Vector2>();
			float cx = slot.layout.sizeSymbol.x + slot.layout.spacingSymbol.x;
			float cy = slot.layout.sizeSymbol.y + slot.layout.spacingSymbol.y;
			for (int i = 0; i < paths.Length; i++) points.Add(new Vector2(i*cx + cx*0.5f, -paths[i]*cy - cy*0.5f));
			sgl.Points = points.ToArray();
			Color color = gradientColor;
			sgl.color = new Color(0, 0, 0, 0);
			sgl.transform.SetParent(slot.transform.parent);
			return DOTween.To(() => sgl.color, x => sgl.color = x, color, duration*0.5f).SetLoops(2, LoopType.Yoyo).SetEase(ease).OnComplete(() => { Destroy(sgl.gameObject); });
		}

		/// <summary>
		/// Highlights the line's icon for the given duration.
		/// The color of the highlighting effect can be changed via LineManager setting.
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public virtual Tweener HighlightLineIcon(float duration, Ease ease = Ease.Flash) { return image.DOColor(gradientColor, duration*0.5f).SetEase(ease).SetLoops(2, LoopType.Yoyo); }

		internal void SetPaths(int? _offset = null) {
			int offset = _offset ?? slot.config.hiddenTopRows;
			List<int> list = new List<int>();
			int revert = 1;
			int[] pathsToFollow = Util.StringToInts(_path);
			if (pathsToFollow == null) pathsToFollow = new int[] {0};
			int currentRow = row;
			int x = 0;
			for (int i = 0; i < slot.reels.Length; i++) {
				list.Add(currentRow + offset);
				currentRow += pathsToFollow[x]*revert;
				x++;
				if (x >= pathsToFollow.Length) {
					if (pathLoopMode == LoopMode.Continue) x--;
					else x = 0;
					if (pathLoopMode == LoopMode.PingPong) {
						Array.Reverse(pathsToFollow);
						revert *= -1;
					}
					if (pathLoopMode == LoopMode.Stay) pathsToFollow = new int[] {0};
				}
			}
			paths = list.ToArray();
		}

		internal SymbolHolder[] GetHoldersOnPath() {
			if (paths == null) return null;
			List<SymbolHolder> list = new List<SymbolHolder>();
			for (int x = 0; x < slot.reels.Length; x++) {
				if (x >= paths.Length) break;
				int currentRow = paths[x];
				Reel reel = slot.reels[x];
				if (reel == null) break;
				if (!slot.config.isRowValid(currentRow)) break;
				if(slot.rows.Length>currentRow)list.Add(slot.rows[currentRow].holders[x]);
			}
			return list.ToArray();
		}

		public int CompareTo(object obj) { return this.order - (obj as Line).order; }

#if UNITY_EDITOR
		[HideInInspector]
		public int lastIndex;
		[HideInInspector]
		public int lastRow;
		[HideInInspector]
		public string lastPath;
		[HideInInspector]
		public Transform lastParent;
		[HideInInspector]
		public LoopMode lastLoopMode;

		private void OnEnable() {
			if (Application.isPlaying) return;
			lastParent = null;
			EditorValidate();
		}

		private void Update() {
			if (Application.isPlaying) return;
			EditorValidate();
		}

		private void EditorValidate() {
			LineManager lineManagerinParent = GetComponentInParent<LineManager>();
			if (lineManager != lineManagerinParent) Validate(lineManagerinParent);
			if (!lineManager) return;

			if (lastIndex != order || lastRow != row || lastParent != transform.parent || _path != lastPath || lastLoopMode != pathLoopMode) {
				if (lineManager.autoNameGameobject) name = "#" + order + " (Row " + row + ")";
				if (lineManager.autoSetTextNumber) textIndex.text = "" + (order + (lineManager.incrementTextNumber ? 1 : 0));
				SetPaths();
				lastIndex = order;
				lastRow = row;
				lastParent = transform.parent;
				lastPath = _path;
				lastLoopMode = pathLoopMode;
			}
		}

		private void OnDrawGizmosSelected() {
			if (Application.isPlaying || !lineManager || !lineManager.drawGizmo || !gameObject.activeInHierarchy) return;
			EditorValidate();

			SymbolHolder[] holders = GetHoldersOnPath();
			if (holders == null) return;
			Gizmos.color = (holders.Length != slot.reels.Length) ? new Color(1, 0, 0, 1) : lineManager.gizmoColor;

			Vector3 lastPos = Vector3.zero;
			foreach (SymbolHolder holder in holders) {
				if (holder == null) return;
				Vector3 pos = holder.transform.position;
				if (holders.Length == 1) lastPos = holder.transform.position + new Vector3(10, 0);
				if (lastPos != Vector3.zero) {
					for (int j = 0; j < 2; j++) {
						Vector3 fix = new Vector3(0, j, 0);
						Gizmos.DrawLine(lastPos + fix, pos + fix);
					}
				}
				lastPos = pos;
			}
		}
#endif
	}
}