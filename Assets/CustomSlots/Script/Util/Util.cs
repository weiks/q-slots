using System;
using DG.Tweening;
using UnityEngine;

namespace CSFramework {
	internal static class Extension {
		public static Sequence Append(this Sequence sequence, float duration, TweenCallback onStart = null, TweenCallback onComplete = null) { return sequence.Append(Util.Tween(duration, onStart, onComplete)); }

		public static Sequence Join(this Sequence sequence, float duration, TweenCallback onStart = null, TweenCallback onComplete = null) { return sequence.Join(Util.Tween(duration, onStart, onComplete)); }
	}

	public class Hide : PropertyAttribute {}

	public class Util {
		/// <summary>
		/// A shortcut method for creating DOTween Sequence.
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public static Sequence Sequence(float duration = 0, TweenCallback onStart = null, TweenCallback onComplete = null) {
			Sequence sequence = DOTween.Sequence();
			return sequence.Join(Tween(duration, onStart, onComplete));
		}

		/// <summary>
		/// A shortcut method for creating DOTween Tween.
		/// </summary>
		/// <param name="duration"></param>
		/// <returns></returns>
		public static Tweener Tween(float duration = 0, TweenCallback onStart = null, TweenCallback onComplete = null) {
			float i = 0;
			if (duration <= 0.01f) duration = 0.01f;
			Tweener tween = DOTween.To(() => i, x => i = x, 1, duration);
			if (onStart != null) tween.OnStart(onStart);
			if (onComplete != null) tween.OnComplete(onComplete);
			return tween;
		}

		public static Tweener Tween(TweenCallback onStart = null, TweenCallback onComplete = null) { return Tween(0, onStart, onComplete); }

		/// <summary>
		/// Ensures particles being emitted without weird behaviour.
		/// </summary>
		/// <param name="ps"></param>
		/// <param name="amount">numbers of emission</param>
		/// <returns></returns>
		public static ParticleSystem Emit(ParticleSystem ps, int amount) {
			ParticleSystem.EmissionModule emitter = ps.emission;
			emitter.enabled = true;
			ps.Emit(amount);
			emitter.enabled = false;
			return ps;
		}

		/// <summary>
		/// Converts a list of numbers(int) stored in a string to an array of ints. Returns null when parsing fails.
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static int[] StringToInts(string s) {
			if (s.EndsWith(",")) s = s.TrimEnd(',');
			try { return Array.ConvertAll<string, int>(s.Split(','), int.Parse); } catch {
				return null;
			}
		}

		/// <summary>
		/// Mainly used to instatiate a skin/template from an existing original(mold).
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="original"></param>
		/// <param name="parent">Will be the parent for the instantiated component.</param>
		/// <param name="pos">The initial (local)position of the instantiated component.</param>
		/// <param name="localPosition"></param>
		/// <returns></returns>
		public static T InstantiateAt<T>(T original, Transform parent, Vector3? pos = null, bool localPosition = true) where T : Component {
			T t = GameObject.Instantiate<T>(original);
			t.transform.SetParent(parent, false);
			if (pos != null)
				if (localPosition) t.transform.localPosition = pos ?? Vector3.zero;
				else t.transform.position = pos ?? Vector3.zero;
			return t;
		}

		public static T InstantiateAt<T>(T original, Transform parent, Transform target) where T : Component {
			T t = InstantiateAt<T>(original, parent);
			t.transform.position = target.transform.position;
			return t;
		}

		/// <summary>
		/// Immediately destroys all the child gameobjects the MonoBehaviour's transform have. Only its direct children will be destroyed.
		/// </summary>
		/// <param name="mono"></param>
		public static void DestroyChildren(Component component) { for (int i = component.transform.childCount - 1; i >= 0; i--) GameObject.DestroyImmediate(component.transform.GetChild(i).gameObject); }

		public static void DestroyChildren<T>(Component component) where T : Component {
			for (int i = component.transform.childCount - 1; i >= 0; i--) {
				Transform trans = component.transform.GetChild(i);
				T t = trans.GetComponent<T>();
				if (t) GameObject.DestroyImmediate(t.gameObject);
			}
		}

		public static int[] GetRandomizedIndexes(int length) {
			System.Random rng = new System.Random();
			int[] ints = new int[length];
			for (int i = 0; i < length; i++) ints[i] = i;
			int n = length;
			while (length > 1) {
				n--;
				int k = rng.Next(n + 1);
				int tmp = ints[k];
				ints[k] = ints[n];
				ints[n] = tmp;
			}
			return ints;
		}
	}
}