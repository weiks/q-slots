using System;
using System.Collections;
using Elona;
using UnityEngine;
using UnityEngine.UI;

namespace CSFramework {
	public class DemoMenu : MonoBehaviour {
		[Serializable]
		public class DemoCanvas {
			public Canvas canvas;
		}

		public DemoCanvas[] list;
		private DemoCanvas current;
		public Transform menu;
		public Image imageLanguage;
		public Sprite spriteEN, spriteJP;
		public Text textFPS;
		public bool showFPS;
		private int FramesPerSec;
		private float frequency = 1.0f;
		private string fps;

		private void Awake() {
			foreach (DemoCanvas item in list) if (item.canvas.gameObject.activeSelf) current = item;
			menu.gameObject.SetActive(false);
			if (showFPS) StartCoroutine(FPS());
		}

		public void ToggleMenu() { menu.gameObject.SetActive(!menu.gameObject.activeSelf); }

		public void ToggleLanguage() {
			Lang.ToggleLanguage();
			imageLanguage.sprite = Lang.current == Lang.ID.EN ? spriteEN : spriteJP;
		}

		public void SwitchSlot(int index) {
			if (current != list[index]) {
				current.canvas.gameObject.SetActive(false);
				current = list[index];
				current.canvas.gameObject.SetActive(true);
			}
			menu.gameObject.SetActive(false);
		}

		private IEnumerator FPS() {
			for (;;) {
				int lastFrameCount = Time.frameCount;
				float lastTime = Time.realtimeSinceStartup;
				yield return new WaitForSeconds(frequency);
				float timeSpan = Time.realtimeSinceStartup - lastTime;
				int frameCount = Time.frameCount - lastFrameCount;
				textFPS.text = string.Format("FPS: {0}", Mathf.RoundToInt(frameCount/timeSpan));
			}
		}
	}
}