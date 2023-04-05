/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace OculusSampleFramework
{
	/// <summary>
	/// Poke tool used for near-field (touching) interactions. Assumes that it will be placed on
	/// finger tips.
	/// </summary>
	public class FingerTipPressTool : InteractableTool
	{
		private const int NUM_VELOCITY_FRAMES = 10;

		[SerializeField] private FingerTipPressToolView _fingerTipPressToolView = null;
		[SerializeField] private OVRPlugin.HandFinger _fingerToFollow = OVRPlugin.HandFinger.Index;

		[SerializeField] private LinearGaugeManager _gaugeManager = null;

		private readonly float[] PRESS_THRESHOLD = {0.08f, 0.16f};

		public override InteractableToolTags ToolTags
		{
			get
			{
				return InteractableToolTags.Poke;
			}
		}
		public override ToolInputState ToolInputState
		{
			get
			{
				return ToolInputState.Inactive;
			}
		}
		public override bool IsFarFieldTool
		{
			get
			{
				return false;
			}
		}

		public override bool EnableState
		{
			get
			{
				return _fingerTipPressToolView.gameObject.activeSelf;
			}
			set
			{
				_fingerTipPressToolView.gameObject.SetActive(value);
			}
		}

		private Vector3[] _velocityFrames;
		private int _currVelocityFrame = 0;
		private bool _sampledMaxFramesAlready;
		private Vector3 _position;

		private BoneCapsuleTriggerLogic[] _boneCapsuleTriggerLogic;

		private float _lastScale = 1.0f;
		private bool _isInitialized = false;
		private OVRBoneCapsule _capsuleToTrack;
		private OVRBoneCapsule _bc;

		public override void Initialize()
		{
			Assert.IsNotNull(_fingerTipPressToolView);

			InteractableToolsInputRouter.Instance.RegisterInteractableTool(this);
			_fingerTipPressToolView.InteractableTool = this;
			_gaugeManager = GameObject.Find("LinearGaugeManager").GetComponent<LinearGaugeManager>();

			_velocityFrames = new Vector3[NUM_VELOCITY_FRAMES];
			Array.Clear(_velocityFrames, 0, NUM_VELOCITY_FRAMES);

			StartCoroutine(AttachTriggerLogic());
		}

		private IEnumerator AttachTriggerLogic()
		{
			while (!HandsManager.Instance || !HandsManager.Instance.IsInitialized())
			{
				yield return null;
			}

			OVRSkeleton handSkeleton = IsRightHandedTool ? HandsManager.Instance.RightHandSkeleton : HandsManager.Instance.LeftHandSkeleton;

			OVRSkeleton.BoneId boneToTestCollisions = OVRSkeleton.BoneId.Hand_Pinky3;
			switch (_fingerToFollow)
			{
				case OVRPlugin.HandFinger.Thumb:
					boneToTestCollisions = OVRSkeleton.BoneId.Hand_Thumb3;
					break;
				case OVRPlugin.HandFinger.Index:
					boneToTestCollisions = OVRSkeleton.BoneId.Hand_Index3;
					break;
				case OVRPlugin.HandFinger.Middle:
					boneToTestCollisions = OVRSkeleton.BoneId.Hand_Middle3;
					break;
				case OVRPlugin.HandFinger.Ring:
					boneToTestCollisions = OVRSkeleton.BoneId.Hand_Ring3;
					break;
				default:
					boneToTestCollisions = OVRSkeleton.BoneId.Hand_Pinky3;
					break;
			}

			List<BoneCapsuleTriggerLogic> boneCapsuleTriggerLogic = new List<BoneCapsuleTriggerLogic>();
			List<OVRBoneCapsule> boneCapsules = HandsManager.GetCapsulesPerBone(handSkeleton, boneToTestCollisions);

			// finger tip should have only one capsule
			if (boneCapsules.Count > 0)
			{
				_capsuleToTrack = boneCapsules[0];
			}

			////// OSY: deep copy boneCapsule
			_bc = new OVRBoneCapsule();
			_bc.BoneIndex = _capsuleToTrack.BoneIndex;
			
			_bc.CapsuleRigidbody = new GameObject("modifiedCapsuleRigidbody").AddComponent<Rigidbody>();
			_bc.CapsuleRigidbody.mass = 1.0f;
            _bc.CapsuleRigidbody.isKinematic = true;
			_bc.CapsuleRigidbody.useGravity = false;
			_bc.CapsuleRigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

			GameObject orbGO = _capsuleToTrack.CapsuleRigidbody.gameObject;

			GameObject _rbGO = _bc.CapsuleRigidbody.gameObject;
			_rbGO.transform.SetParent(orbGO.transform.parent, false);
			_rbGO.transform.position = orbGO.transform.position;
			_rbGO.transform.rotation = orbGO.transform.rotation;

			_bc.CapsuleCollider = new GameObject("modifiedCapsuleCollider").AddComponent<CapsuleCollider>();

			_bc.CapsuleCollider.radius = _capsuleToTrack.CapsuleCollider.radius;
			_bc.CapsuleCollider.height = _capsuleToTrack.CapsuleCollider.height;
			_bc.CapsuleCollider.direction = _capsuleToTrack.CapsuleCollider.direction;
			_bc.CapsuleCollider.center = _capsuleToTrack.CapsuleCollider.center;

			GameObject occGO = _capsuleToTrack.CapsuleCollider.gameObject;

			GameObject _ccGO = _bc.CapsuleCollider.gameObject;
			_ccGO.transform.SetParent(_rbGO.transform, false);
			_ccGO.transform.localPosition = occGO.transform.localPosition;
			_ccGO.transform.localRotation = occGO.transform.localRotation;
			////// deep copy end

			var boneCapsuleTrigger = _bc.CapsuleRigidbody.gameObject.AddComponent<BoneCapsuleTriggerLogic>();
			_bc.CapsuleCollider.isTrigger = true;
			boneCapsuleTrigger.ToolTags = ToolTags;
			boneCapsuleTriggerLogic.Add(boneCapsuleTrigger);
		
		
			// foreach (var ovrCapsuleInfo in boneCapsules)
			// {
			// 	var boneCapsuleTrigger = ovrCapsuleInfo.CapsuleRigidbody.gameObject.AddComponent<BoneCapsuleTriggerLogic>();
			// 	ovrCapsuleInfo.CapsuleCollider.isTrigger = true;
			// 	boneCapsuleTrigger.ToolTags = ToolTags;
			// 	boneCapsuleTriggerLogic.Add(boneCapsuleTrigger);
			// }

			_boneCapsuleTriggerLogic = boneCapsuleTriggerLogic.ToArray();

			_isInitialized = true;
		}

		private void Update()
		{
			if (!HandsManager.Instance || !HandsManager.Instance.IsInitialized() || !_isInitialized || _capsuleToTrack == null)
			{
				return;
			}

			OVRHand hand = IsRightHandedTool ? HandsManager.Instance.RightHand : HandsManager.Instance.LeftHand;
			float currentScale = hand.HandScale;

			float forceScale = 0.5f;
			float forceOffset = 0.025f;

			// push tool into the tip based on how wide it is. so negate the direction
			Transform capsuleTransform = _capsuleToTrack.CapsuleCollider.transform;
			// NOTE: use time settings 0.0111111/0.02 to make collisions work correctly!
			Vector3 capsuleDirection = capsuleTransform.right;
			Vector3 capsuleTipPosition = capsuleTransform.position + _capsuleToTrack.CapsuleCollider.height * 0.5f
			  * capsuleDirection;

			//// OSY
			float scaledRadius = _fingerTipPressToolView.SphereRadius * forceScale;
			Vector3 toolSphereRadiusOffsetFromTip = currentScale * scaledRadius * capsuleDirection;
			// push tool back so that it's centered on transform/bone
			Vector3 toolPosition = capsuleTipPosition + toolSphereRadiusOffsetFromTip;

			Vector3 scale3D = new Vector3(forceScale, forceScale, forceScale);
			Vector3 toolOffset = capsuleDirection * forceOffset;

			// scale colliding capsule and tool
			_bc.CapsuleCollider.radius = scaledRadius;
			_bc.CapsuleCollider.height = scaledRadius * 2f;
			transform.localScale = scale3D;

			// make capsule tip trigger input
			_bc.CapsuleRigidbody.position = toolPosition + toolOffset; // + (scaledRadius - _bc.CapsuleCollider.height * 0.5f) * capsuleDirection;
			_bc.CapsuleCollider.transform.position = _bc.CapsuleRigidbody.position;
			_bc.CapsuleCollider.transform.rotation = capsuleTransform.rotation;

			transform.position = toolPosition + toolOffset;
			transform.rotation = _bc.CapsuleCollider.transform.rotation;
			InteractionPosition = toolPosition + toolOffset;
			//// OSY end

			UpdateAverageVelocity();

			CheckAndUpdateScale();

			// OSY MODIFICATION HERE

			if (_gaugeManager.GetHandForce() > PRESS_THRESHOLD[1])
			{
				transform.position += Vector3.Normalize(transform.right)*0.05f;
				transform.localScale *= 0.5f;
				_lastScale *= 0.5f;
			} else if (_gaugeManager.GetHandForce() > PRESS_THRESHOLD[0])
			{
				transform.position += Vector3.Normalize(transform.right)*0.025f;
				transform.localScale *= 0.8f;
				_lastScale *= 0.8f;
			}
			
		}

		private void UpdateAverageVelocity()
		{
			var prevPosition = _position;
			var currPosition = transform.position;
			var currentVelocity = (currPosition - prevPosition) / Time.deltaTime;
			_position = currPosition;
			_velocityFrames[_currVelocityFrame] = currentVelocity;
			// if sampled more than allowed, loop back toward the beginning
			_currVelocityFrame = (_currVelocityFrame + 1) % NUM_VELOCITY_FRAMES;

			Velocity = Vector3.zero;
			// edge case; when we first start up, we will have only sampled less than the
			// max frames. so only compute the average over that subset. After that, the
			// frame samples will act like an array that loops back toward to the beginning
			if (!_sampledMaxFramesAlready && _currVelocityFrame == NUM_VELOCITY_FRAMES - 1)
			{
				_sampledMaxFramesAlready = true;
			}

			int numFramesToSamples = _sampledMaxFramesAlready ? NUM_VELOCITY_FRAMES : _currVelocityFrame + 1;
			for (int frameIndex = 0; frameIndex < numFramesToSamples; frameIndex++)
			{
				Velocity += _velocityFrames[frameIndex];
			}

			Velocity /= numFramesToSamples;
		}

		private void CheckAndUpdateScale()
		{
			float currentScale = IsRightHandedTool ? HandsManager.Instance.RightHand.HandScale
				: HandsManager.Instance.LeftHand.HandScale;
			if (Mathf.Abs(currentScale - _lastScale) > Mathf.Epsilon)
			{
				transform.localScale = new Vector3(currentScale, currentScale, currentScale);
				_lastScale = currentScale;
			}
		}

		public override List<InteractableCollisionInfo> GetNextIntersectingObjects()
		{
			_currentIntersectingObjects.Clear();

			foreach (var boneCapsuleTriggerLogic in _boneCapsuleTriggerLogic)
			{
				var collidersTouching = boneCapsuleTriggerLogic.CollidersTouchingUs;
				foreach (ColliderZone colliderTouching in collidersTouching)
				{
					_currentIntersectingObjects.Add(new InteractableCollisionInfo(colliderTouching,
						colliderTouching.CollisionDepth, this));
				}
			}

			return _currentIntersectingObjects;
		}

		public override void FocusOnInteractable(Interactable focusedInteractable,
		  ColliderZone colliderZone)
		{
			// no need for focus
		}

		public override void DeFocus()
		{
			// no need for focus
		}
	}
}
