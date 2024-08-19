using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeSceneManager : ExpSceneManager
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void StartTrial()
    {
        DestroyCenter();
        GenerateTargets();
        _isInTrial = true;
    }

    public override void EndTrial()
    {
        _isInTrial = false;
    }

    protected override void GenerateTargets()
    {
        GameObject target1 = Instantiate(targetSpherePrefab, new Vector3(0.1f, 0f, 1f), Quaternion.identity);
        target1.transform.localScale *= TARGET_SIZE;
        target1.GetComponent<TargetSphere>().IsExpTarget = true;

        GameObject target2 = Instantiate(targetSpherePrefab, new Vector3(-0.1f, 0f, 1f), Quaternion.identity);
        target2.transform.localScale *= TARGET_SIZE;
        target2.GetComponent<TargetSphere>().IsExpTarget = true;

        GameObject sphere = Instantiate(targetSpherePrefab, new Vector3(-0.105f, -0.01f, 0.99f), Quaternion.identity);
        sphere.transform.localScale *= TARGET_SIZE;
    }
}
