using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace OculusSampleFramework
{
    // public class SphereController : Interactable
    public class SphereController : MonoBehaviour
    {
        [SerializeField] private GameObject _contactZone = null;
        [SerializeField] private GameObject _actionZone = null;

        // [SerializeField] private ContactTest[] _contactTests = null;

        // public enum ContactTest
        // {
        //     PerpenTest = 0,
        //     BackwardsPress
        // }

        public InteractableState CurrentSphereState { get; private set; } = InteractableState.Default;

        private Dictionary<InteractableTool, InteractableState> _toolToState =
            new Dictionary<InteractableTool, InteractableState>();

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }
}