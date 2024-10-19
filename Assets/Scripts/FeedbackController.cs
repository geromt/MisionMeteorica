using TMPro;
using UnityEngine;

public class FeedbackController : MonoBehaviour
{
    public delegate void FeedbackEvent(int aciertos, int audioClipIndex);
    public static event FeedbackEvent OnMessageSelected;

    [SerializeField] private TMP_Text mensajeFinal;

    private string[] _mensajesAnimo = new string[] { 
        "¡No te rindas!", 
        "¡Tú puedes!", 
        "¡No te desanimes!" 
    };
    private string[] _mensajesSeguir = new string[] {
        "¡Sigue así!",
        "¡Continúa así!",
        "¡Bien hecho!"
    };
    private string[] _mensajesFelicitaciones = new string[] {
        "¡Lo has hecho muy bien!",
        "¡Muy bien hecho!",
        "¡Excelente!"
    };

    private void OnEnable()
    {
        AdminController.OnAciertosDePartidaEnviados += ShowFinalMessage;
        Button3DController.OnContinueGameClick += HideFinalMessage;
    }

    private void OnDisable()
    {
        AdminController.OnAciertosDePartidaEnviados -= ShowFinalMessage;
        Button3DController.OnContinueGameClick -= HideFinalMessage;
    }

    private void Awake()
    {
        mensajeFinal.gameObject.SetActive(false);
    }

    private void ShowFinalMessage(int aciertos)
    {
        if (aciertos < 5)
        {
            var i = Random.Range(0, _mensajesAnimo.Length);
            mensajeFinal.text = _mensajesAnimo[i];
            OnMessageSelected?.Invoke(aciertos, i);
        }
        else if (aciertos > 7)
        {
            var i = Random.Range(0, _mensajesFelicitaciones.Length);
            mensajeFinal.text = _mensajesFelicitaciones[i];
            OnMessageSelected?.Invoke(aciertos, i);
        }
        else
        {
            var i = Random.Range(0, _mensajesSeguir.Length);
            mensajeFinal.text = _mensajesSeguir[i];
            OnMessageSelected?.Invoke(aciertos, i);
        }

        mensajeFinal.gameObject.SetActive(true);
    }

    private void HideFinalMessage()
    {
        mensajeFinal.gameObject.SetActive(false);
    }
}
