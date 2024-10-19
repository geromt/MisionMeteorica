using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class WriteToFileController : MonoBehaviour
{
    [SerializeField] private bool writeData = false;

    private StreamWriter wristPosWriter;
    private StreamWriter launchInfoWriter;
    private StreamWriter leftLaunchesWriter;
    private StreamWriter rightLaunchesWriter;

    private bool left = false;
    private bool right = false;
    private Vector3 leftHand;
    private Vector3 rightHand;

    private void OnEnable()
    {
        SensorAdmin.Send += WriteWristPos;
        GeneratorController.OnAsteroidDestinationSelect += WriteInitLaunch;
        AdminController.OnCatchEnd += WriteEndLaunch;
    }

    private void OnDisable()
    {
        SensorAdmin.Send -= WriteWristPos;
        GeneratorController.OnAsteroidDestinationSelect -= WriteInitLaunch;
        AdminController.OnCatchEnd += WriteEndLaunch;

        wristPosWriter.Close();
        launchInfoWriter.Close();
        leftLaunchesWriter.Close();
        rightLaunchesWriter.Close();
    }

    private void Awake()
    {
        wristPosWriter = new StreamWriter(Path.Combine(GameMaster.rutaDeArchivos, "WristPosInfo.txt"));
        launchInfoWriter = new StreamWriter(Path.Combine(GameMaster.rutaDeArchivos, "LaunchInfo.txt"));
        leftLaunchesWriter = new StreamWriter(Path.Combine(GameMaster.rutaDeArchivos, "LeftLaunches.txt"));
        rightLaunchesWriter = new StreamWriter(Path.Combine(GameMaster.rutaDeArchivos, "RightLaunches.txt"));
    }

    private void Update()
    {
        if (!writeData) return;

        if (left)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now);
            sb.Append(';');
            sb.Append(leftHand.ToString("F3"));

            leftLaunchesWriter.WriteLine(sb.ToString());
        }
        if (right)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(DateTime.Now);
            sb.Append(';');
            sb.Append(rightHand.ToString("F3"));

            rightLaunchesWriter.WriteLine(sb.ToString());
        }
    }

    private void WriteWristPos(List<Landmark> landmarks)
    {
        if (!writeData) return;

        StringBuilder sb = new StringBuilder();
        sb.Append(DateTime.Now);
        sb.Append(';');
        sb.Append(landmarks[19].ToVector3().ToString("F3"));
        sb.Append(';');
        sb.Append(landmarks[20].ToVector3().ToString("F3"));

        wristPosWriter.WriteLine(sb.ToString());

        rightHand = landmarks[16].ToVector3();
        leftHand = landmarks[15].ToVector3();
    }

    private void WriteInitLaunch(float x, float y)
    {
        if (!writeData) return;

        StringBuilder sb = new StringBuilder();
        sb.Append(DateTime.Now);
        sb.Append(';');
        sb.AppendFormat("x: {0};", x);
        sb.AppendFormat("y: {0};", y);

        launchInfoWriter.WriteLine(sb.ToString());

        if (x > 0)
            right = true;
        if (x < 0)
            left = true;
    }

    private void WriteEndLaunch(bool didCatchBall)
    {
        if (!writeData) return;

        StringBuilder sb = new StringBuilder();
        sb.Append(DateTime.Now);
        sb.Append(';');
        sb.Append(didCatchBall);

        launchInfoWriter.WriteLine(sb.ToString());

        if (right)
        {
            rightLaunchesWriter.WriteLine("###");
            right = false;
        }
        if (left)
        {
            leftLaunchesWriter.WriteLine("###");
            left = false;
        }
    }
}
