using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCube : MonoBehaviour
{
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
            targetSphere.GoalOut();
        }
    }
}
