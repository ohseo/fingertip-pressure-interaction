using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ExpLogManager : MonoBehaviour
{
    public int _taskNum { get; set; }
    private ExpSceneManager _expSceneManager;
    private string baseDirectoryPath;
    private string dataDirectoryPath;
    private string fileName;
    private FileStream _fsWrite;
    private StreamWriter _streamWriter;
    private string[] _eventLogHeaderTask1 = new string[16]{"Set Number", "Trial Number", "Event Name", "Time Stamp", "Trial Duration", "Input State",
                                                    "Hit Index", "Target x", "Target y", "Target z",
                                                    "Is Exp Target", "Hit Offset x", "Hit Offset y", "Hit Offset z", "Hit Offset mag", "Is Timeout"};
    private int _setNum;
    private int _trialNum;
    private long _timeStamp;
    private float _trialDuration;
    private string _inputState;
    private string _hitIndex;
    private Vector3 _targetPosition;
    private string _isExpTarget;
    private Vector3 _hitOffset;
    private string _isTimeout;
    private delegate string GenerateStringArrayDelegate(string str);
    private GenerateStringArrayDelegate _genStr;

    private Vector3 _expTargetPosition;
    private int _expTargetIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init()
    {
        // generateStringArrayDelegate = _taskNum == 1 ? GenerateStringArrayTask1 : GenerateStringArrayTask2;
        _genStr = _taskNum == 1 ? GenerateStringArrayTask1 : GenerateStringArrayTask2;
        baseDirectoryPath = Application.dataPath;
        dataDirectoryPath = "/Data/";
        CreateNewFile();
        // ResetValues();
    }

    void OnDestroy()
    {
        _streamWriter.Close();
    }

    void ResetValues()
    {
        _trialDuration = 0f;
        _inputState = "";
        _hitIndex = "";
        _targetPosition = Vector3.zero;
        _isExpTarget = "";
        _hitOffset = Vector3.zero;
        _isTimeout = "";
    }

    public void SetExpSceneManager(ExpSceneManager esm)
    {
        _expSceneManager = esm;
        _expSceneManager.RegisterForStartEvent(new UnityAction<int, int, int, Vector3>(OnTrialStart));
        _expSceneManager.RegisterForEndEvent(new UnityAction<float, string>(OnTrialEnd));
        if(_taskNum == 1)
        {

        } else if(_taskNum == 2)
        {
            ((CubeSceneManager)_expSceneManager).RegisterForGoalInEvent(new UnityAction<float>(OnGoalIn));
            ((CubeSceneManager)_expSceneManager).RegisterForGoalOutEvent(new UnityAction<float>(OnGoalOut));
        }
    }

    public void SetRaycastingTool(RaycastingTool tool)
    {
        tool.RegisterForTargetChangeEvent(new UnityAction<float, string, Vector3[], string>(OnTargetChange));
        tool.RegisterForSelectionEvent(new UnityAction<float, string, Vector3[], string>(OnSelection));
        tool.RegisterForGrabEvent(new UnityAction<float, string, Vector3>(OnGrab));
        tool.RegisterForReleaseEvent(new UnityAction<float, string, Vector3>(OnRelease));
        tool.RegisterForInputStateChangeEvent(new UnityAction<float, string, string>(OnInputStateChange));
    }

    public void CreateNewFile()
    {
        string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")+"_EventLog.csv";
        string fullPath = baseDirectoryPath+dataDirectoryPath+fileName;

        _fsWrite = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        _streamWriter = new StreamWriter(_fsWrite, System.Text.Encoding.UTF8);
        string header = _taskNum == 1 ? string.Join(",", _eventLogHeaderTask1) : "temp";
        _streamWriter.WriteLine(header);
    }

    public void OnTrialStart(int currSet, int currTrial, int targetIndex, Vector3 targetPos)
    {
        ResetValues();
        _setNum = currSet;
        _trialNum = currTrial;
        _hitIndex = targetIndex.ToString();
        _targetPosition = targetPos;
        _expTargetPosition = targetPos;
        _streamWriter.WriteLine(_genStr("Trial Start"));
    }

    public void OnTrialEnd(float trialDuration, string isTimeOut)
    {
        ResetValues();
        _trialDuration = trialDuration;
        _isTimeout = isTimeOut;
        _streamWriter.WriteLine(_genStr("Trial End"));
        _expTargetPosition = Vector3.zero;
    }
    
    public void OnInputStateChange(float trialDuration, string prevstate, string currstate)
    {
        ResetValues();
        _trialDuration = trialDuration;
        _inputState = currstate;
        _streamWriter.WriteLine(_genStr("Ray State Change"));
        // Debug.Log("Logger: ray state changed: from "+prevstate+" to "+currstate);
    }

    public void OnGoalIn(float trialDuration)
    {
        ResetValues();
        Debug.Log("Logger: target goal in");
    }

    public void OnGoalOut(float trialDuration)
    {
        ResetValues();
        Debug.Log("Logger: target goal out");
    }

    public void OnTargetChange(float trialDuration, string tg, Vector3[] v, string b)
    {
        ResetValues();
        _trialDuration = trialDuration;
        _hitIndex = tg;
        _targetPosition = v[0];
        _hitOffset = tg == "null" ? Vector3.zero : v[1] - _expTargetPosition;
        _isExpTarget = b;
        _streamWriter.WriteLine(_genStr("Target Change"));
        // Debug.Log("Logger: target change occurred: "+prevtg+" to "+tg);
    }

    public void OnSelection(float trialDuration, string objstr, Vector3[] v, string b)
    {
        ResetValues();
        _trialDuration = trialDuration;
        _hitIndex = objstr;
        _isExpTarget = b;
        _targetPosition = v[0];
        _hitOffset = objstr == "null" ? Vector3.zero : v[1] - _expTargetPosition;
        _streamWriter.WriteLine(_genStr("Selection"));
        // Debug.Log("Logger: object selected: "+objstr);
    }

    public void OnGrab(float trialDuration, string objstr, Vector3 offset)
    {
        ResetValues();
        Debug.Log("Logger: object grabbed: "+objstr);
    }

    public void OnRelease(float trialDuration, string objstr, Vector3 offset)
    {
        ResetValues();
        Debug.Log("Logger: object released: "+objstr);
    }

    public string GenerateStringArrayTask1(string eventName)
    {
        List<string> values = new List<string>();

        values.Add(_setNum.ToString());
        values.Add(_trialNum.ToString());
        values.Add(eventName);
        DateTimeOffset dt = new DateTimeOffset(DateTime.Now);
        values.Add(dt.ToUnixTimeMilliseconds().ToString()); //timestamp
        values.Add(_trialDuration.ToString());
        values.Add(_inputState);
        values.Add(_hitIndex);
        values.Add(_targetPosition.x.ToString());   // preserve decimals
        values.Add(_targetPosition.y.ToString());
        values.Add(_targetPosition.z.ToString());
        values.Add(_isExpTarget.ToString());
        values.Add(_hitOffset.x.ToString());
        values.Add(_hitOffset.y.ToString());
        values.Add(_hitOffset.z.ToString());
        values.Add(_hitOffset.magnitude.ToString());
        values.Add(_isTimeout.ToString());

        return String.Join(",", values.ToArray());
    }

    public string GenerateStringArrayTask2(string eventName)
    {
        return "";
    }
}
