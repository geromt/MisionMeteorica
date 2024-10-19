using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Se debe agregar este script como componente de todos los botones u otros objetos con comportamientos similares
/// </summary>
public class ButtonTween : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public delegate void ButtonEvent();
    public static event ButtonEvent EnterButton;

    public float xScale = 1.1f;
    public float yScale = 1.1f;
    public float zScale = 1;
    public bool setInitScale = false;
    public Vector3 initScale;

    private int _moveXLoopTweenId;
    private Vector3 _initPosition;

    /// <summary>
    /// Escala este objeto cuando el cursor se encuentra encima de él
    /// </summary>
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (GameMaster.CalidadGraficos == Graficos.Bajos)
            return;

        _initPosition = transform.localPosition;
        if (!setInitScale)
            initScale = transform.localScale;
        LeanTween.scale(this.gameObject, new Vector3(xScale, yScale, zScale), 0.3f).setEase(LeanTweenType.easeOutCirc).setIgnoreTimeScale(true);
        EnterButton?.Invoke();
    }

    /// <summary>
    /// Escala este objeto a sus valores originales cuando cursor sale de él
    /// </summary>
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        LeanTween.scale(this.gameObject, initScale, 0.3f).setEase(LeanTweenType.easeOutCirc).setIgnoreTimeScale(true);
    }

    /// <summary>
    /// Animación que hace que desplaza el objeto hacia un lado, depediendo del valor de distance, regresa a su posición original
    /// y lo repite en un loop.
    /// Se utiliza en los botones para cambiar de sección en las escenas de Clínica y Paciente
    /// </summary>
    public void MoveXLoop(float distance)
    {
        _initPosition = GetComponent<Transform>().localPosition;
        initScale = GetComponent<Transform>().localScale;
        Vector3 finalPos = GetComponent<Transform>().localPosition;
        finalPos.x += distance;
        _moveXLoopTweenId = LeanTween.moveLocal(this.gameObject, finalPos, 0.3f).setEase(LeanTweenType.easeInOutCirc).setLoopPingPong().id;
    }

    public void VibrateALittle()
    {
        float duration = 0.08f;

        LeanTween.rotateAround(this.gameObject, Vector3.forward, 10f, duration);
        LeanTween.rotateAround(this.gameObject, Vector3.forward, -20f, duration * 2).setDelay(duration);
        LeanTween.rotateAround(this.gameObject, Vector3.forward, 20f, duration * 2).setDelay(duration * 3);
        LeanTween.rotateAround(this.gameObject, Vector3.forward, -10f, duration).setDelay(duration * 5);
    }

    /// <summary>
    /// Regresa el objeto a su posición y escala original. Se usa en casos donde se desactiva el objeto antes de que terminen las
    /// animaciones
    /// <summary> 
    public void CancelTween()
    {
        LeanTween.cancel(this.gameObject, _moveXLoopTweenId);
        LeanTween.moveLocal(this.gameObject, _initPosition, 0.1f);
        GetComponent<Transform>().localScale = initScale;
    }
}