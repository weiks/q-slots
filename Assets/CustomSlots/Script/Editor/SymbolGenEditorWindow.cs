using UnityEngine;
using System.Collections;
using UnityEditor;

namespace CSFramework {
	public class SymbolGenEditorWindow : CustomSlotEditorWindow {
		[MenuItem("Window/Custom Slots/SymbolGen")]
		private static void Open() { GetWindow<SymbolGenEditorWindow>("SymbolGen"); }

		private Vector2 scrollPos;

		private void OnGUI() {
			if (!FindCustomSlot()) return;

			GUILayout.Space(20);
			SymbolGen gen = slot.symbolGen;
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

			EditorGUILayout.LabelField("SymbolGen (" + slot.name + ")");

			SerializedObject so = new SerializedObject(slot);
			EditorGUILayout.PropertyField(so.FindProperty("symbolGen").FindPropertyRelative("setting"), true);

			so.ApplyModifiedProperties();

			GUILayout.Space(20);
			if (GUILayout.Button("Generate new Loadout")) {
				if (gen.setting.confirmGeneration && !EditorUtility.DisplayDialog("Warning", "Generating new loadout will replace the current symbol loadout. ", "Yes", "No!")) { } else {
					gen.Generate();
					gen.Simulate();
				}
			}
			if (!Application.isPlaying) {
				GUILayout.Space(10);
				if (GUILayout.Button("Try / Simulate a Game")) gen.Simulate();
				GUILayout.Space(20);
				if (gen.log != null) {
					EditorGUILayout.LabelField(gen.log.name);
					EditorGUILayout.TextArea(gen.log.summary);
					GUILayout.Space(20);
				}
			}
			EditorGUILayout.EndScrollView();
		}
	}
}