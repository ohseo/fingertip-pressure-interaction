using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCube : MonoBehaviour
{
    private const float OVERLAP_THRESHOLD = 0.01f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        TargetSphere targetSphere = collider.gameObject.GetComponent<TargetSphere>();
        if(targetSphere != null)
        {
            targetSphere.GoalIn();
        }
    }

    void OnTriggerExit(Collider collider)
    {
        TargetSphere targetSphere = collider.gameObject.GetComponent<TargetSphere>();
        if(targetSphere != null)
        {
            // bool overlap = Mathf.Abs((transform.position - targetSphere.transform.position).magnitude) < OVERLAP_THRESHOLD;
            // if(!overlap)
            // {
                targetSphere.GoalOut();
            // }
        }
    }
}
