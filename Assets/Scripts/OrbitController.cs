using UnityEngine;

public class OrbitController : MonoBehaviour
{
    [Tooltip("Indica si la rotation es al azar")]
    public bool staticRotation;
    [Tooltip("Si staticRotation es true, indica la rotacion de la orbita")]
    public Vector3 rotation;

    private Transform _planet;
    private Transform _thisTransform;
    private Vector3 _orbitRotation;
    private Vector3 _planetRotation;


    private void Awake()
    {
        _thisTransform = transform;
        _planet = _thisTransform.GetChild(0);
        _planet.rotation = Random.rotation;
        _planetRotation = Random.insideUnitSphere * Random.Range(0.05f, 0.01f);

        if (staticRotation)
        {
            _orbitRotation = rotation;
        }
        else
        {
            _thisTransform.rotation = Random.rotation;
            _orbitRotation = Random.insideUnitSphere * Random.Range(0.03f, 0.01f);
        }
    }

    private void Update()
    {
        transform.Rotate(_orbitRotation);
        _planet.Rotate(_planetRotation);
    }
}
