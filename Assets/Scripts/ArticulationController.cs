using UnityEngine;

public class ArticulationController : MonoBehaviour
{
    [Tooltip("Punto de origen")]
    [SerializeField] private Transform from;
    [Tooltip("Punto destino")]
    [SerializeField] private Transform to;
    [Tooltip("Rotacion de correccion")]
    [SerializeField] private Vector3 rotateTargetToZ;

    private Quaternion _cilindroRotation;
    private Vector3 _lookToVector;
    private Quaternion _changeTarget;
    private Quaternion _forwardToTarget;

    private Transform _thisTransform;

    private void Awake()
    {
        _thisTransform = transform;
    }

    private void Update()
    {
        _lookToVector = to.position - from.position;
        if (_lookToVector == Vector3.zero)
            return;
        _cilindroRotation = CustomLookRotation(_lookToVector, Vector3.up);
        _thisTransform.rotation = Quaternion.Lerp(_thisTransform.rotation, _cilindroRotation, 0.2f);
    }

    private Quaternion CustomLookRotation(Vector3 forward, Vector3 up)
    {
        _changeTarget = Quaternion.Euler(rotateTargetToZ);
        _forwardToTarget = Quaternion.LookRotation(forward, up);

        return _forwardToTarget * _changeTarget;
    }
}
