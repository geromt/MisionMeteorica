using System.Collections.Generic;
using UnityEngine;

public class PoseTracker : MonoBehaviour
{
    [SerializeField] private GameObject[] posePoints;

    private void OnEnable()
    {
        SensorAdmin.Send += ProcessLandmarks;
    }

    private void OnDisable()
    {
        SensorAdmin.Send -= ProcessLandmarks;
    }

    private void ProcessLandmarks(List<Landmark> landmarks)
    {
        for (int i = 0; i < posePoints.Length; i++)
            posePoints[i].transform.localPosition = landmarks[i].ToVector3();
    }
}
