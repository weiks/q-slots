using UnityEditor;
using UnityEngine;

namespace CSFramework {
	[CustomPropertyDrawer(typeof (Hide))]
	internal class HideDrawer : PropertyDrawer {
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) { return 0f; }

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) { }
	}

	[CustomEditor(typeof (CustomSlot))]
	public class CustomSlotEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			CustomSlot t = target as CustomSlot;
			GUILayout.Space(20);
			if (GUILayout.Button("Refresh Layout")) {
				t.layout.Refresh();
				EditorUtility.SetDirty(t);
			}
			if (GUILayout.Button("Open SymbolGen Window")) {
				SymbolGenEditorWindow window = EditorWindow.GetWindow<SymbolGenEditorWindow>("SymbolGen");
				window.slot = t;
			}
			if (GUILayout.Button("Open Quick Tool Window")) {
				ShortcutToolEditorWindow window = EditorWindow.GetWindow<ShortcutToolEditorWindow>("Quick Tool");
				window.slot = t;
			}
		}
	}

	[CustomEditor(typeof (Reel))]
	public class ReelEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			Reel t = target as Reel;
			GUILayout.Space(20);
			if (GUILayout.Button("Refresh Reel")) {
				t.RefreshHolders();
				EditorUtility.SetDirty(t);
			}
		}
	}

	[CustomEditor(typeof (SymbolManager))]
	public class SymbolManagerEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			SymbolManager t = target as SymbolManager;

			if (GUILayout.Button("Sort")) {
				t.Sort();
			}
			if (GUILayout.Button("Add a new Symbol")) {
				Util.InstantiateAt<Symbol>(t.slot.skin.defaultSymbol, t.transform);
				EditorUtility.SetDirty(t);
			}
			EditorGUILayout.LabelField("Or simply press CTRL+d on an existing symbol to duplicate");
		}
	}

	[CustomEditor(typeof (LineManager))]
	public class LineManagerEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			LineManager t = target as LineManager;
			GUILayout.Space(20);
			if (GUILayout.Button("Add a new Line")) {
				Util.InstantiateAt<Line>(t.slot.skin.line, t.transform);
				EditorUtility.SetDirty(t);
			}
			if (GUILayout.Button("Generate Way Game Lines")) {
				if (EditorUtility.DisplayDialog("Warning", "This will destroy the existing lines the slot has and generates all the possible paylines. ", "Yes", "No!")) {
					t.CreateLinesForWayGame();
					EditorUtility.SetDirty(t);
				}
			}
			EditorGUILayout.LabelField("Or simply press CTRL+d on an existing line to duplicate");
		}
	}

	[CustomEditor(typeof (PayTableGen))]
	public class PayTableGenEditor : Editor {
		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			PayTableGen t = target as PayTableGen;
			GUILayout.Space(20);
			if (GUILayout.Button("Generate PayTable")) {
				t.Generate();
				EditorUtility.SetDirty(t);
			}
		}
	}

	public class CustomSlotEditorWindow : EditorWindow {
		public CustomSlot slot;

		protected void Update() {
			if (!Application.isPlaying && !slot) {
				if (Selection.activeGameObject && Selection.activeGameObject.GetComponentInParent<CustomSlot>()) Repaint();
			}
		}

		protected bool FindCustomSlot() {
			if (Selection.activeGameObject) {
				CustomSlot slotInTree = Selection.activeGameObject.GetComponentInParent<CustomSlot>();
				if (slotInTree) slot = slotInTree;
			}
			if (!slot) {
				EditorGUILayout.LabelField("Please select a hierarchy tree CustomSlot exists.");
				return false;
			}
			return true;
		}
	}
}