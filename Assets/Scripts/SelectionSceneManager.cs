using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Platform;
using Oculus.Platform.Models;
using OculusSampleFramework;
using System;
using Random=UnityEngine.Random;


public class SelectionSceneManager : ExpSceneManager
{
    private const float HORIZONTAL_RANGE_M = 0.2f;
    private const float VERTICAL_RANGE_M = 0.2f;
    private const float DEPTH_RANGE_M = 0.2f;
    private const float TARGET_POSITION_DEG = 0f;
    private const float TARGET_GRID_MARGIN = 0.01f;
    private const float VERTICAL_CENTER_OFFSET = 0.0f;
    private const int SEQUENCE_LENGTH = 1;
    private List<GameObject> _targets = new List<GameObject>();
    private List<GameObject> _internalTargets = new List<GameObject>();
    private int _expTargetIndex;
    private int _expTargetCount = 0;

    public override void StartTrial()
    {
        DestroyCenter();
        GenerateTargets();
        _trialDuration = 0f;
        _isTimeout = false;
        _isInTrial = true;
        _expTargetCount = 0;
    }

    public override void EndTrial()
    {
        foreach(GameObject target in _targets)
        {
            Destroy(target);
        }
        _internalTargets.Clear();
        _targets.Clear();
        LoadNewScene();
        _isTimeout = false;
        _isInTrial = false;
        _expTargetCount = 0;
    }

    public void NextTarget()
    {
        _expTargetCount++;
        if(_expTargetCount == SEQUENCE_LENGTH)
        {
            EndTrial();
            return;
        }
        _internalTargets[_expTargetIndex].GetComponent<TargetSphere>().IsExpTarget = false;
        _internalTargets.RemoveAt(_expTargetIndex);
        if (_internalTargets.Count > 0)
        {
            int r = Random.Range(0, _internalTargets.Count-1);
            _internalTargets[r].GetComponent<TargetSphere>().MakeExpTarget();
            _expTargetIndex = r;
            
        } else
        {
            _isTimeout = true;
        }
    }

    protected override void GenerateTargets()
    {
        if(_targets.Count != 0)
        {
            _targets.Clear();
        }
        if(_internalTargets.Count != 0)
        {
            _internalTargets.Clear();
        }

        Vector3 areaSize = new Vector3(HORIZONTAL_RANGE_M, VERTICAL_RANGE_M, DEPTH_RANGE_M);
        Vector3 targetPosition = new Vector3();
        targetPosition.x = Mathf.Sin(-TARGET_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth-areaSize.x/2f;
        targetPosition.y = Mathf.Sin(VERTICAL_CENTER_OFFSET * Mathf.Deg2Rad)*_targetDepth-areaSize.y/2f;
        targetPosition.z = Mathf.Cos(TARGET_POSITION_DEG * Mathf.Deg2Rad)*_targetDepth-areaSize.z/2f;

        Vector3 gridSize = new Vector3(areaSize.x/_numGrid.x, areaSize.y/_numGrid.y, areaSize.z/_numGrid.z);

        for (int i=0; i<_numGrid.x; i++)
        {
            for (int j=0; j<_numGrid.y; j++)
            {
                for (int k=0; k<_numGrid.z; k++)
                {        
                    float x = Random.Range(TARGET_GRID_MARGIN, gridSize.x-TARGET_GRID_MARGIN);
                    float y = Random.Range(TARGET_GRID_MARGIN, gridSize.y-TARGET_GRID_MARGIN);
                    float z = Random.Range(TARGET_GRID_MARGIN, gridSize.z-TARGET_GRID_MARGIN);
                    Vector3 pos = new Vector3(x+i*gridSize.x+targetPosition.x, //+(float)Math.Pow(-1.0,k+1)*gridSize.x/4f, 
                                                y+j*gridSize.y+targetPosition.y,
                                                z+k*gridSize.z+targetPosition.z);
                    GameObject sphere = Instantiate(targetSpherePrefab, pos, Quaternion.identity);
                    sphere.transform.localScale *= TARGET_SIZE;
                    _targets.Add(sphere);

                    // if(i>0 && i<_numGrid.x-1 && j>0 && j<_numGrid.y-1 && k>0 && k<_numGrid.z-1)
                    // if(i>0 && i<_numGrid.x-1 && j>0 && j<_numGrid.y-1 && k==0)
                    // {
                        _internalTargets.Add(sphere);
                    // }
                }
            }
        }

        int r = Random.Range(0, _internalTargets.Count-1);
        _internalTargets[r].GetComponent<TargetSphere>().IsExpTarget = true;        
        _expTargetIndex = r;
        _expTargetCount++;
    }
}
