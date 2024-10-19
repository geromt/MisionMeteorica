using System.Collections.Generic;
using UnityEngine;

public class ArmVisibilityController : MonoBehaviour
{
    public delegate void ArmVisibilityEvent(BodyParts bodyParts);
    public static event ArmVisibilityEvent PartIsVisible;
    public static event ArmVisibilityEvent PartIsNotVisible;

    public enum BodyParts
    {
        LeftElbow,
        LeftWrist,
        RightElbow,
        RightWrist
    }

    private const int LeftElbowIndex = 13;
    private const int LeftWristIndex = 15;
    private const int RightElbowIndex = 14;
    private const int RightWristIndex = 16;
    private const float VisibilityThreshold = 0.7f;

    private bool _isLeftElbowVisible = false;
    private bool _isLeftWristVisible = false;
    private bool _isRightElbowVisible = false;
    private bool _isRightWristVisible = false;

    private void OnEnable()
    {
        SensorAdmin.Send += CheckVisibility;
    }

    private void OnDisable()
    {
        SensorAdmin.Send -= CheckVisibility;
    }

    private void CheckVisibility(List<Landmark> landmarks)
    {
        CheckBodyPartVisibility(landmarks[LeftElbowIndex], BodyParts.LeftElbow, ref _isLeftElbowVisible);
        CheckBodyPartVisibility(landmarks[LeftWristIndex], BodyParts.LeftWrist, ref _isLeftWristVisible);
        CheckBodyPartVisibility(landmarks[RightElbowIndex], BodyParts.RightElbow, ref _isRightElbowVisible);
        CheckBodyPartVisibility(landmarks[RightWristIndex], BodyParts.RightWrist, ref _isRightWristVisible);
    }

    private void CheckBodyPartVisibility(Landmark lm, BodyParts bodyPart, ref bool state)
    {
        if (lm.Visibility > VisibilityThreshold)
        {
            if (!state)
            {
                state = true;
                PartIsVisible?.Invoke(bodyPart);
            }
        }
        else
        {
            if (state)
            {
                state = false;
                PartIsNotVisible?.Invoke(bodyPart);
            }
        }
    }
}
