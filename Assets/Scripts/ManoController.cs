using UnityEngine;

public class ManoController : MonoBehaviour
{
    [SerializeField] private Material touchingLineMaterial;

    private Material _originalLineMaterial;

    // variables de prueba
    //private float _maxX = float.NegativeInfinity;
    //private float _minX = float.PositiveInfinity;
    //private float _maxY = float.NegativeInfinity;
    //private float _minY = float.PositiveInfinity;

    //private void Update()
    //{
    //    if (!name.Contains("Right")) return;

    //    var pos = transform.position;
    //    if (pos.z > -1.2f) return;
    //    if (pos.x > -0.91f) return;
    //    var x = pos.x;
    //    var y = pos.y;

    //    if (x < _minX) _minX = x;
    //    if (x > _maxX) _maxX = x;
    //    if (y < _minY) _minY = y;
    //    if (y > _maxY) _maxY = y;

    //    Debug.Log("Mano: " + name + " minX: " + _minX + " maxX: " + _maxX + " minY: " + _minY + " maxY: " + _maxY);
    //}


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("asteroidLine"))
        {
            if (_originalLineMaterial is null)
                _originalLineMaterial = other.GetComponent<MeshRenderer>().sharedMaterial;
            other.GetComponent<MeshRenderer>().sharedMaterial = touchingLineMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("asteroidLine"))
        {
            other.GetComponent<MeshRenderer>().sharedMaterial = _originalLineMaterial;
        }
    }
}
