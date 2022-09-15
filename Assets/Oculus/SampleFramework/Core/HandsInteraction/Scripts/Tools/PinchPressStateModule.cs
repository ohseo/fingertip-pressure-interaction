// Copyright (c) Facebook, Inc. and its affiliates. All Rights Reserved.

using UnityEngine;

namespace OculusSampleFramework
{
	/// <summary>
	/// Manages pinch state, including if an object is being focused via something
	/// like a ray (or not).
	/// </summary>
	public class PinchPressStateModule
	{
		private const float PINCH_STRENGTH_THRESHOLD = 1.0f;
		private const float PRESS_STRENGTH_THRESHOLD = 0.5f;

		private enum PinchPressState
		{
			None = 0,
			PinchDown,
			PinchStay,
			PinchUp,
			PressDown,
			PressStay,
			PressUp
		}

		private PinchPressState _currPinchState;
		private Interactable _firstFocusedInteractable;

		/// <summary>
		/// We want a pinch up and down gesture to be done **while** an object is focused.
		/// We don't want someone to pinch, unfocus an object, then refocus before doing
		/// pinch up. We also want to avoid focusing a different interactable during this process.
		/// While the latter is difficult to do since a person might focus nothing before
		/// focusing on another interactable, it's theoretically possible.
		/// </summary>
		public bool PinchUpAndDownOnFocusedObject
		{
			get
			{
				// return _currPinchState == PinchState.PinchUp && _firstFocusedInteractable != null;
				return _currPinchState == PinchPressState.PinchUp;
			}
		}

		public bool PinchSteadyOnFocusedObject
		{
			get
			{
				// return _currPinchState == PinchState.PinchStay && _firstFocusedInteractable != null;
				return _currPinchState == PinchPressState.PinchStay;
			}
		}

		public bool PinchDownOnFocusedObject
		{
			get
			{
				// return _currPinchState == PinchState.PinchDown && _firstFocusedInteractable != null;
				return _currPinchState == PinchPressState.PinchDown;
			}
		}

		public PinchPressStateModule()
		{
			_currPinchState = PinchPressState.None;
			_firstFocusedInteractable = null;
		}

		public void UpdateState(OVRHand hand, Interactable currFocusedInteractable, LinearGaugeManager gaugeManager)
		{
			float pinchStrength = hand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
			bool isPinching = Mathf.Abs(PINCH_STRENGTH_THRESHOLD - pinchStrength) < Mathf.Epsilon;
			float pressStrength = gaugeManager.GetHandForce();
			bool isPressing = Mathf.Abs(PRESS_STRENGTH_THRESHOLD - pressStrength) < Mathf.Epsilon;
			var oldPinchState = _currPinchState;

			switch (oldPinchState)
			{
				case PinchPressState.PinchUp:
					// can only be in pinch up for a single frame, so consider
					// next frame carefully
					if (isPinching)
					{
						_currPinchState = PinchPressState.PinchDown;
						if (currFocusedInteractable != _firstFocusedInteractable)
						{
							_firstFocusedInteractable = null;
						}
					}
					else
					{
						_currPinchState = PinchPressState.None;
						_firstFocusedInteractable = null;
					}
					break;
				case PinchPressState.PinchStay:
				case PinchPressState.PinchDown:
				case PinchPressState.PressUp:
					_currPinchState = isPressing ? PinchPressState.PressDown : PinchPressState.PinchStay;
					_currPinchState = isPinching ? PinchPressState.PinchStay : PinchPressState.PinchUp;
					// if object is not focused anymore, then forget it
					if (currFocusedInteractable != _firstFocusedInteractable)
					{
						_firstFocusedInteractable = null;
					}
					break;
				case PinchPressState.PressStay:
				case PinchPressState.PressDown:
					_currPinchState = isPressing ? PinchPressState.PressStay : PinchPressState.PressUp;
					if (currFocusedInteractable != _firstFocusedInteractable)
					{
						_firstFocusedInteractable = null;
					}
					break;
				default:
					if (isPinching)
					{
						_currPinchState = PinchPressState.PinchDown;
						// this is the interactable that must be focused through out the pinch up and down
						// gesture.
						_firstFocusedInteractable = currFocusedInteractable;
					}
					break;
			}
		}
	}
}
