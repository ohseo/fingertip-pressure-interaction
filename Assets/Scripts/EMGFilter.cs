using System;
using System.Collections;
using System.Collections.Generic;

public class EMGFilter
{

    int windowSize = 20;

    List<float>[] emgDataWindow = new List<float>[8];

    List<int>[] rawDataBin = new List<int>[8];

    public float[] emg_raw = new float[8];
    public float[] emg_rec = new float[8];
    public float[] emg_RMS = new float[8];

    public float[] emg_MAV = new float[8];
    public float emg_totalMAV;
    public float emg_totalRMS;
    public float emg_weightedSum;


    public float[] min = new float[8];
    public float[] max = new float[8];
    int minCalibrationCounter = 0;
    int maxCalibrationCounter = 0;
    int calibrationDuration = 1000;
    int rate = 200;


    public EMGFilter()
    {

        for (int i = 0; i < 8; i++)
        {
            emgDataWindow[i] = new List<float>();
            for (int j = 0; j < windowSize; j++)
            {
                emgDataWindow[i].Add(0);
            }

            rawDataBin[i] = new List<int>();
            for (int j = 0; j < 600; j++)
            {
                rawDataBin[i].Add(0);
            }
            min[i] = 0;
            max[i] = 1;
        }
    }

    public void addEMGValue(int[] emg)
    {
        for (int i = 0; i < 8; i++)
        {
            emg_raw[i] = (float)emg[i] / 128;
            emg_rec[i] = Math.Abs(emg_raw[i]);

            rawDataBin[i].Add(emg[i]);
            rawDataBin[i].RemoveAt(0);

            emgDataWindow[i].Add(emg_rec[i]);
            emgDataWindow[i].RemoveAt(0);

            emg_MAV[i] = calculateMAV(emgDataWindow[i]);
            emg_RMS[i] = calculateRMS(emgDataWindow[i]);

            // Calibration
            if (minCalibrationCounter-- > 0)
                if (min[i] < emg_RMS[i]) min[i] = emg_RMS[i];
            if (maxCalibrationCounter-- > 0)
                if (max[i] < emg_RMS[i]) max[i] = emg_RMS[i];

        }
        calculateWeightedSum();

        calculateTotalMAV();
        calculateTotalRMS();
    }

    public void setMin(float[] min)
    {
        min.CopyTo(this.min, 0);
    }

    public void setMax(float[] max)
    {
        max.CopyTo(this.max, 0);
    }

    public float[] getFilteredEMG()
    {
        return null;
    }

    public void calibrateMinValue()
    {
        for (int i = 0; i < 8; i++) min[i] = 0;
        minCalibrationCounter = calibrationDuration;
    }

    public void calibrateMaxValue()
    {
        for (int i = 0; i < 8; i++) max[i] = 0;
        maxCalibrationCounter = calibrationDuration;
    }

    public void resetMinValue()
    {
        for (int i = 0; i < 8; i++) min[i] = 0;
    }

    public void resetMaxValue()
    {
        for (int i = 0; i < 8; i++) max[i] = 1;
    }


    private void calculateMAV()
    {
        for (int i = 0; i < 8; i++)
        {
            float sum = 0;
            foreach (float val in emgDataWindow[i])
            {
                sum += val;
            }
            emg_MAV[i] = sum / windowSize;
        }
    }

    private float calculateMAV(List<float> channel)
    {
        float sum = 0;
        foreach (float val in channel)
        {
            sum += val;
        }
        return sum / windowSize;
    }

    private void calculateTotalMAV()
    {
        float sum = 0;
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            foreach (float val in emgDataWindow[i])
            {
                sum += val;
                count++;
            }
        }
        emg_totalMAV = sum / count;
    }

    private void calculateRMS()
    {
        for (int i = 0; i < 8; i++)
        {
            double sum = 0;
            foreach (float val in emgDataWindow[i])
            {
                sum += val * val;
            }
            emg_RMS[i] = (float)Math.Sqrt(sum / windowSize);
        }
    }

    private float calculateRMS(List<float> channel)
    {
        double sum = 0;
        foreach (float val in channel)
        {
            sum += val * val;
        }
        return (float)Math.Sqrt(sum / windowSize);
    }

    private void calculateTotalRMS()
    {
        double sum = 0;
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            foreach (float val in emgDataWindow[i])
            {
                sum += val * val;
                count++;
            }
        }
        emg_totalRMS = (float)Math.Sqrt(sum / count);
    }

    private void calculateWeightedSum()
    {
        float sum = 0;
        for (int i = 0; i < 8; i++)
        {
            if (max[i] > min[i])
            {
                sum += max[i] - min[i];
            }
            else
            {
                min[i] = max[i];
            }

            emg_weightedSum += (emg_RMS[i] - min[i]) < 0 ? 0 : emg_RMS[i] - min[i];
        }

        emg_weightedSum /= sum;

    }

    public List<int>[] getRawData()
    {
        return rawDataBin;
    }
}
