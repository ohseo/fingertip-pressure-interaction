using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class EMGCalibrationData
{
    public float[] min;
    public float[] max;

    public EMGCalibrationData(float[] min, float[] max)
    {
        this.min = new float[8];
        this.max = new float[8];
        min.CopyTo(this.min, 0);
        max.CopyTo(this.max, 0);
    }
}


public static class EMGDataSaveSystem
{
    public static void SaveData (float[] minData, float[] maxData)
    {
        BinaryFormatter formatter = new BinaryFormatter();

        string path = Application.dataPath + "/Resources/CalibrationData/user.dat";
        FileStream stream = new FileStream(path, FileMode.Create);

        EMGCalibrationData data = new EMGCalibrationData(minData, maxData);

        formatter.Serialize(stream, data);
        stream.Close();
        Debug.Log("saved datat to " + path);
    }

    public static void LoadData (EMGFilter emgFilter)
    {
        string path = Application.dataPath + "/Resources/CalibrationData/user.dat";

        if(File.Exists(path))
        {
            BinaryFormatter foramtter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            EMGCalibrationData data = foramtter.Deserialize(stream) as EMGCalibrationData;
            emgFilter.setMin(data.min);
            emgFilter.setMax(data.max);
        }
        else
        {
            Debug.LogError("calibration file not found");
            return;
        }
    } 
}
