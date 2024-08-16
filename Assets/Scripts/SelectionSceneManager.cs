using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    private const float TARGET_GRID_MARGIN = 0.005f;
    private const float VERTICAL_CENTER_OFFSET = 0.0f;
    private const float TARGET_DEPTH_MARGIN = 0.05f;
    private const int SEQUENCE_LENGTH = 1;
    private List<GameObject> _targets = new List<GameObject>();
    private List<GameObject> _internalTargets = new List<GameObject>();
    private int _expTargetIndex;
    private GameObject _expTarget;
    private int _expTargetCount = 0;
    private FileStream _sceneFSWrite;
    private StreamWriter _sceneWriter;
    private string filePath;

    public void SetFilePath(string path)
    {
        filePath = path;
    }

    public override void StartTrial()
    {
        DestroyCenter();
        GenerateTargets();
        LogScene();
        _trialDuration = 0f;
        _isTimeout = false;
        _isInTrial = true;
        _expTargetCount = 0;
        Vector3[] v = {_expTarget.transform.position, Vector3.zero};
        _startTrialTrigger.Invoke(_currentSet, _currentTrial, _expTargetIndex, v);
    }

    public override void EndTrial()
    {
        _endTrialTrigger.Invoke(_trialDuration, _isTimeout.ToString(), Vector3.zero);
        foreach(GameObject target in _targets)
        {
            Destroy(target);
        }
        _internalTargets.Clear();
        _targets.Clear();
        _isTimeout = false;
        _isInTrial = false;
        _expTargetCount = 0;
        _currentTrial++;
        if(_currentTrial > MAX_TRIAL_NUM)
        {
            _text.text = "Set Completed";
            EndSet();
            return;
        }
        LoadNewScene();
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
                    Vector3 pos = new Vector3(x+i*gridSize.x+targetPosition.x,
                                                y+j*gridSize.y+targetPosition.y,
                                                z+k*gridSize.z+targetPosition.z);
                    GameObject sphere = Instantiate(targetSpherePrefab, pos, Quaternion.identity);
                    sphere.transform.localScale *= TARGET_SIZE;
                    sphere.GetComponent<TargetSphere>().targetIndex = _targets.Count;
                    _targets.Add(sphere);

                    // if(i>0 && i<_numGrid.x-1 && j>0 && j<_numGrid.y-1 && k>0 && k<_numGrid.z-1)
                    if(pos.z > targetPosition.z+TARGET_DEPTH_MARGIN && pos.z < targetPosition.z+DEPTH_RANGE_M-TARGET_DEPTH_MARGIN)
                    {
                        _internalTargets.Add(sphere);
                    }
                }
            }
        }

        int r = Random.Range(0, _internalTargets.Count-1);
        _internalTargets[r].GetComponent<TargetSphere>().MakeExpTarget();
        _expTargetIndex = _targets.FindIndex(x => GameObject.ReferenceEquals(x, _internalTargets[r]));
        _expTarget = _targets[_expTargetIndex];
        _expTargetCount++;
    }

    private void LogScene()
    {
        string sceneFullPath = filePath+$"_Set{_currentSet}_Trial{_currentTrial}_SceneData.csv";
        _sceneFSWrite = new FileStream(sceneFullPath, FileMode.Create, FileAccess.Write);
        _sceneWriter = new StreamWriter(_sceneFSWrite, System.Text.Encoding.UTF8);
        _sceneWriter.WriteLine("Target Index, Target x, Target y, Target z, Is Exp Target");
        
        for (int i=0; i<_targets.Count; i++)
        {
            List<string> values = new List<string>();

            values.Add(i.ToString());
            values.Add(_targets[i].transform.position.x.ToString());
            values.Add(_targets[i].transform.position.y.ToString());
            values.Add(_targets[i].transform.position.z.ToString());
            values.Add(_targets[i].GetComponent<TargetSphere>().IsExpTarget.ToString());

            _sceneWriter.WriteLine(String.Join(",", values.ToArray()));
        }

        _sceneWriter.Close();
        _sceneFSWrite.Close();
    }
}
