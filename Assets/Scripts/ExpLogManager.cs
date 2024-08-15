using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class ExpLogManager : MonoBehaviour
{
    public int _participantNum { get; set; }
    public int _taskNum { get; set; }
    public int _raycastingMode { get; set; }
    public int _depthCondition { get; set; }
    
    private ExpSceneManager _expSceneManager;
    
    private string baseDirectoryPath;
    private string dataDirectoryPath;
    private string fileName;
    private FileStream _eventFSWrite, _summaryFSWrite;
    private StreamWriter _eventStreamWriter, _summaryStreamWriter;
    private string[] _eventLogHeaderTask1 = new string[17]{"Set Number", "Trial Number", "Event Name", "Time Stamp", "Trial Duration", "Input State",
                                                    "Target Index", "Target x", "Target y", "Target z", "Is Exp Target", "Is User Error",
                                                    "Hit Offset x", "Hit Offset y", "Hit Offset z", "Hit Offset mag", "Is Timeout"};
    private string[] _eventLogHeaderTask2 = new string[16]{"Set Number", "Trial Number", "Event Name", "Time Stamp", "Trial Duration", "Input State",
                                                    "Target Index", "Target x", "Target y", "Target z", "Is In Goal",
                                                    "Goal Offset x", "Goal Offset y", "Goal Offset z", "Goal Offset mag", "Is Timeout"};
    private string[] _summaryLogHeaderTask1 = new string[6]{"Set Number", "Trial Number", "Task Completion Time", "User Errors", "Target Dwell Time", "Is Timeout"};
    private string[] _summaryLogHeaderTask2 = new string[9]{"Set Number", "Trial Number", "Task Completion Time", "Goal Dwell Time",
                                                    "Goal Offset x", "Goal Offset y", "Goal Offset z", "Goal Offset mag", "Is Timeout"};
    
    private int _setNum;
    private int _trialNum;

    private long _timeStamp;
    private float _trialDuration;
    private string _inputState;
    private string _hitIndex;
    private Vector3 _targetPosition;
    private string _isExpTarget;
    private string _isUserError;
    private string _isInGoal;
    private Vector3 _hitOffset;
    private Vector3 _goalOffset;
    private string _isTimeout;
    private delegate string GenerateEventStringDelegate(string str);
    private GenerateEventStringDelegate _genEventStr;

    private int _userErrors;
    private float _targetDwellTime;
    private bool _targetEncountered;
    private float _targetEncounterTime;
    private float _goalDwellTime;
    private bool _goalEncountered;
    private float _goalEncounterTime;
    private Vector3 _goalOffsetFinal;
    private delegate string GenerateSummaryStringDelegate();
    private GenerateSummaryStringDelegate _genSumStr;

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
        _genEventStr = _taskNum == 1 ? GenerateEventStringTask1 : GenerateEventStringTask2;
        _genSumStr = _taskNum == 1 ? GenerateSummaryStringTask1 : GenerateSummaryStringTask2;
        baseDirectoryPath = Application.dataPath;
        dataDirectoryPath = "/Data/";
        CreateNewFile();
        // ResetValues();
    }

    void OnDestroy()
    {
        _eventStreamWriter.Close();
        _eventFSWrite.Close();

        _summaryStreamWriter.Close();
        _summaryFSWrite.Close();
    }

    void ResetEventValues()
    {
        _trialDuration = 0f;
        _inputState = "";
        _hitIndex = "";
        _targetPosition = Vector3.zero;
        _isExpTarget = "";
        _isUserError = "";
        _isInGoal = "";
        _hitOffset = Vector3.zero;
        _goalOffset = Vector3.zero;
        _isTimeout = "";
    }

    void ResetSummaryValues()
    {
        _userErrors = 0;
        _targetDwellTime = 0f;
        _targetEncountered = false;

        _goalDwellTime = 0f;
        _goalEncountered = false;
        _goalOffsetFinal = Vector3.zero;
    }

    public void SetExpSceneManager(ExpSceneManager esm)
    {
        _expSceneManager = esm;
        _expSceneManager.RegisterForStartEvent(new UnityAction<int, int, int, Vector3[]>(OnTrialStart));
        _expSceneManager.RegisterForEndEvent(new UnityAction<float, string, Vector3>(OnTrialEnd));
        if(_taskNum == 1)
        {

        } else if(_taskNum == 2)
        {
            ((CubeSceneManager)_expSceneManager).RegisterForGoalInEvent(new UnityAction<float, Vector3, Vector3>(OnGoalIn));
            ((CubeSceneManager)_expSceneManager).RegisterForGoalOutEvent(new UnityAction<float, Vector3, Vector3>(OnGoalOut));
        }
    }

    public void SetExpConditions(int p, int taskNum, int modeNum, int conditionNum)
    {
        _participantNum = p;
        _taskNum = taskNum;
        _raycastingMode = modeNum;
        _depthCondition = conditionNum;
    }

    public void SetRaycastingTool(RaycastingTool tool)
    {
        tool.RegisterForTargetChangeEvent(new UnityAction<float, string, Vector3[], string>(OnTargetChange));
        tool.RegisterForSelectionEvent(new UnityAction<float, string, Vector3[], string>(OnSelection));
        tool.RegisterForGrabEvent(new UnityAction<float, string, Vector3[], string>(OnGrab));
        tool.RegisterForReleaseEvent(new UnityAction<float, string, Vector3[], string>(OnRelease));
        tool.RegisterForInputStateChangeEvent(new UnityAction<float, string, string>(OnInputStateChange));
    }

    public string CreateFilePath()
    {
        string conditions = $"_P{_participantNum}_Task{_taskNum}_{_raycastingMode}{_depthCondition}";
        string fileName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")+conditions;
        string fullPath = baseDirectoryPath+dataDirectoryPath+fileName;
        return fullPath;
    }

    public void CreateNewFile()
    {
        string fullPath = CreateFilePath();

        // event log
        string eventFullPath = fullPath+"_EventLog.csv";

        _eventFSWrite = new FileStream(eventFullPath, FileMode.Create, FileAccess.Write);
        _eventStreamWriter = new StreamWriter(_eventFSWrite, System.Text.Encoding.UTF8);
        string header = _taskNum == 1 ? string.Join(",", _eventLogHeaderTask1) : string.Join(",", _eventLogHeaderTask2);
        _eventStreamWriter.WriteLine(header);

        //summary
        string summaryFullPath = fullPath+"_Summary.csv";

        _summaryFSWrite = new FileStream(summaryFullPath, FileMode.Create, FileAccess.Write);
        _summaryStreamWriter = new StreamWriter(_summaryFSWrite, System.Text.Encoding.UTF8);
        header = _taskNum == 1 ? string.Join(",", _summaryLogHeaderTask1) : string.Join(",", _summaryLogHeaderTask2);
        _summaryStreamWriter.WriteLine(header);
    }

    public void OnTrialStart(int currSet, int currTrial, int targetIndex, Vector3[] v)
    {
        ResetEventValues();
        _setNum = currSet;
        _trialNum = currTrial;
        _hitIndex = targetIndex.ToString();
        _targetPosition = v[0];
        _expTargetPosition = v[0];
        _goalOffset = v[0] - v[1];
        _eventStreamWriter.WriteLine(_genEventStr("Trial Start"));

        ResetSummaryValues();
    }

    public void OnTrialEnd(float trialDuration, string isTimeOut, Vector3 goalOffset)
    {
        ResetEventValues();
        _trialDuration = trialDuration;
        _goalOffset = goalOffset;
        _isTimeout = isTimeOut;
        _eventStreamWriter.WriteLine(_genEventStr("Trial End"));
        _expTargetPosition = Vector3.zero;

        if(_taskNum == 1 && _targetEncountered)
        {
            _targetDwellTime = trialDuration - _targetEncounterTime;
            _targetEncountered = false;
        } else if(_taskNum == 2 && _goalEncountered)
        {
            _goalDwellTime = trialDuration - _goalEncounterTime;
            _goalEncountered = false;
            _goalOffsetFinal = goalOffset;
        }
        _summaryStreamWriter.WriteLine(_genSumStr());
    }
    
    public void OnInputStateChange(float trialDuration, string prevstate, string currstate)
    {
        ResetEventValues();
        _trialDuration = trialDuration;
        _inputState = currstate;
        _eventStreamWriter.WriteLine(_genEventStr("Ray State Change"));
        // Debug.Log("Logger: ray state changed: from "+prevstate+" to "+currstate);
    }

    public void OnGoalIn(float trialDuration, Vector3 targetPos, Vector3 goalOffset)
    {
        ResetEventValues();
        _trialDuration = trialDuration;
        _targetPosition = targetPos;
        _goalOffset = goalOffset;
        _eventStreamWriter.WriteLine(_genEventStr("Goal In"));
        // Debug.Log("Logger: target goal in");

        if(_taskNum == 2 && !_goalEncountered)
        {
            _goalEncountered = true;
            _goalEncounterTime = trialDuration;
        }
    }

    public void OnGoalOut(float trialDuration, Vector3 targetPos, Vector3 goalOffset)
    {
        ResetEventValues();
        _trialDuration = trialDuration;
        _targetPosition = targetPos;
        _goalOffset = goalOffset;
        _eventStreamWriter.WriteLine(_genEventStr("Goal Out"));
        // Debug.Log("Logger: target goal out");
    }

    public void OnTargetChange(float trialDuration, string tg, Vector3[] v, string b)
    {
        ResetEventValues();
        _trialDuration = trialDuration;
        _hitIndex = tg;
        _targetPosition = v[0];
        _hitOffset = tg == "null" ? Vector3.zero : v[1] - _expTargetPosition;
        _isExpTarget = b;
        _eventStreamWriter.WriteLine(_genEventStr("Target Change"));
        // Debug.Log("Logger: target change occurred: "+prevtg+" to "+tg);

        if(_taskNum == 1 && b.Equals(true.ToString()) && !_targetEncountered)
        {
            _targetEncountered = true;
            _targetEncounterTime = trialDuration;
        }
    }

    public void OnSelection(float trialDuration, string objstr, Vector3[] v, string b)
    {
        ResetEventValues();
        _trialDuration = trialDuration;
        _hitIndex = objstr;
        _isExpTarget = b;
        _isUserError = b.Equals(false.ToString()) ? true.ToString() : false.ToString();
        _targetPosition = v[0];
        _hitOffset = objstr == "null" ? Vector3.zero : v[1] - _expTargetPosition;
        _eventStreamWriter.WriteLine(_genEventStr("Selection"));
        // Debug.Log("Logger: object selected: "+objstr);

        if(b.Equals(true.ToString()) && _targetEncountered)
        {
            _targetDwellTime = trialDuration - _targetEncounterTime;
            _targetEncountered = false;
        } else if(b.Equals(false.ToString()))
        {
            _userErrors++;
        }
    }

    public void OnGrab(float trialDuration, string objstr, Vector3[] v, string b)
    {
        ResetEventValues();
        _trialDuration = trialDuration;
        _hitIndex = objstr;
        _targetPosition = v[0];
        _isInGoal = b;
        _goalOffset = v[1];
        _eventStreamWriter.WriteLine(_genEventStr("Grab"));
        // Debug.Log("Logger: object grabbed: "+objstr);
    }

    public void OnRelease(float trialDuration, string objstr, Vector3[] v, string b)
    {
        ResetEventValues();
        _trialDuration = trialDuration;
        _hitIndex = objstr;
        _targetPosition = v[0];
        _isInGoal = b;
        _goalOffset = v[1];
        _eventStreamWriter.WriteLine(_genEventStr("Release"));
        // Debug.Log("Logger: object released: "+objstr);

        if(b.Equals(true.ToString()) && _goalEncountered)
        {
            _goalDwellTime = trialDuration - _goalEncounterTime;
            _goalEncountered = false;
        }
        _goalOffsetFinal = v[1];
    }

    public string GenerateEventStringTask1(string eventName)
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
        values.Add(_isExpTarget);
        values.Add(_isUserError);
        values.Add(_hitOffset.x.ToString());
        values.Add(_hitOffset.y.ToString());
        values.Add(_hitOffset.z.ToString());
        values.Add(_hitOffset.magnitude.ToString());
        values.Add(_isTimeout);

        return String.Join(",", values.ToArray());
    }

    public string GenerateEventStringTask2(string eventName)
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
        values.Add(_isInGoal);
        values.Add(_goalOffset.x.ToString());
        values.Add(_goalOffset.y.ToString());
        values.Add(_goalOffset.z.ToString());
        values.Add(_goalOffset.magnitude.ToString());
        values.Add(_isTimeout);

        return String.Join(",", values.ToArray());
    }

    public string GenerateSummaryStringTask1()
    {
        List<string> values = new List<string>();

        values.Add(_setNum.ToString());
        values.Add(_trialNum.ToString());
        values.Add(_trialDuration.ToString());
        values.Add(_userErrors.ToString());
        values.Add(_targetDwellTime.ToString());
        values.Add(_isTimeout);

        return String.Join(",", values.ToArray());
    }

    public string GenerateSummaryStringTask2()
    {
        List<string> values = new List<string>();

        values.Add(_setNum.ToString());
        values.Add(_trialNum.ToString());
        values.Add(_trialDuration.ToString());
        values.Add(_goalDwellTime.ToString());
        values.Add(_goalOffsetFinal.x.ToString());
        values.Add(_goalOffsetFinal.y.ToString());
        values.Add(_goalOffsetFinal.z.ToString());
        values.Add(_goalOffsetFinal.magnitude.ToString());
        values.Add(_isTimeout);

        return String.Join(",", values.ToArray());
    }
}
