using UnityEngine;

public class EsperaTween : MonoBehaviour
{
    private void Start()
    {
        LeanTween.rotateAroundLocal(gameObject, Vector3.back, 359, 1).setLoopClamp().setIgnoreTimeScale(true);
    }
}
