using System;
using System.Collections.Generic;
using UnityEngine;

public class WebcamUtils
{
    private static Vector3[] previousLMs = new Vector3[33];

    private static string[] values;
    private static int fps;
    private static List<Landmark> landmarks;
    private static Vector3 currentLM;
    private static Vector3 averageLM;
    private static float visibility, x, y, z;

    public static (bool success, WebcamData? data) WebcamDataProcessing(string data, bool calcEMA)
    {
        if (data.Length < 1 || data.Equals("['']"))
        {
            #if UNITY_EDITOR
            Debug.LogWarning("Webcam tracker sent an empty list.");
            #endif
            return (false, null);
        }

        try
        {
            // Quitamos el primer y el ultimo caracter 
            data = data.Remove(0, 1);
            data = data.Remove(data.Length - 1, 1);
            values = data.Split(',');
        }
        catch (FormatException)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("Unexpected webcam data format");
            #endif
            return (false, null);
        }

        if (values.Length != 133)
        {
            #if UNITY_EDITOR
            Debug.LogWarning("Unexpected number of values in webcam data");
            #endif
            return (false, null);
        }

        fps = int.Parse(values[0]);
        landmarks = new List<Landmark>();

        for (int i = 0; i < (values.Length - 1) / 4; i++)
        {
            visibility = float.Parse(values[i * 4 + 1], System.Globalization.CultureInfo.InvariantCulture);
            x = float.Parse(values[i * 4 + 2], System.Globalization.CultureInfo.InvariantCulture);
            y = float.Parse(values[i * 4 + 3], System.Globalization.CultureInfo.InvariantCulture);
            z = float.Parse(values[i * 4 + 4], System.Globalization.CultureInfo.InvariantCulture);

            if (calcEMA)
            {
                currentLM = new Vector3(x, y, z);
                if (previousLMs[i] == null)
                {
                    previousLMs[i] = currentLM;
                    landmarks.Add(new Landmark(visibility, currentLM));
                }
                else
                {
                    averageLM = CalcEMA(currentLM, previousLMs[i]);
                    previousLMs[i] = averageLM;
                    landmarks.Add(new Landmark(visibility, averageLM));
                }
            }
            else
            {
                landmarks.Add(new Landmark(visibility, x, y, z));
            }
        }
        return (true, new WebcamData(landmarks, fps));
    }

    private static Vector3 CalcEMA(Vector3 currentLM, Vector3 previousLM, float smoothingFactor = 0.5f)
    {
        return smoothingFactor * currentLM + (1 - smoothingFactor) * previousLM;
    }
}

public readonly struct WebcamData
{
    public WebcamData(List<Landmark> landmarks, int fps)
    {
        Landmarks = landmarks;
        FPS = fps;
    }

    public List<Landmark> Landmarks { get; }
    public int FPS { get; }
};

public readonly struct Landmark
{
    public float Visibility { get; }
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    public Landmark(float visibility, float x, float y, float z)
    {
        Visibility = visibility;
        X = x;
        Y = y;
        Z = z;
    }

    public Landmark(float visibility, Vector3 coords)
    {
        Visibility = visibility;
        X = coords.x;
        Y = coords.y;
        Z = coords.z;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(X, Y, Z);
    }
}
