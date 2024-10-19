using UnityEngine;

public class CameraVibration : MonoBehaviour
{
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float offset = 0.3f;

    private AnimationCurve _curve;


    private void OnEnable()
    {
        AsteroidController.OnAvoidHand += Vibrate;
    }

    private void OnDisable()
    {
        AsteroidController.OnAvoidHand -= Vibrate;
    }

    private void Awake()
    {
        _curve = new AnimationCurve(
            new Keyframe(0, 0),
            new Keyframe(0.2f, 1),
            new Keyframe(0.4f, -1),
            new Keyframe(0.6f, 0.5f),
            new Keyframe(0.8f, -0.5f),
            new Keyframe(1, 0)
        );
    }

    private void Vibrate()
    {
        if (GameMaster.CalidadGraficos == Graficos.Bajos)
            return;

        LeanTween.moveLocalX(gameObject, offset, duration).setEase(_curve);
    }
}
