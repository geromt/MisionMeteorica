using UnityEngine;

public class RotatePlanet : MonoBehaviour
{
    [SerializeField] private float time;
    [SerializeField] private bool initRandomPos;

    private LTDescr _rotateTween;

    private void OnEnable()
    {
        SelectGameQuality.ChangeQuality += InitRotation;
    }

    private void OnDisable()
    {
        SelectGameQuality.ChangeQuality -= InitRotation;
    }

    private void Start()
    {
        if (initRandomPos)
            transform.localPosition = CalcRandomPos(Vector3.zero, 0.5f);
        _rotateTween = LeanTween.rotateAroundLocal(gameObject, Vector3.up, 360, time).setLoopClamp();

        InitRotation();
    }

    private void InitRotation()
    {
        if (GameMaster.CalidadGraficos == Graficos.Altos)
            PlayRotate();
        else if (GameMaster.CalidadGraficos == Graficos.Bajos)
            StopRotate();
    }

    private void StopRotate()
    {
        _rotateTween.pause();
    }

    private void PlayRotate()
    {
        _rotateTween.resume();
    }

    /// <summary>
    /// Calcula una posicion random dentro del perimetro del circulo con centro y radio indicados. 
    /// </summary>
    /// <returns>Regresa un Vector3 donde x, z indican una posicion random dentro del circulo indicado. La coordenada y es la misma que la del centro</returns>
    private Vector3 CalcRandomPos(Vector3 center, float radius)
    {
        var z = Random.Range(-radius, +radius);
        var x = center.x + Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(z - center.z, 2));
        x = (Random.value > 0.5f) ? x : -x;

        return new Vector3(x, center.y, z);
    }
}
