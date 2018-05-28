using UnityEditor;
using UnityEngine;

namespace CSFramework {
	public class ShortcutToolEditorWindow : CustomSlotEditorWindow {
		[MenuItem("Window/Custom Slots/Quick Tool")]
		private static void Open() { GetWindow<ShortcutToolEditorWindow>("Quick Tool"); }

		private void OnGUI() {
			if (!FindCustomSlot()) return;
			GUILayout.Space(20);

			EditorGUILayout.LabelField("Quick Tool (" + slot.name + ")");
			GUILayout.Space(10);
			if (GUILayout.Button("Refresh Layout")) {
				slot.layout.Refresh();
				EditorUtility.SetDirty(slot);
			}
			SymbolGen gen = slot.symbolGen;

			GUILayout.Space(10);
			if (GUILayout.Button("Open SymbolGen Window")) {
				SymbolGenEditorWindow window = GetWindow<SymbolGenEditorWindow>("SymbolGen");
				window.slot = slot;
			}
			GUILayout.Space(10);
			if (GUILayout.Button("Generate new Loadout")) {
				if (gen.setting.confirmGeneration && !EditorUtility.DisplayDialog("Warning", "Generating new loadout will replace the current symbol loadout. ", "Yes", "No!")) { } else {
					gen.Generate();
				}
			}

			PayTableGen ptg = slot.GetComponent<PayTableGen>();
			if (ptg) {
				GUILayout.Space(10);
				if (GUILayout.Button("Generate PayTable")) {
					ptg.Generate();
					EditorUtility.SetDirty(ptg);
				}
			}
		}
	}
}