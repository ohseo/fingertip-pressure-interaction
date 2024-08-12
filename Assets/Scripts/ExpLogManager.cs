using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ExpLogManager : MonoBehaviour
{
    private ExpSceneManager _expSceneManager;
    private string baseDirectoryPath;
    private string dataDirectoryPath;
    private string fileName;
    private FileStream _fsWrite;
    private StreamWriter _streamWriter;
    private string[] _eventLogHeaderTask1 = new string[12]{"Event Name", "Time Stamp", "Trial Duration", "Input State",
                                                    "Hit Index", "Hit x", "Hit y", "Hit z",
                                                    "Is Exp Target", "Hit Point x", "Hit Point y", "Hit Point z"};
    private long _timeStamp;
    private float _trialDuration;
    private string _inputState;
    private int _hitIndex;
    private Vector3 _hitPosition;
    private bool _isExpTarget;
    private Vector3 _hitPoint;

    // Start is called before the first frame update
    void Start()
    {
        baseDirectoryPath = Application.dataPath;
        dataDirectoryPath = "/Data/";
        CreateNewFile();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy()
    {
        _streamWriter.Close();
    }

    public void SetExpSceneManager(ExpSceneManager esm)
    {
        _expSceneManager = esm;
        _expSceneManager.RegisterForStartEvent(new UnityAction(OnTrialStart));
        _expSceneManager.RegisterForEndEvent(new UnityAction<float, bool>(OnTrialEnd));
    }

    public void CreateNewFile()
    {
        string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")+"_EventLog.csv";
        string fullPath = baseDirectoryPath+dataDirectoryPath+fileName;

        _fsWrite = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        _streamWriter = new StreamWriter(_fsWrite, System.Text.Encoding.UTF8);
        _streamWriter.WriteLine(string.Join(",", _eventLogHeaderTask1));
    }

    public void OnTrialStart()
    {
        DateTimeOffset dt = new DateTimeOffset(DateTime.Now);
        _streamWriter.Write("Trial Start, ");
        _streamWriter.Write(dt.ToUnixTimeMilliseconds().ToString());
        _streamWriter.WriteLine("");
        Debug.Log("Logger: trial start event called:"+dt.ToUnixTimeMilliseconds().ToString());
    }

    public void OnTrialEnd(float trialDuration, bool isTimeOut)
    {
        DateTimeOffset dt = new DateTimeOffset(DateTime.Now);
        _streamWriter.Write("Trial End, ");
        _streamWriter.Write(dt.ToUnixTimeMilliseconds().ToString());
        _streamWriter.Write(", ");
        _streamWriter.Write(trialDuration.ToString());
        _streamWriter.WriteLine("");
        Debug.Log("Logger: trial end event called:"+dt.ToUnixTimeMilliseconds()+"\t"+trialDuration.ToString()+"\t"+isTimeOut.ToString());
    }
    
    public void OnInputStateChange()
    {

    }

    public void OnTargetStateChange()
    {

    }

    public string GenerateStringArray(string eventName)
    {
        List<string> values = new List<string>();
        values.Add(eventName);
        DateTimeOffset dt = new DateTimeOffset(DateTime.Now);
        values.Add(dt.ToUnixTimeMilliseconds().ToString());
        // values.Add(trialDuration.ToString());
        return String.Join(",", values.ToArray());
    }
}
