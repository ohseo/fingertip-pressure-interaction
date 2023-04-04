/************************************************************************************

Copyright (c) Facebook Technologies, LLC and its affiliates. All rights reserved.  

See SampleFramework license.txt for license terms.  Unless required by applicable law 
or agreed to in writing, the sample code is provided “AS IS” WITHOUT WARRANTIES OR 
CONDITIONS OF ANY KIND, either express or implied.  See the license for specific 
language governing permissions and limitations under the license.

************************************************************************************/

using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Visual of finger tip poke tool.
/// </summary>
namespace OculusSampleFramework
{
	public class FingerTipPressToolView : MonoBehaviour, InteractableToolView
	{
		[SerializeField] private MeshRenderer _sphereMeshRenderer = null;
		[SerializeField] private LineRenderer _lineRenderer = null;

		public InteractableTool InteractableTool { get; set; }

		public bool EnableState
		{
			get
			{
				return _sphereMeshRenderer.enabled;
			}
			set
			{
				_sphereMeshRenderer.enabled = value;
			}
		}

		public bool ToolActivateState { get; set; }

		public float SphereRadius { get; private set; }

		private Vector3[] linePositions = new Vector3[2];

		private void Awake()
		{
			Assert.IsNotNull(_sphereMeshRenderer);
			Assert.IsNotNull(_lineRenderer);
			SphereRadius = _sphereMeshRenderer.transform.localScale.z * 0.5f;
			_lineRenderer.positionCount = 2;
		}

		public void SetFocusedInteractable(Interactable interactable)
		{
			// nothing to see here
		}

		private void Update()
		{
			// var myPosition = InteractableTool.ToolTransform.position + InteractableTool.ToolTransform.localScale.x * 0.005f * InteractableTool.ToolTransform.forward;
			var myPosition = InteractableTool.InteractionPosition;
			var myForward = InteractableTool.ToolTransform.right;

			linePositions[0] = myPosition;
			linePositions[1] = myPosition + myForward;
			_lineRenderer.SetPositions(linePositions);
		}
	}
}
