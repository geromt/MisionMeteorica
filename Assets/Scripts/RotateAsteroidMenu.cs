using UnityEngine;

public class RotateAsteroidMenu : MonoBehaviour
{
    [SerializeField] private int index;
    [SerializeField] private bool rotateX;
    [SerializeField] private bool rotateY;
    [SerializeField] private bool rotateZ;
    [SerializeField] private int time;

    private Vector3 _initPos;
    private Quaternion _initRotation;


    private void OnEnable()
    {
        MenuController.ClickAsteroid += RotateAsteroid;
    }

    private void OnDisable()
    {
        MenuController.ClickAsteroid -= RotateAsteroid;
    }

    private void Awake()
    {
        _initPos = transform.localPosition;
        _initRotation = transform.localRotation;
    }

    private void Update()
    {
        transform.localPosition = _initPos;
    }

    private void RotateAsteroid(int receiveIndex)
    {
        if (receiveIndex != index)
        {
            CancelRotation();
            return;
        } 

        if (rotateX)
            LeanTween.rotateAroundLocal(gameObject, Vector3.right, 360, time).setLoopClamp();

        if (rotateY)
            LeanTween.rotateAroundLocal(gameObject, Vector3.up, 360, time).setLoopClamp();

        if (rotateZ)
            LeanTween.rotateAroundLocal(gameObject, Vector3.forward, 360, time).setLoopClamp();
    }

    private void CancelRotation()
    {
        LeanTween.cancel(gameObject);
        transform.localPosition = _initPos;
        transform.localRotation = _initRotation;
    }
}
