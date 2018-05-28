using System;
using DG.Tweening;

namespace CSFramework {
	/// <summary>
	/// Base Event class for CustomSlot's event system.
	/// Can be added by calling <see cref="CustomSlot.AddEvent(SlotEvent)"/> method.
	/// While an event is played, CustomSlot stops its progress and calls the Event's Update until
	/// the event is deactivated.
	/// </summary>
	public class SlotEvent {
		public bool isDeactivated = false;
		public Action<SlotEvent> onActivate;
		public Action onDeactivate;
		public SlotEvent(Action<SlotEvent> onActivate = null) { this.onActivate = onActivate; }

		public virtual void Activate() { if (onActivate != null) onActivate(this); }

		public virtual void Deactivate() {
			isDeactivated = true;
			if (onDeactivate != null) onDeactivate();
		}

		public virtual void Update() { }
	}

	/// <summary>
	/// A SlotEvent inherited class that plays DOTween Sequence on activation and deactivated when
	/// the sequence is complete.
	/// </summary>
	public class EventTweenSequence : SlotEvent {
		private Sequence sequence;

		public EventTweenSequence(Sequence sequence) {
			this.sequence = sequence;
			sequence.Pause();
		}

		public override void Update() {
			base.Update();
			if (!sequence.IsActive() || sequence.IsComplete()) Deactivate();
		}

		public override void Activate() {
			base.Activate();
			sequence.Play();
		}
	}
}