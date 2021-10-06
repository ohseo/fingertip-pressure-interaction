using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EMGUIManager : MonoBehaviour
{
    [SerializeField]
    List<UIGauge> gauges;
    [SerializeField]
    UIGauge forceGauge;
    [SerializeField]
    ThalmicMyo myo;
    EMGFilter emgFilter;

    // Use this for initialization
    void Start()
    {
        if (myo != null) emgFilter = myo.internalMyo.emgFilter;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) onCalibrateMin();
        if (Input.GetKeyDown(KeyCode.S)) onCalibrateMax();
        if (Input.GetKeyDown(KeyCode.Q)) onResetMin();
        if (Input.GetKeyDown(KeyCode.W)) onResetMax();
        if (Input.GetKeyDown(KeyCode.Space)) SaveCalibrationData();

        for (int i = 0; i < 8; i++)
        {
            gauges[i].value = myo.emgFiltered[i] / 128;
            //gauges[i].value = emgFilter.emg_rec[i];
            gauges[i].min = emgFilter.min[i];
            gauges[i].max = emgFilter.max[i];
        }

        if (forceGauge != null) forceGauge.value = emgFilter.emg_weightedSum;
    }

    void OnapplicationQuit()
    {
        SaveCalibrationData();
    }

    void SaveCalibrationData()
    {
        EMGDataSaveSystem.SaveData(emgFilter.min, emgFilter.max);
    }

    void DrawQuad(Rect position, Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.Box(position, GUIContent.none);
    }

    public void onCalibrateMin()
    {
        emgFilter.calibrateMinValue();
    }

    public void onCalibrateMax()
    {
        emgFilter.calibrateMaxValue();
    }

    public void onResetMin()
    {
        emgFilter.resetMinValue();
    }

    public void onResetMax()
    {
        emgFilter.resetMaxValue();
    }

    private void OnGUI()
    {
        //DrawQuad(new Rect(20,20,20,20),Color.red);
    }
}
