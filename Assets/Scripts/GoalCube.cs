using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalCube : MonoBehaviour
{
    private const float OVERLAP_THRESHOLD = 0.01f;
    public UnityAction TriggerEnter;
    public UnityAction TriggerExit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collider)
    {
        TargetSphere targetSphere = collider.gameObject.GetComponent<TargetSphere>();
        if(targetSphere != null)
        {
            targetSphere.GoalIn();
        }
        TriggerEnter.Invoke();
    }

    public void OnTriggerExit(Collider collider)
    {
        TargetSphere targetSphere = collider.gameObject.GetComponent<TargetSphere>();
        if(targetSphere != null)
        {
            targetSphere.GoalOut();
        }
        TriggerExit.Invoke();
    }
}
