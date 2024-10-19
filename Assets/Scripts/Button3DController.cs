using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Button3DController : MonoBehaviour
{
    public delegate void ButtonEvent();
    public static event ButtonEvent OnGameStartClick;
    public static event ButtonEvent OnGameEndClick;
    public static event ButtonEvent OnContinueGameClick;

    [SerializeField] private Renderer buttonRenderer;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private string triggerText;
    [SerializeField] private int secToWait = 3;
    [SerializeField][ColorUsage(true, true)] private Color touchingMainColor;
    [SerializeField][ColorUsage(true, true)] private Color touchingFresnelColor;
    [SerializeField] private Calls call;

    [Serializable]
    public enum Calls { Otra, ComenzarJuego, Continuar, SeguirJugando }
    public Action otherMethod;

    private Color _mainColor;
    private Color _fresnelColor;
    private string _defaultText;

    private void Awake()
    {
        if (GameMaster.CalidadGraficos == Graficos.Altos)
        {
            _fresnelColor = buttonRenderer.material.GetColor("_FresnelColor");
            _mainColor = buttonRenderer.material.GetColor("_MainColor");
        }
        else if (GameMaster.CalidadGraficos == Graficos.Bajos)
        {
            _mainColor = buttonRenderer.material.color;
        }
        
        _defaultText = buttonText.text;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("mano"))
        {
            ChangeTouchingColors();
            StartCoroutine(BeginCountDown());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("mano"))
        {
            ChangeMainColors();
            StopAllCoroutines();
            buttonText.text = _defaultText;
        }
    }

    IEnumerator BeginCountDown()
    {
        for (int i = secToWait; i >= 1; i--)
        {
            buttonText.text = string.Format("{0}\n{1}", triggerText, i);
            yield return new WaitForSeconds(1f);
        }

        if (call == Calls.Continuar)
            CallContinuar();
        else if (call == Calls.SeguirJugando)
            CallSeguirJugando();
        else if (call == Calls.ComenzarJuego)
            CallComenzarJuego();
        else if (call == Calls.Otra)
            otherMethod?.Invoke();

        ChangeMainColors();
        buttonText.text = _defaultText;
    }

    public void CallComenzarJuego()
    {
        OnGameStartClick?.Invoke();
    }

    public void CallContinuar()
    {
        OnGameEndClick?.Invoke();
    }

    public void CallSeguirJugando()
    {
        buttonText.text = _defaultText;
        ChangeMainColors();

        OnContinueGameClick?.Invoke();
        transform.parent.gameObject.SetActive(false);
    }

    private void ChangeTouchingColors()
    {
        if (GameMaster.CalidadGraficos == Graficos.Altos)
        {
            buttonRenderer.material.SetColor("_MainColor", touchingMainColor);
            buttonRenderer.material.SetColor("_FresnelColor", touchingFresnelColor);
        }
        else if (GameMaster.CalidadGraficos == Graficos.Bajos)
        {
            buttonRenderer.material.color = touchingMainColor;
        }
    }

    private void ChangeMainColors()
    {
        if (GameMaster.CalidadGraficos == Graficos.Altos)
        {
            buttonRenderer.material.SetColor("_MainColor", _mainColor);
            buttonRenderer.material.SetColor("_FresnelColor", _fresnelColor);
        }
        else if (GameMaster.CalidadGraficos == Graficos.Bajos)
        {
            buttonRenderer.material.color = _mainColor;
        }
    }
}
