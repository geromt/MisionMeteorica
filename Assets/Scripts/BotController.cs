using UnityEngine;

public class BotController : MonoBehaviour
{
    public GameObject LeftShoulder;
    public GameObject LeftArm;
    public GameObject LeftForeArm;
    public GameObject RightShoulder;
    public GameObject RightArm;
    public GameObject RightForeArm;

    private void OnEnable()
    {
        ArmVisibilityController.PartIsVisible += ShowArm;
        ArmVisibilityController.PartIsNotVisible += HideArm;
    }

    private void OnDisable()
    {
        ArmVisibilityController.PartIsVisible -= ShowArm;
        ArmVisibilityController.PartIsNotVisible -= HideArm;
    }

    private void ShowArm(ArmVisibilityController.BodyParts bp)
    {
        if (bp == ArmVisibilityController.BodyParts.LeftWrist)
        {
            LeftShoulder.GetComponent<ArticulationController>().enabled = true;
            LeftArm.GetComponent<ArticulationController>().enabled = true;
            LeftForeArm.GetComponent<ArticulationController>().enabled = true;
        }
        if (bp == ArmVisibilityController.BodyParts.RightWrist)
        {
            RightShoulder.GetComponent<ArticulationController>().enabled = true;
            RightArm.GetComponent<ArticulationController>().enabled = true;
            RightForeArm.GetComponent<ArticulationController>().enabled = true;
        }
    }

    private void HideArm(ArmVisibilityController.BodyParts bp)
    {
        if (bp == ArmVisibilityController.BodyParts.LeftWrist)
        {
            DeactivateBodyPart(LeftShoulder);
            DeactivateBodyPart(LeftArm);
            DeactivateBodyPart(LeftForeArm);
        }
        if (bp == ArmVisibilityController.BodyParts.RightWrist)
        {
            DeactivateBodyPart(RightShoulder);
            DeactivateBodyPart(RightArm);
            DeactivateBodyPart(RightForeArm);
        }
    }

    private void DeactivateBodyPart(GameObject bpGameObject)
    {
        bpGameObject.GetComponent<ArticulationController>().enabled = false;
        bpGameObject.transform.localRotation = Quaternion.identity;
    }
}
