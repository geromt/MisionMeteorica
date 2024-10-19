using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class PanelSalirController : MonoBehaviour
{
    public delegate void SalirEvent();
    public static event SalirEvent OnShowConfirmaSalirPanel;


    [SerializeField] private Button salirButton;
    [SerializeField] private GameObject confirmaSalirPanel;
    [SerializeField] private Tipo tipo = Tipo.TerminaJuego;


    private Image _confirmaSalirImage;
    private GameObject _confirmaSalirPanelChild;
    private Button _siConfirmaButton;
    private Button _noConfirmaButton;
    private Color _blackTransparentColor;

    [Serializable]
    public enum Tipo 
    { 
        TerminaRonda, 
        TerminaJuego, 
        TerminaAntesRonda //Para cuando sale durante el tutorial o mietras se activa la webcam
    }

    private void OnEnable()
    {
        AdminController.OnRutineStart += ChangeToRutine;
    }

    private void OnDisable()
    {
        AdminController.OnRutineStart -= ChangeToRutine;
    }


    private void Awake()
    {
        _blackTransparentColor = new Color(0, 0, 0, 0);

        confirmaSalirPanel.SetActive(false);
        _confirmaSalirImage = confirmaSalirPanel.GetComponent<Image>();
        _confirmaSalirImage.color = _blackTransparentColor;
        _confirmaSalirPanelChild = confirmaSalirPanel.transform.GetChild(0).gameObject;
        _confirmaSalirPanelChild.transform.localScale = new Vector3(1, 0, 1);
        _siConfirmaButton = _confirmaSalirPanelChild.transform.GetChild(1).GetComponent<Button>();
        _noConfirmaButton = _confirmaSalirPanelChild.transform.GetChild(2).GetComponent<Button>();

        salirButton.onClick.RemoveAllListeners();
        salirButton.onClick.AddListener(ShowConfirmaSalirPanel);
        _noConfirmaButton.onClick.RemoveAllListeners();
        _noConfirmaButton.onClick.AddListener(HideConfirmaSalirPanel);
        _siConfirmaButton.onClick.RemoveAllListeners();
        _siConfirmaButton.onClick.AddListener(TerminarOnClick);
    }

    private void ShowConfirmaSalirPanel()
    {
        if (tipo == Tipo.TerminaRonda || tipo == Tipo.TerminaAntesRonda)
            Time.timeScale = 0;
        confirmaSalirPanel.SetActive(true);
        LeanTween.value(gameObject, (color) => _confirmaSalirImage.color = color, _blackTransparentColor, Color.black, 0.25f).setIgnoreTimeScale(true).setOnComplete(
            () => LeanTween.scaleY(_confirmaSalirPanelChild, 1, 0.25f).setIgnoreTimeScale(true).setEase(LeanTweenType.easeInOutBounce));
        OnShowConfirmaSalirPanel?.Invoke();
    }

    private void HideConfirmaSalirPanel()
    {
        if (tipo == Tipo.TerminaRonda || tipo == Tipo.TerminaAntesRonda)
            Time.timeScale = 1;
        LeanTween.scaleY(_confirmaSalirPanelChild, 0, 0.25f).setEase(LeanTweenType.easeInOutBounce).setOnComplete(
            () => LeanTween.value(gameObject, (color) => _confirmaSalirImage.color = color, Color.black, _blackTransparentColor, 0.25f).setOnComplete(
                () => confirmaSalirPanel.SetActive(false)));
    }

    private void TerminarOnClick()
    {
        Debug.Log(tipo);
        switch (tipo)
        {
            case Tipo.TerminaJuego:
                Salir();
                break;
            case Tipo.TerminaRonda:
                TerminarRonda();
                break;
            case Tipo.TerminaAntesRonda:
                TerminarAntesRonda();
                break;
        }
    }

    private void ChangeToRutine()
    {
        if (tipo == Tipo.TerminaAntesRonda)
            tipo = Tipo.TerminaRonda;
    }

    public void Salir()
    {
        GameMaster.ResetSerialPortLeft();
        GameMaster.ResetSerialPortRight();
        GameMaster.CloseUDPClient();

        Process[] handTrackerProcesses = Process.GetProcessesByName("LANRBodyTracker");
        foreach (Process p in handTrackerProcesses)
        {
            Debug.Log("Terminando Proceso: " + p.Id);
            p.Kill();
        }
        Debug.Log("Termina aplicacion");
        Application.Quit();
    }

    private void TerminarRonda()
    {
        Time.timeScale = 1;
        Debug.Log("Termina juego");
        SceneManager.LoadScene("ResultsScene");
    }

    private void TerminarAntesRonda()
    {
        Time.timeScale = 1;
        Debug.Log("Termina antes de que comience la ronda");
        SceneManager.LoadScene("MenuScene");
    }
}
