/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace OculusSampleFramework
{
	/// <summary>
	/// Ray tool used for far-field interactions.
	/// </summary>
	public class RayPressTool : InteractableTool
	{
		private const float MINIMUM_RAY_CAST_DISTANCE = 0.8f;
		private const float COLLIDER_RADIUS = 0.01f;
		private const int NUM_MAX_PRIMARY_HITS = 10;
		private const int NUM_MAX_SECONDARY_HITS = 25;
		private const int NUM_COLLIDERS_TO_TEST = 20;
		private readonly float[] CD_GAIN = {0.5f, 0.25f};
		private const float PRESS_LOWER_THRESHOLD = 0.07f;

		private const int RELATIVE_MODE = 4;

		[SerializeField] private RayPressToolView _rayToolView = null;
		[Range(0.0f, 45.0f)] [SerializeField] private float _coneAngleDegrees = 20.0f;
		[SerializeField] private float _farFieldMaxDistance = 5f;
		[SerializeField] private LinearGaugeManager _gaugeManager = null;

		private TextMeshProUGUI _text;

		private bool hasPinchDownSaved = false;
		private Vector3 pinchDownPosition;
		private Vector3 pinchDownForward;
		
		private Vector3 prevPointingPosition, prevPointingForward, prevResultPosition, prevResultForward;

		public override InteractableToolTags ToolTags
		{
			get
			{
				return InteractableToolTags.Ray;
			}
		}

		private PinchPressStateModule _pinchStateModule = new PinchPressStateModule();
		private Interactable _focusedInteractable;

		//Only 4 states are defined in ToolInputState
		public override ToolInputState ToolInputState
		{
			get
			{
				if (_pinchStateModule.PressDownOnFocusedObject)
				{
					return ToolInputState.PrimaryInputDown;
				}
				if (_pinchStateModule.PressSteadyOnFocusedObject)
				{
					return ToolInputState.PrimaryInputDownStay;
				}
				if (_pinchStateModule.PressUpOnFocusedObject)
				{
					return ToolInputState.PrimaryInputUp;
				}

				return ToolInputState.Inactive;
			}
		}

		public override bool IsFarFieldTool
		{
			get { return true; }
		}

		public override bool EnableState
		{
			get
			{
				return _rayToolView.EnableState;
			}
			set
			{
				_rayToolView.EnableState = value;
			}
		}

		private Collider[] _collidersOverlapped = new Collider[NUM_COLLIDERS_TO_TEST];

		private Interactable _currInteractableCastedAgainst = null;
		private float _coneAngleReleaseDegrees;

		private RaycastHit[] _primaryHits = new RaycastHit[NUM_MAX_PRIMARY_HITS];
		private Collider[] _secondaryOverlapResults = new Collider[NUM_MAX_SECONDARY_HITS];
		private bool _initialized = false;

		public override void Initialize()
		{
			Assert.IsNotNull(_rayToolView);
			InteractableToolsInputRouter.Instance.RegisterInteractableTool(this);
			_rayToolView.InteractableTool = this;
			_gaugeManager = GameObject.Find("LinearGaugeManager").GetComponent<LinearGaugeManager>();
			_coneAngleReleaseDegrees = _coneAngleDegrees * 1.2f;
			_initialized = true;
			_text = GameObject.Find("Canvas/pressState").GetComponent<TextMeshProUGUI>();
		}

		private void OnDestroy()
		{
			if (InteractableToolsInputRouter.Instance != null)
			{
				InteractableToolsInputRouter.Instance.UnregisterInteractableTool(this);
			}
		}

		private void Update()
		{
			if (!HandsManager.Instance || !HandsManager.Instance.IsInitialized() || !_initialized)
			{
				return;
			}

			var hand = IsRightHandedTool ? HandsManager.Instance.RightHand : HandsManager.Instance.LeftHand;
			var skeleton = IsRightHandedTool ? HandsManager.Instance.RightHandSkeleton : HandsManager.Instance.LeftHandSkeleton;
			var pointer = hand.PointerPose;
			var centerEye = GameObject.Find("CenterEyeAnchor").transform;
			var shoulder = centerEye.position - new Vector3(0.1f, 0.25f, 0f);
			var wrist = skeleton.Bones[(int)OVRPlugin.BoneId.Hand_WristRoot].Transform.position;
			// var midpoint = (skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Thumb3].Transform.position + skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index3].Transform.position)/2;
			var midpoint = skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index1].Transform.position
				+ skeleton.Bones[(int)OVRPlugin.BoneId.Hand_Index1].Transform.up.normalized * 0.05f;
			var forearmpoint = wrist * 0.5f + shoulder * 0.5f;
			var forward = Vector3.Normalize(midpoint - forearmpoint);

			if(IsRightHandedTool)
			{
				transform.position = pointer.position;
				transform.rotation = pointer.rotation;
			}else{
				transform.position = midpoint;
				transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
			}

			var prevPosition = InteractionPosition;
			var currPosition = transform.position;
			Velocity = (currPosition - prevPosition) / Time.deltaTime;
			InteractionPosition = currPosition;

			_pinchStateModule.UpdateState(hand, _focusedInteractable, _gaugeManager);
			// _pinchStateModule.UpdateState(hand, _focusedInteractable);
			_rayToolView.RelativeActivateState = !_pinchStateModule.NotPinching;
			_rayToolView.ToolActivateState = _pinchStateModule.PressSteadyOnFocusedObject ||
				_pinchStateModule.PressDownOnFocusedObject;
			// _rayToolView.ToolActivateState = true;

			// OSY RAY MODIFICATION HERE
			// if(RELATIVE_MODE == 1)
			// {
			// 	if(_pinchStateModule.IsPinchDown)
			// 	{
			// 		pinchDownPosition = transform.position;
			// 		pinchDownForward = transform.forward;
			// 		hasPinchDownSaved = true;
			// 	} else if(!_pinchStateModule.NotPinching && !_pinchStateModule.IsPinchDown)
			// 	{
			// 		if(hasPinchDownSaved)
			// 		{
			// 			var newForward = transform.forward * CD_GAIN + pinchDownForward * (1-CD_GAIN);
			// 			newForward = Vector3.Normalize(newForward);
			// 			transform.forward = newForward;
			// 			transform.position = pinchDownPosition;
			// 		}
			// 	} else
			// 	{
			// 		hasPinchDownSaved = false;
			// 	}
			// } else if (RELATIVE_MODE == 2)
			// {
			// 	if(_pinchStateModule.IsPinchDown)
			// 	{
			// 		pinchDownPosition = transform.position;
			// 		pinchDownForward = transform.forward;
			// 		hasPinchDownSaved = true;
			// 	} else if(!_pinchStateModule.NotPinching && !_pinchStateModule.IsPinchDown)
			// 	{
			// 		if(hasPinchDownSaved)
			// 		{
			// 			var newPosition = transform.position * CD_GAIN + pinchDownPosition * (1-CD_GAIN);
			// 			transform.position = newPosition;
			// 			transform.forward = pinchDownForward;
			// 		}
			// 	} else
			// 	{
			// 		hasPinchDownSaved = false;
			// 	}
			// } else if (RELATIVE_MODE == 3)
			// {
			// 	if(_pinchStateModule.IsPinchDown)
			// 	{
			// 		pinchDownPosition = transform.position;
			// 		pinchDownForward = transform.forward;
			// 		hasPinchDownSaved = true;
			// 	} else if(!_pinchStateModule.NotPinching && !_pinchStateModule.IsPinchDown)
			// 	{
			// 		if(hasPinchDownSaved)
			// 		{
			// 			var newPosition = transform.position * CD_GAIN + pinchDownPosition * (1-CD_GAIN);
			// 			var newForward = transform.forward * CD_GAIN + pinchDownForward * (1-CD_GAIN);
			// 			newForward = Vector3.Normalize(newForward);
			// 			transform.position = newPosition;
			// 			transform.forward = newForward;
			// 		}
			// 	} else
			// 	{
			// 		hasPinchDownSaved = false;
			// 	}
			// } else if (RELATIVE_MODE == 4)
			// {
				if(_pinchStateModule.IsPinchDown)
				{
					prevPointingPosition = transform.position;
					prevPointingForward = transform.forward;
					prevResultPosition = transform.position;
					prevResultForward = transform.forward;
					pinchDownPosition = transform.position;
					pinchDownForward = transform.forward;
					hasPinchDownSaved = true;
				} else if(!_pinchStateModule.NotPinching && !_pinchStateModule.IsPinchDown)
				{
					if(hasPinchDownSaved)
					{
						var cdgain = 0f;
						if(_pinchStateModule.pressStrength < PRESS_LOWER_THRESHOLD)
						{
							cdgain = CD_GAIN[0];
						} else {
							cdgain = CD_GAIN[1];
						}
						var newPosition = (transform.position - prevPointingPosition) * cdgain + prevResultPosition;
						var newForward = (transform.forward - prevPointingForward) * cdgain + prevResultForward;
						newForward = Vector3.Normalize(newForward);
						prevPointingPosition = transform.position;
						prevPointingForward = transform.forward;
						prevResultPosition = newPosition;
						prevResultForward = newForward;
						transform.position = newPosition;
						transform.forward = newForward;
					}
				} else
				{
					hasPinchDownSaved = false;
				}
			// }

			// if(!_pinchStateModule.NoPress)
			// {
			// 	//test code
			// 	transform.rotation = pointer.rotation * Quaternion.Euler(0,-90,0);
			// }

			_text.text = _pinchStateModule.currentPinchState;

		}

		/// <summary>
		/// Avoid hand collider during raycasts so move origin some distance away from where tool is.
		/// </summary>
		/// <returns>Proper raycast origin.</returns>
		private Vector3 GetRayCastOrigin()
		{
			return transform.position + MINIMUM_RAY_CAST_DISTANCE * transform.forward;
		}

		public override List<InteractableCollisionInfo> GetNextIntersectingObjects()
		{
			if (!_initialized)
			{
				return _currentIntersectingObjects;
			}

			// if we already have focused on something, keep it until the angle between
			// our forward direction and object vector becomes too large
			if (_currInteractableCastedAgainst != null &&
				HasRayReleasedInteractable(_currInteractableCastedAgainst))
			{
				// reset state
				_currInteractableCastedAgainst = null;
			}

			// Find target interactable if we haven't found one before.
			if (_currInteractableCastedAgainst == null)
			{
				_currentIntersectingObjects.Clear();
				_currInteractableCastedAgainst = FindTargetInteractable();

				// If we have found one, query collision zones.
				if (_currInteractableCastedAgainst != null)
				{
					var targetHitPoint = _currInteractableCastedAgainst.transform.position;
					int numHits = Physics.OverlapSphereNonAlloc(targetHitPoint, COLLIDER_RADIUS, _collidersOverlapped);

					// find all colliders encountered; focus only on ones belonging to target element
					for (int i = 0; i < numHits; i++)
					{
						Collider colliderHit = _collidersOverlapped[i];
						var colliderZone = colliderHit.GetComponent<ColliderZone>();
						if (colliderZone == null)
						{
							continue;
						}

						Interactable interactableComponent = colliderZone.ParentInteractable;
						if (interactableComponent == null || interactableComponent
							!= _currInteractableCastedAgainst)
						{
							continue;
						}

						InteractableCollisionInfo collisionInfo = new InteractableCollisionInfo(colliderZone,
							colliderZone.CollisionDepth, this);
						_currentIntersectingObjects.Add(collisionInfo);
					}

					// clear intersecting object if no collisions were found
					if (_currentIntersectingObjects.Count == 0)
					{
						_currInteractableCastedAgainst = null;
					}
				}
			}

			return _currentIntersectingObjects;
		}

		private bool HasRayReleasedInteractable(Interactable focusedInteractable)
		{
			var ourPosition = transform.position;
			var forwardDirection = transform.forward;
			var hysteresisDotThreshold = Mathf.Cos(_coneAngleReleaseDegrees * Mathf.Deg2Rad);
			var vectorToFocusedObject = focusedInteractable.transform.position - ourPosition;
			vectorToFocusedObject.Normalize();
			var hysteresisDotProduct = Vector3.Dot(vectorToFocusedObject, forwardDirection);
			return hysteresisDotProduct < hysteresisDotThreshold;
		}

		/// <summary>
		/// Find all objects from primary ray cast or if that fails, all objects in a
		/// cone around main ray direction via a "secondary" cast.
		/// </summary>
		private Interactable FindTargetInteractable()
		{
			var rayOrigin = GetRayCastOrigin();
			var rayDirection = transform.forward;
			Interactable targetInteractable = null;

			// attempt primary ray cast
			targetInteractable = FindPrimaryRaycastHit(rayOrigin, rayDirection);

			// if primary cast fails, try secondary cone test
			if (targetInteractable == null)
			{
				targetInteractable = FindInteractableViaConeTest(rayOrigin, rayDirection);
			}

			return targetInteractable;
		}

		/// <summary>
		/// Find first hit that is supports our tool's method of interaction.
		/// </summary>
		private Interactable FindPrimaryRaycastHit(Vector3 rayOrigin, Vector3 rayDirection)
		{
			Interactable interactableCastedAgainst = null;

			// hit order not guaranteed, so find closest
			int numHits = Physics.RaycastNonAlloc(new Ray(rayOrigin, rayDirection), _primaryHits, Mathf.Infinity);
			float minDistance = 0.0f;
			for (int hitIndex = 0; hitIndex < numHits; hitIndex++)
			{
				RaycastHit raycastHit = _primaryHits[hitIndex];

				// continue if something occludes it and that object is not an interactable
				var currentHitColliderZone = raycastHit.transform.GetComponent<ColliderZone>();
				if (currentHitColliderZone == null)
				{
					continue;
				}

				// at this point we have encountered an interactable. Only consider it if
				// it allows interaction with our tool. Otherwise ignore it.
				Interactable currentInteractable = currentHitColliderZone.ParentInteractable;
				if (currentInteractable == null || (currentInteractable.ValidToolTagsMask & (int)ToolTags) == 0)
				{
					continue;
				}

				var vectorToInteractable = currentInteractable.transform.position - rayOrigin;
				var distanceToInteractable = vectorToInteractable.magnitude;
				if (interactableCastedAgainst == null || distanceToInteractable < minDistance)
				{
					interactableCastedAgainst = currentInteractable;
					minDistance = distanceToInteractable;
				}
			}

			return interactableCastedAgainst;
		}

		/// <summary>
		/// If primary cast fails, try secondary test to see if we can target an interactable.
		/// This target has to be far enough and support our tool via appropriate
		/// tags, and must be within a certain angle from our primary ray direction.
		/// </summary>
		/// <param name="rayOrigin">Primary ray origin.</param>
		/// <param name="rayDirection">Primary ray direction.</param>
		/// <returns>Interactable found, if any.</returns>
		private Interactable FindInteractableViaConeTest(Vector3 rayOrigin, Vector3 rayDirection)
		{
			Interactable targetInteractable = null;

			float minDistance = 0.0f;
			float minDotProductThreshold = Mathf.Cos(_coneAngleDegrees * Mathf.Deg2Rad);
			// cone extends from center line, where angle is split between top and bottom half
			float halfAngle = Mathf.Deg2Rad * _coneAngleDegrees * 0.5f;
			float coneRadius = Mathf.Tan(halfAngle) * _farFieldMaxDistance;

			int numColliders = Physics.OverlapBoxNonAlloc(
				rayOrigin + rayDirection * _farFieldMaxDistance * 0.5f, // center
				new Vector3(coneRadius, coneRadius, _farFieldMaxDistance * 0.5f), //half extents
				_secondaryOverlapResults, transform.rotation);

			for (int i = 0; i < numColliders; i++)
			{
				Collider colliderHit = _secondaryOverlapResults[i];
				// skip invalid colliders
				var colliderZone = colliderHit.GetComponent<ColliderZone>();
				if (colliderZone == null)
				{
					continue;
				}

				// skip invalid components
				Interactable interactableComponent = colliderZone.ParentInteractable;
				if (interactableComponent == null ||
					(interactableComponent.ValidToolTagsMask & (int)ToolTags) == 0)
				{
					continue;
				}

				var vectorToInteractable = interactableComponent.transform.position - rayOrigin;
				var distanceToInteractable = vectorToInteractable.magnitude;
				vectorToInteractable /= distanceToInteractable;
				var dotProduct = Vector3.Dot(vectorToInteractable, rayDirection);
				// must be inside cone!
				if (dotProduct < minDotProductThreshold)
				{
					continue;
				}

				if (targetInteractable == null || distanceToInteractable < minDistance)
				{
					targetInteractable = interactableComponent;
					minDistance = distanceToInteractable;
				}
			}

			return targetInteractable;
		}

		public override void FocusOnInteractable(Interactable focusedInteractable,
		  ColliderZone colliderZone)
		{
			_rayToolView.SetFocusedInteractable(focusedInteractable);
			_focusedInteractable = focusedInteractable;
		}

		public override void DeFocus()
		{
			_rayToolView.SetFocusedInteractable(null);
			_focusedInteractable = null;
		}
	}
}
